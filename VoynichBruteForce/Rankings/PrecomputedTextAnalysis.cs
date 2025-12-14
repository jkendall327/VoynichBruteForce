using System.Buffers;

namespace VoynichBruteForce.Rankings;

/// <summary>
/// Pre-computes and caches text analysis data using span-based operations
/// to minimize allocations. Implements IDisposable to return pooled arrays.
/// </summary>
public sealed class PrecomputedTextAnalysis : IDisposable
{
    // Backing storage (owned, from ArrayPool)
    private char[]? _textBuffer;
    private readonly int _textLength;

    // Cleaned text (whitespace removed, lowercased) - also pooled
    private char[]? _cleanedBuffer;
    private int _cleanedLength = -1; // -1 means not computed yet

    // Word boundaries (pooled)
    private Range[]? _wordRanges;
    private int _wordCount = -1; // -1 means not computed yet

    // Lazy-computed data
    private SpanWordFrequencyMap? _wordFrequencies;

    private bool _disposed;

    /// <summary>
    /// Creates analysis from a span (avoids .ToString()).
    /// Copies to a new pooled array.
    /// </summary>
    public PrecomputedTextAnalysis(ReadOnlySpan<char> text)
    {
        _textLength = text.Length;

        if (_textLength == 0)
        {
            _textBuffer = null;

            return;
        }

        // Rent and copy the text
        _textBuffer = ArrayPool<char>.Shared.Rent(_textLength);
        text.CopyTo(_textBuffer);
    }

    /// <summary>
    /// Legacy constructor for backward compatibility with string input.
    /// </summary>
    public PrecomputedTextAnalysis(string text) : this(text.AsSpan())
    {
    }

    /// <summary>
    /// Read-only view of the original text.
    /// </summary>
    public ReadOnlySpan<char> TextSpan => _textBuffer.AsSpan(0, _textLength);

    /// <summary>
    /// Read-only memory for passing to components that need Memory&lt;char&gt;.
    /// </summary>
    public ReadOnlyMemory<char> TextMemory => _textBuffer.AsMemory(0, _textLength);

    /// <summary>
    /// Total character count of original text.
    /// </summary>
    public int Length => _textLength;

    /// <summary>
    /// Number of words in the text.
    /// </summary>
    public int WordCount
    {
        get
        {
            EnsureWordsParsed();

            return _wordCount;
        }
    }

    /// <summary>
    /// Gets word boundaries for span-based word access.
    /// </summary>
    public WordBoundaries Words
    {
        get
        {
            EnsureWordsParsed();

            return new WordBoundaries(_wordRanges!, _wordCount);
        }
    }

    /// <summary>
    /// Span view of cleaned text (whitespace removed, lowercased).
    /// </summary>
    public ReadOnlySpan<char> CleanedTextSpan
    {
        get
        {
            EnsureCleanedTextComputed();

            return _cleanedBuffer.AsSpan(0, _cleanedLength);
        }
    }

    /// <summary>
    /// Length of cleaned text.
    /// </summary>
    public int CleanedTextLength
    {
        get
        {
            EnsureCleanedTextComputed();

            return _cleanedLength;
        }
    }

    /// <summary>
    /// Character frequencies (excluding whitespace, lowercased).
    /// </summary>
    public Dictionary<char, int> CharFrequencies => field ??= ComputeCharFrequencies();

    /// <summary>
    /// Hash-based word frequency map.
    /// </summary>
    public SpanWordFrequencyMap WordFrequencyMap
    {
        get
        {
            if (_wordFrequencies == null)
            {
                EnsureWordsParsed();
                _wordFrequencies = ComputeWordFrequencies();
            }

            return _wordFrequencies;
        }
    }

    /// <summary>
    /// Bigram data for entropy calculations.
    /// </summary>
    public BigramData Bigrams => field ??= ComputeBigramData();

    // ===== Private computation methods =====

    private void EnsureWordsParsed()
    {
        if (_wordCount >= 0)
        {
            return; // Already computed
        }

        if (_textLength == 0)
        {
            _wordRanges = Array.Empty<Range>();
            _wordCount = 0;

            return;
        }

        // Estimate: at most textLength/2 words (every other char is space)
        var maxWords = (_textLength + 1) / 2;
        _wordRanges = ArrayPool<Range>.Shared.Rent(maxWords);

        var text = TextSpan;
        var wordStart = -1;
        var wordIdx = 0;

        for (var i = 0; i <= text.Length; i++)
        {
            var isWordChar = i < text.Length && !char.IsWhiteSpace(text[i]);

            if (isWordChar && wordStart < 0)
            {
                wordStart = i;
            }
            else if (!isWordChar && wordStart >= 0)
            {
                _wordRanges[wordIdx++] = new Range(wordStart, i);
                wordStart = -1;
            }
        }

        _wordCount = wordIdx;
    }

    private void EnsureCleanedTextComputed()
    {
        if (_cleanedLength >= 0)
        {
            return; // Already computed
        }

        if (_textLength == 0)
        {
            _cleanedBuffer = Array.Empty<char>();
            _cleanedLength = 0;

            return;
        }

        // Count non-whitespace
        var text = TextSpan;
        var count = 0;

        foreach (var c in text)
        {
            if (!char.IsWhiteSpace(c))
            {
                count++;
            }
        }

        if (count == 0)
        {
            _cleanedBuffer = Array.Empty<char>();
            _cleanedLength = 0;

            return;
        }

        _cleanedBuffer = ArrayPool<char>.Shared.Rent(count);
        var idx = 0;

        foreach (var c in text)
        {
            if (!char.IsWhiteSpace(c))
            {
                _cleanedBuffer[idx++] = char.ToLowerInvariant(c);
            }
        }

        _cleanedLength = count;
    }

    private Dictionary<char, int> ComputeCharFrequencies()
    {
        var frequencies = new Dictionary<char, int>();

        if (_textLength == 0)
        {
            return frequencies;
        }

        foreach (var c in TextSpan)
        {
            if (char.IsWhiteSpace(c))
            {
                continue;
            }

            var normalized = char.ToLowerInvariant(c);

            if (frequencies.TryGetValue(normalized, out var existing))
            {
                frequencies[normalized] = existing + 1;
            }
            else
            {
                frequencies[normalized] = 1;
            }
        }

        return frequencies;
    }

    private SpanWordFrequencyMap ComputeWordFrequencies()
    {
        // Estimate unique words as sqrt of total words (empirical heuristic)
        var estimatedUnique = Math.Max(16, (int)Math.Sqrt(_wordCount) * 2);
        var map = new SpanWordFrequencyMap(TextMemory, estimatedUnique);

        var ranges = _wordRanges.AsSpan(0, _wordCount);

        foreach (var range in ranges)
        {
            map.AddOrIncrement(range);
        }

        return map;
    }

    private BigramData ComputeBigramData()
    {
        EnsureCleanedTextComputed();

        var bigramCounts = new Dictionary<char, Dictionary<char, int>>();
        var prevCharCounts = new Dictionary<char, int>();

        if (_cleanedLength < 2)
        {
            return new BigramData(bigramCounts, prevCharCounts, _cleanedLength);
        }

        var cleaned = CleanedTextSpan;

        for (var i = 0; i < cleaned.Length - 1; i++)
        {
            var prevChar = cleaned[i];
            var currChar = cleaned[i + 1];

            if (!bigramCounts.TryGetValue(prevChar, out var inner))
            {
                inner = new Dictionary<char, int>();
                bigramCounts[prevChar] = inner;
            }

            if (inner.TryGetValue(currChar, out var count))
            {
                inner[currChar] = count + 1;
            }
            else
            {
                inner[currChar] = 1;
            }

            if (prevCharCounts.TryGetValue(prevChar, out var prevCount))
            {
                prevCharCounts[prevChar] = prevCount + 1;
            }
            else
            {
                prevCharCounts[prevChar] = 1;
            }
        }

        return new BigramData(bigramCounts, prevCharCounts, _cleanedLength);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (_textBuffer != null)
        {
            ArrayPool<char>.Shared.Return(_textBuffer);
            _textBuffer = null;
        }

        if (_cleanedBuffer is { Length: > 0 })
        {
            ArrayPool<char>.Shared.Return(_cleanedBuffer);
            _cleanedBuffer = null;
        }

        if (_wordRanges is { Length: > 0 })
        {
            ArrayPool<Range>.Shared.Return(_wordRanges);
            _wordRanges = null;
        }

        _wordFrequencies?.Dispose();
    }
}

/// <summary>
/// Pre-computed bigram data for H2 entropy calculation.
/// </summary>
public sealed class BigramData
{
    public BigramData(Dictionary<char, Dictionary<char, int>> bigramCounts,
        Dictionary<char, int> prevCharCounts,
        int cleanedTextLength)
    {
        BigramCounts = bigramCounts;
        PrevCharCounts = prevCharCounts;
        CleanedTextLength = cleanedTextLength;
    }

    /// <summary>
    /// Bigram frequency table: outer key = previous char, inner key = current char, value = count.
    /// </summary>
    public Dictionary<char, Dictionary<char, int>> BigramCounts { get; }

    /// <summary>
    /// Count of how many times each character appears as the "previous" character in a bigram.
    /// </summary>
    public Dictionary<char, int> PrevCharCounts { get; }

    /// <summary>
    /// Length of the cleaned (whitespace-removed, lowercased) text.
    /// </summary>
    public int CleanedTextLength { get; }

    /// <summary>
    /// Total number of bigrams (cleanedTextLength - 1).
    /// </summary>
    public int TotalBigrams => Math.Max(0, CleanedTextLength - 1);
}
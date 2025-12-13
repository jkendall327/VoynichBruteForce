namespace VoynichBruteForce.Rankings;

/// <summary>
/// Pre-computes and caches text analysis data to avoid redundant parsing across multiple rankers.
/// All properties are lazily initialized on first access.
/// </summary>
public sealed class PrecomputedTextAnalysis
{
    private static readonly char[] WhitespaceSeparators = [' ', '\t', '\n', '\r'];

    private readonly string _text;

    public PrecomputedTextAnalysis(string text) => _text = text;

    /// <summary>
    /// The original text.
    /// </summary>
    public string Text => _text;

    /// <summary>
    /// Words split by whitespace, with empty entries removed.
    /// Used by: NeighboringWordSimilarityRanker, RepeatedAdjacentWordsRanker, VocabularySizeRanker
    /// </summary>
    public string[] Words => field ??= ComputeWords();

    /// <summary>
    /// Text with whitespace removed and lowercased.
    /// Used by: ConditionalEntropyRanker
    /// </summary>
    public string CleanedText => field ??= ComputeCleanedText();

    /// <summary>
    /// Frequency of each character (excluding whitespace, lowercased).
    /// Used by: SingleCharEntropyRanker
    /// </summary>
    public Dictionary<char, int> CharFrequencies => field ??= ComputeCharFrequencies();

    /// <summary>
    /// Word frequency map (case-insensitive).
    /// Used by: ZipfLawRanker, WordLengthFrequencyRanker, VocabularySizeRanker
    /// </summary>
    public Dictionary<string, int> WordFrequencies => field ??= ComputeWordFrequencies();

    /// <summary>
    /// Bigram analysis data for H2 entropy calculation.
    /// Used by: ConditionalEntropyRanker
    /// </summary>
    public BigramData Bigrams => field ??= ComputeBigramData();

    private string[] ComputeWords()
    {
        return _text.Split(WhitespaceSeparators, StringSplitOptions.RemoveEmptyEntries);
    }

    private string ComputeCleanedText()
    {
        // Count non-whitespace characters
        var count = 0;
        foreach (var c in _text)
        {
            if (!char.IsWhiteSpace(c))
                count++;
        }

        if (count == 0)
            return string.Empty;

        // Allocate exact size and fill with lowercased chars
        Span<char> buffer = count <= 256 ? stackalloc char[count] : new char[count];
        var index = 0;
        foreach (var c in _text)
        {
            if (!char.IsWhiteSpace(c))
                buffer[index++] = char.ToLowerInvariant(c);
        }

        return new string(buffer);
    }

    private Dictionary<char, int> ComputeCharFrequencies()
    {
        var frequencies = new Dictionary<char, int>();

        foreach (char c in _text)
        {
            if (char.IsWhiteSpace(c))
                continue;

            char normalized = char.ToLowerInvariant(c);
            if (frequencies.TryGetValue(normalized, out var existing))
                frequencies[normalized] = existing + 1;
            else
                frequencies[normalized] = 1;
        }

        return frequencies;
    }

    private Dictionary<string, int> ComputeWordFrequencies()
    {
        var frequencies = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var words = Words;

        foreach (var word in words)
        {
            if (frequencies.TryGetValue(word, out var existing))
                frequencies[word] = existing + 1;
            else
                frequencies[word] = 1;
        }

        return frequencies;
    }

    private BigramData ComputeBigramData()
    {
        var cleanedText = CleanedText;
        var bigramCounts = new Dictionary<char, Dictionary<char, int>>();
        var prevCharCounts = new Dictionary<char, int>();

        if (cleanedText.Length < 2)
            return new BigramData(bigramCounts, prevCharCounts, cleanedText.Length);

        for (int i = 0; i < cleanedText.Length - 1; i++)
        {
            char prevChar = cleanedText[i];
            char currChar = cleanedText[i + 1];

            // Count bigrams
            if (!bigramCounts.TryGetValue(prevChar, out var inner))
            {
                inner = new Dictionary<char, int>();
                bigramCounts[prevChar] = inner;
            }

            if (inner.TryGetValue(currChar, out var count))
                inner[currChar] = count + 1;
            else
                inner[currChar] = 1;

            // Count previous character occurrences (for conditional probability)
            if (prevCharCounts.TryGetValue(prevChar, out var prevCount))
                prevCharCounts[prevChar] = prevCount + 1;
            else
                prevCharCounts[prevChar] = 1;
        }

        return new BigramData(bigramCounts, prevCharCounts, cleanedText.Length);
    }
}

/// <summary>
/// Pre-computed bigram data for H2 entropy calculation.
/// </summary>
public sealed class BigramData
{
    public BigramData(
        Dictionary<char, Dictionary<char, int>> bigramCounts,
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

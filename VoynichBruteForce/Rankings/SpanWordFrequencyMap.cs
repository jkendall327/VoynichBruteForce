using System.Buffers;

namespace VoynichBruteForce.Rankings;

/// <summary>
/// Hash-based word frequency counting without string allocations.
/// Uses case-insensitive span hashing with collision handling via full span comparison.
/// </summary>
public sealed class SpanWordFrequencyMap : IDisposable
{
    private struct Entry
    {
        public Range WordRange;
        public int HashCode;
        public int Frequency;
        public int Next; // Index of next entry in collision chain, -1 if none
    }

    private Entry[] _entries;
    private int[] _buckets; // Maps hash bucket -> entry index (-1 = empty)
    private int _count;
    private readonly int _bucketCount;
    private bool _disposed;

    // Reference to the backing text for span comparisons
    private readonly ReadOnlyMemory<char> _text;

    public SpanWordFrequencyMap(ReadOnlyMemory<char> text, int estimatedUniqueWords)
    {
        _text = text;
        _bucketCount = HashHelpers.GetPrime(Math.Max(estimatedUniqueWords, 16));

        _entries = ArrayPool<Entry>.Shared.Rent(_bucketCount);
        _buckets = ArrayPool<int>.Shared.Rent(_bucketCount);

        // Initialize buckets to -1 (empty)
        Array.Fill(_buckets, -1, 0, _bucketCount);
        _count = 0;
    }

    /// <summary>
    /// The number of unique words counted.
    /// </summary>
    public int UniqueWordCount => _count;

    /// <summary>
    /// The total number of words processed (sum of all frequencies).
    /// </summary>
    public int TotalWordCount
    {
        get
        {
            int total = 0;
            for (int i = 0; i < _count; i++)
            {
                total += _entries[i].Frequency;
            }
            return total;
        }
    }

    /// <summary>
    /// Adds or increments count for a word span.
    /// </summary>
    public void AddOrIncrement(Range wordRange)
    {
        var word = _text.Span[wordRange];
        var hash = string.GetHashCode(word, StringComparison.OrdinalIgnoreCase);
        var bucketIndex = (hash & 0x7FFFFFFF) % _bucketCount;

        // Search existing entries in this bucket
        var entryIndex = _buckets[bucketIndex];
        while (entryIndex >= 0)
        {
            ref var entry = ref _entries[entryIndex];
            if (entry.HashCode == hash &&
                word.Equals(_text.Span[entry.WordRange], StringComparison.OrdinalIgnoreCase))
            {
                // Found existing word - increment frequency
                entry.Frequency++;
                return;
            }
            entryIndex = entry.Next;
        }

        // Not found - add new entry
        if (_count >= _entries.Length)
        {
            // Need to grow the entries array
            GrowEntries();
        }

        var newIndex = _count++;
        ref var newEntry = ref _entries[newIndex];
        newEntry.WordRange = wordRange;
        newEntry.HashCode = hash;
        newEntry.Frequency = 1;
        newEntry.Next = _buckets[bucketIndex];
        _buckets[bucketIndex] = newIndex;
    }

    private void GrowEntries()
    {
        var newCapacity = _entries.Length * 2;
        var newEntries = ArrayPool<Entry>.Shared.Rent(newCapacity);
        Array.Copy(_entries, newEntries, _count);
        ArrayPool<Entry>.Shared.Return(_entries);
        _entries = newEntries;
    }

    /// <summary>
    /// Gets sorted frequencies (descending) for Zipf's law calculation.
    /// Returns a rented array that caller must return to ArrayPool.
    /// </summary>
    public (int[] Frequencies, int Count) GetSortedFrequencies()
    {
        var freqs = ArrayPool<int>.Shared.Rent(_count);
        for (int i = 0; i < _count; i++)
        {
            freqs[i] = _entries[i].Frequency;
        }
        Array.Sort(freqs, 0, _count, Comparer<int>.Create((a, b) => b.CompareTo(a)));
        return (freqs, _count);
    }

    /// <summary>
    /// Populates parallel arrays with word lengths and frequencies for correlation calculation.
    /// Caller must provide arrays of at least UniqueWordCount size.
    /// Returns the count of items written.
    /// </summary>
    public int GetLengthsAndFrequencies(Span<int> lengths, Span<int> frequencies)
    {
        var count = Math.Min(_count, Math.Min(lengths.Length, frequencies.Length));
        for (int i = 0; i < count; i++)
        {
            var range = _entries[i].WordRange;
            lengths[i] = range.End.Value - range.Start.Value;
            frequencies[i] = _entries[i].Frequency;
        }
        return count;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_entries != null)
        {
            ArrayPool<Entry>.Shared.Return(_entries);
            _entries = null!;
        }

        if (_buckets != null)
        {
            ArrayPool<int>.Shared.Return(_buckets);
            _buckets = null!;
        }
    }
}

internal static class HashHelpers
{
    private static readonly int[] Primes =
    [
        3, 7, 11, 17, 23, 29, 37, 47, 59, 71, 89, 107, 131, 163, 197, 239, 293, 353, 431, 521, 631,
        761, 919, 1103, 1327, 1597, 1931, 2333, 2801, 3371, 4049, 4861, 5839, 7013, 8419, 10103,
        12143, 14591, 17519, 21023, 25229, 30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631,
        130363, 156437, 187751, 225307, 270371, 324449
    ];

    public static int GetPrime(int min)
    {
        foreach (var prime in Primes)
        {
            if (prime >= min) return prime;
        }
        return Primes[^1];
    }
}

namespace VoynichBruteForce.Rankings;

/// <summary>
/// Stores word boundary information for zero-allocation word access.
/// Words are stored as Range values that slice into a backing char array.
/// </summary>
public readonly struct WordBoundaries
{
    private readonly Range[] _ranges;
    private readonly int _count;

    public WordBoundaries(Range[] ranges, int count)
    {
        _ranges = ranges;
        _count = count;
    }

    /// <summary>
    /// The number of words.
    /// </summary>
    public int Count => _count;

    /// <summary>
    /// Gets the Range for the word at the given index.
    /// </summary>
    public Range this[int index] => _ranges[index];

    /// <summary>
    /// Gets a span view of the word at the given index.
    /// </summary>
    public ReadOnlySpan<char> GetWord(ReadOnlySpan<char> text, int index)
        => text[_ranges[index]];

    /// <summary>
    /// Returns the underlying ranges for iteration.
    /// </summary>
    public ReadOnlySpan<Range> AsSpan() => _ranges.AsSpan(0, _count);
}

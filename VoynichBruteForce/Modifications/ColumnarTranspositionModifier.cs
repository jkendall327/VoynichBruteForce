using System.Text;

namespace VoynichBruteForce.Modifications;

/// <summary>
/// Applies a columnar transposition cipher. Text is written into rows of a grid,
/// then read out by columns in a specified order.
///
/// Transposition ciphers were known in antiquity (the Spartan scytale) and were
/// well-documented by Renaissance cryptographers. The technique requires only
/// paper/parchment arranged in a grid - no special tools or memorization beyond
/// the column order (key).
/// </summary>
public class ColumnarTranspositionModifier : ITextModifier
{
    private readonly int[] _columnOrder;

    public string Name => $"ColumnarTransposition({string.Join(",", _columnOrder)})";

    // Higher cognitive cost - requires grid layout and careful column reading
    public CognitiveComplexity CognitiveCost => new(6);

    /// <summary>
    /// Creates a columnar transposition cipher.
    /// </summary>
    /// <param name="columnOrder">
    /// The order in which to read columns (0-indexed).
    /// For example, [2,0,1] means read column 2 first, then 0, then 1.
    /// </param>
    public ColumnarTranspositionModifier(int[] columnOrder)
    {
        if (columnOrder.Length == 0)
        {
            throw new ArgumentException("Column order must have at least one element", nameof(columnOrder));
        }

        // Validate that it's a valid permutation
        var sorted = columnOrder.OrderBy(x => x).ToArray();
        for (var i = 0; i < sorted.Length; i++)
        {
            if (sorted[i] != i)
            {
                throw new ArgumentException("Column order must be a valid permutation of 0 to n-1", nameof(columnOrder));
            }
        }

        _columnOrder = columnOrder;
    }

    /// <summary>
    /// Creates a columnar transposition cipher from a keyword.
    /// The column order is determined by alphabetically sorting the keyword letters.
    /// </summary>
    public static ColumnarTranspositionModifier FromKeyword(string keyword)
    {
        if (string.IsNullOrEmpty(keyword))
        {
            throw new ArgumentException("Keyword cannot be empty", nameof(keyword));
        }

        // Determine column order by sorting keyword letters alphabetically
        var indexed = keyword.Select((c, i) => (Char: char.ToUpper(c), Index: i))
            .OrderBy(x => x.Char)
            .ThenBy(x => x.Index)
            .Select((x, newIndex) => (Original: x.Index, NewIndex: newIndex))
            .OrderBy(x => x.NewIndex)
            .Select(x => x.Original)
            .ToArray();

        // We need the inverse - which column to read at each position
        var columnOrder = new int[keyword.Length];
        for (var i = 0; i < indexed.Length; i++)
        {
            columnOrder[indexed[i]] = i;
        }

        // Re-sort by the assigned numbers to get reading order
        var readOrder = Enumerable.Range(0, keyword.Length)
            .OrderBy(i => columnOrder[i])
            .ToArray();

        return new ColumnarTranspositionModifier(readOrder);
    }

    public string ModifyText(string text)
    {
        var numCols = _columnOrder.Length;
        var numRows = (text.Length + numCols - 1) / numCols;

        // Build the grid
        var grid = new char[numRows, numCols];
        var index = 0;

        for (var row = 0; row < numRows; row++)
        {
            for (var col = 0; col < numCols; col++)
            {
                grid[row, col] = index < text.Length ? text[index++] : ' ';
            }
        }

        // Read out by columns in the specified order
        var result = new StringBuilder(text.Length);

        foreach (var col in _columnOrder)
        {
            for (var row = 0; row < numRows; row++)
            {
                var c = grid[row, col];
                if (c != ' ' || result.Length < text.Length)
                {
                    result.Append(c);
                }
            }
        }

        return result.ToString().TrimEnd();
    }
}

namespace VoynichBruteForce.Sources.Asemic;

/// <summary>
/// Generates text by systematically traversing a table (grid) of syllables.
/// Tables and grids were common organizational tools in medieval manuscripts,
/// used for calendrical calculations, astronomical tables, and mnemonic devices.
/// A 15th century scholar could easily prepare a table of syllables and traverse it
/// in various patterns (row-by-row, column-by-column, diagonal, spiral, etc.).
/// This represents a systematic, table-based approach to text generation.
/// </summary>
public class SyllableTableTextProvider : ITextProvider
{
    private readonly string[,] _syllableTable;
    private readonly TraversalPattern _pattern;
    private readonly int _wordCount;
    private readonly int _syllablesPerWord;

    /// <summary>
    /// Defines how the syllable table is traversed.
    /// </summary>
    public enum TraversalPattern
    {
        /// <summary>Left to right, top to bottom (reading order)</summary>
        RowMajor,
        /// <summary>Top to bottom, left to right</summary>
        ColumnMajor,
        /// <summary>Diagonal pattern (top-left to bottom-right)</summary>
        Diagonal,
        /// <summary>Alternating: first row left-to-right, second row right-to-left, etc.</summary>
        Boustrophedon
    }

    /// <summary>
    /// Creates a syllable table provider with a default table of medieval Latin syllables.
    /// </summary>
    public SyllableTableTextProvider() : this(
        syllableTable: CreateDefaultTable(),
        pattern: TraversalPattern.RowMajor,
        wordCount: 100,
        syllablesPerWord: 3)
    {
    }

    /// <summary>
    /// Creates a syllable table provider with custom parameters.
    /// </summary>
    /// <param name="syllableTable">2D array of syllables to traverse</param>
    /// <param name="pattern">Pattern for traversing the table</param>
    /// <param name="wordCount">Number of words to generate</param>
    /// <param name="syllablesPerWord">Number of syllables per word</param>
    public SyllableTableTextProvider(string[,] syllableTable, TraversalPattern pattern, int wordCount, int syllablesPerWord)
    {
        _syllableTable = syllableTable;
        _pattern = pattern;
        _wordCount = wordCount;
        _syllablesPerWord = syllablesPerWord;
    }

    private static string[,] CreateDefaultTable()
    {
        // A 6x6 table of common medieval Latin syllables
        return new string[,]
        {
            { "al", "an", "ar", "as", "at", "ba" },
            { "ca", "da", "de", "di", "do", "ed" },
            { "el", "en", "er", "es", "et", "ex" },
            { "in", "is", "la", "le", "li", "ma" },
            { "na", "ne", "no", "or", "pa", "pe" },
            { "ra", "re", "ri", "ro", "sa", "ta" }
        };
    }

    public string GetText()
    {
        var words = new List<string>(_wordCount);
        var rows = _syllableTable.GetLength(0);
        var cols = _syllableTable.GetLength(1);
        var totalSyllables = rows * cols;
        var currentIndex = 0;

        for (var i = 0; i < _wordCount; i++)
        {
            var syllables = new List<string>(_syllablesPerWord);

            for (var j = 0; j < _syllablesPerWord; j++)
            {
                var (row, col) = GetCoordinates(currentIndex % totalSyllables, rows, cols);
                syllables.Add(_syllableTable[row, col]);
                currentIndex++;
            }

            words.Add(string.Join("", syllables));
        }

        return string.Join(" ", words);
    }

    /// <summary>
    /// Converts a linear index to row/column coordinates based on the traversal pattern.
    /// </summary>
    private (int row, int col) GetCoordinates(int index, int rows, int cols)
    {
        return _pattern switch
        {
            TraversalPattern.RowMajor => (index / cols, index % cols),

            TraversalPattern.ColumnMajor => (index % rows, index / rows),

            TraversalPattern.Diagonal => GetDiagonalCoordinates(index, rows, cols),

            TraversalPattern.Boustrophedon => GetBoustrophedonCoordinates(index, rows, cols),

            _ => (index / cols, index % cols)
        };
    }

    private (int row, int col) GetDiagonalCoordinates(int index, int rows, int cols)
    {
        // Traverse diagonals from top-left to bottom-right
        var diagonal = 0;
        var position = index;

        // Find which diagonal we're on
        while (position >= 0)
        {
            var diagonalLength = Math.Min(diagonal + 1, Math.Min(rows, cols));
            if (diagonal >= rows)
                diagonalLength = Math.Min(cols - (diagonal - rows + 1), rows);
            if (diagonal >= cols)
                diagonalLength = Math.Min(rows - (diagonal - cols + 1), cols);

            if (position < diagonalLength)
                break;

            position -= diagonalLength;
            diagonal++;
        }

        // Calculate position within the diagonal
        var row = diagonal < cols ? position : position + (diagonal - cols + 1);
        var col = diagonal < cols ? diagonal - position : cols - 1 - position;

        return (Math.Min(row, rows - 1), Math.Min(col, cols - 1));
    }

    private (int row, int col) GetBoustrophedonCoordinates(int index, int rows, int cols)
    {
        // Boustrophedon: "as the ox plows" - alternating direction each row
        var row = index / cols;
        var col = index % cols;

        // Reverse column order on odd rows
        if (row % 2 == 1)
            col = cols - 1 - col;

        return (row % rows, col);
    }
}

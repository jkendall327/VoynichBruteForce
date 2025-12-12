namespace VoynichBruteForce.Sources.Asemic;

/// <summary>
/// Generates text using arithmetic progressions to select letters or syllables.
/// Arithmetic sequences (progressions) were well understood in medieval mathematics,
/// appearing in works like Fibonacci's Liber Abaci (1202) and other texts on calculation.
/// A 15th century scholar could easily generate sequences like 1, 3, 5, 7... or 2, 5, 8, 11...
/// and use them to systematically select elements from an alphabet or syllabary.
/// This represents a deterministic, number-based approach to text generation.
/// </summary>
public class ArithmeticSequenceTextProvider : ITextProvider
{
    private readonly string[] _alphabet;
    private readonly int _start;
    private readonly int _step;
    private readonly int _wordCount;
    private readonly int _lettersPerWord;

    /// <summary>
    /// Creates an arithmetic sequence provider with default parameters.
    /// Uses a simple progression (start=0, step=3) mapped to medieval Latin syllables.
    /// </summary>
    public ArithmeticSequenceTextProvider() : this(
        alphabet: new[] { "a", "ba", "ca", "da", "e", "fa", "ga", "ha", "i", "la", "ma", "na",
                         "o", "pa", "ra", "sa", "ta", "u", "va", "xa", "za" },
        start: 0,
        step: 3,
        wordCount: 100,
        lettersPerWord: 4)
    {
    }

    /// <summary>
    /// Creates an arithmetic sequence provider with custom parameters.
    /// </summary>
    /// <param name="alphabet">Array of characters or syllables to select from</param>
    /// <param name="start">Starting position in the sequence</param>
    /// <param name="step">Step size for the arithmetic progression</param>
    /// <param name="wordCount">Number of words to generate</param>
    /// <param name="lettersPerWord">Number of elements per word</param>
    public ArithmeticSequenceTextProvider(string[] alphabet, int start, int step, int wordCount, int lettersPerWord)
    {
        _alphabet = alphabet;
        _start = start;
        _step = step;
        _wordCount = wordCount;
        _lettersPerWord = lettersPerWord;
    }

    public string GetText()
    {
        var words = new List<string>(_wordCount);
        int position = _start;

        for (int i = 0; i < _wordCount; i++)
        {
            var wordParts = new List<string>(_lettersPerWord);

            for (int j = 0; j < _lettersPerWord; j++)
            {
                // Map the current position in the sequence to an alphabet index
                // Use modulo to wrap around the alphabet
                int alphabetIndex = position % _alphabet.Length;
                wordParts.Add(_alphabet[alphabetIndex]);

                // Advance to next position in arithmetic sequence
                position += _step;
            }

            words.Add(string.Join("", wordParts));
        }

        return string.Join(" ", words);
    }
}

namespace VoynichBruteForce.Sources.Asemic;

/// <summary>
/// Generates purely random text by selecting characters at random.
/// This simulates a scholar using dice, drawing lots, or other randomization methods
/// available in 15th century Italy to produce asemic writing.
/// </summary>
public class RandomTextProvider : ITextProvider
{
    private readonly Random _random;
    private readonly char[] _alphabet;
    private readonly int _wordCount;
    private readonly int _minWordLength;
    private readonly int _maxWordLength;

    /// <summary>
    /// Creates a random text provider with default parameters.
    /// Uses a simplified Latin alphabet typical of medieval manuscripts.
    /// </summary>
    public RandomTextProvider(Random random) : this(
        random: random,
        alphabet: "abcdefghilmnopqrstuvxyz", // Medieval Latin alphabet (no j, k, w)
        wordCount: 100,
        minWordLength: 2,
        maxWordLength: 8)
    {
    }

    /// <summary>
    /// Creates a random text provider with custom parameters.
    /// </summary>
    /// <param name="random">Random generator to use</param>
    /// <param name="alphabet">Character set to draw from</param>
    /// <param name="wordCount">Number of words to generate</param>
    /// <param name="minWordLength">Minimum word length</param>
    /// <param name="maxWordLength">Maximum word length</param>
    public RandomTextProvider(Random random, string alphabet, int wordCount, int minWordLength, int maxWordLength)
    {
        _random = random;
        _alphabet = alphabet.ToCharArray();
        _wordCount = wordCount;
        _minWordLength = minWordLength;
        _maxWordLength = maxWordLength;
    }

    public string GetText()
    {
        var words = new List<string>(_wordCount);

        for (int i = 0; i < _wordCount; i++)
        {
            int wordLength = _random.Next(_minWordLength, _maxWordLength + 1);
            var wordChars = new char[wordLength];

            for (int j = 0; j < wordLength; j++)
            {
                wordChars[j] = _random.NextItem(_alphabet);
            }

            words.Add(new string(wordChars));
        }

        return string.Join(" ", words);
    }
}

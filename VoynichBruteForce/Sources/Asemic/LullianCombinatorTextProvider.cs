namespace VoynichBruteForce.Sources.Asemic;

/// <summary>
/// Generates text using a method inspired by Ramon Llull's Ars Combinatoria (13th century).
/// Llull's method involved concentric rotating wheels (or circles) with letters or concepts,
/// which could be systematically combined to generate all possible combinations.
/// This was well-known to Renaissance scholars and represents a purely mechanical,
/// algorithmic approach to text generation that predates the Voynich manuscript.
/// </summary>
public class LullianCombinatorTextProvider : ITextProvider
{
    private readonly string[] _wheel1;
    private readonly string[] _wheel2;
    private readonly string[] _wheel3;
    private readonly int _wordCount;

    /// <summary>
    /// Creates a Lullian combinator with default wheels based on medieval Latin syllables.
    /// </summary>
    public LullianCombinatorTextProvider() : this(
        wheel1: new[] { "a", "e", "i", "o", "u" },
        wheel2: new[] { "b", "c", "d", "f", "g", "l", "m", "n", "p", "r", "s", "t" },
        wheel3: new[] { "a", "e", "i", "o", "u", "ar", "er", "or", "an", "en", "in", "on", "um", "us" },
        wordCount: 100)
    {
    }

    /// <summary>
    /// Creates a Lullian combinator with custom wheels.
    /// </summary>
    /// <param name="wheel1">First wheel of elements (typically vowels or prefixes)</param>
    /// <param name="wheel2">Second wheel of elements (typically consonants or roots)</param>
    /// <param name="wheel3">Third wheel of elements (typically vowels or suffixes)</param>
    /// <param name="wordCount">Number of words to generate</param>
    public LullianCombinatorTextProvider(string[] wheel1, string[] wheel2, string[] wheel3, int wordCount)
    {
        _wheel1 = wheel1;
        _wheel2 = wheel2;
        _wheel3 = wheel3;
        _wordCount = wordCount;
    }

    public string GetText()
    {
        var words = new List<string>(_wordCount);
        int totalCombinations = _wheel1.Length * _wheel2.Length * _wheel3.Length;

        // Generate words by systematically rotating through all combinations
        // This simulates rotating the wheels one position at a time
        for (int i = 0; i < _wordCount; i++)
        {
            // Use modulo arithmetic to cycle through combinations
            // This ensures we systematically cover all possibilities
            int index1 = (i / (_wheel2.Length * _wheel3.Length)) % _wheel1.Length;
            int index2 = (i / _wheel3.Length) % _wheel2.Length;
            int index3 = i % _wheel3.Length;

            string word = _wheel1[index1] + _wheel2[index2] + _wheel3[index3];
            words.Add(word);
        }

        return string.Join(" ", words);
    }
}

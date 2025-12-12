using System.Text;

namespace VoynichBruteForce.Modifications;

/// <summary>
/// Separates text into consonants and vowels, outputting all consonants
/// followed by all vowels (or vice versa).
/// "HELLO" becomes "HLLЕО" (consonants: HLL, vowels: EO).
///
/// The distinction between vowels and consonants was fundamental to
/// classical and medieval grammar. This separation technique requires
/// no tools and leverages basic linguistic knowledge that any educated
/// person would possess.
/// </summary>
public class ConsonantVowelSplitModifier : ITextModifier
{
    private readonly bool _consonantsFirst;

    private static readonly HashSet<char> Vowels = new()
    {
        'a', 'e', 'i', 'o', 'u',
        'A', 'E', 'I', 'O', 'U'
    };

    public string Name => _consonantsFirst ? "ConsonantVowelSplit(C,V)" : "ConsonantVowelSplit(V,C)";

    // Low cognitive cost - simple categorization
    public CognitiveComplexity CognitiveCost => new(2);

    /// <summary>
    /// Creates a consonant-vowel split modifier.
    /// </summary>
    /// <param name="consonantsFirst">If true, consonants come first; otherwise vowels first.</param>
    public ConsonantVowelSplitModifier(bool consonantsFirst = true)
    {
        _consonantsFirst = consonantsFirst;
    }

    public string ModifyText(string text)
    {
        var consonants = new StringBuilder();
        var vowels = new StringBuilder();
        var nonLetters = new List<(int Index, char Char)>();

        var letterIndex = 0;
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (char.IsLetter(c))
            {
                if (Vowels.Contains(c))
                {
                    vowels.Append(c);
                }
                else
                {
                    consonants.Append(c);
                }
                letterIndex++;
            }
            else
            {
                nonLetters.Add((letterIndex, c));
            }
        }

        // Combine based on order preference
        var letters = _consonantsFirst
            ? consonants.ToString() + vowels.ToString()
            : vowels.ToString() + consonants.ToString();

        // Reinsert non-letters at their relative positions
        if (nonLetters.Count == 0)
        {
            return letters;
        }

        var result = new StringBuilder(text.Length);
        var li = 0;
        var ni = 0;

        for (var i = 0; i < text.Length; i++)
        {
            if (ni < nonLetters.Count && nonLetters[ni].Index == li)
            {
                result.Append(nonLetters[ni].Char);
                ni++;
            }
            else if (li < letters.Length)
            {
                result.Append(letters[li]);
                li++;
            }
        }

        // Append any remaining
        while (li < letters.Length)
        {
            result.Append(letters[li++]);
        }

        return result.ToString();
    }
}

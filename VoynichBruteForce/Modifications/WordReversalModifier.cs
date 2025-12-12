using System.Text;

namespace VoynichBruteForce.Modifications;

/// <summary>
/// Reverses each word in the text while preserving word boundaries.
/// "hello world" becomes "olleh dlrow".
///
/// This is a simple transformation that requires no memorization or tools,
/// just careful attention. Mirror writing was famously used by Leonardo da Vinci
/// in the same period. Palindromes and word reversals were popular among
/// Renaissance scholars as puzzles and in mystical/Kabbalistic contexts.
/// </summary>
public class WordReversalModifier : ITextModifier
{
    public string Name => "WordReversal";

    // Moderate cognitive cost - requires attention but no memorization
    public CognitiveComplexity CognitiveCost => new(3);

    public string ModifyText(string text)
    {
        var result = new StringBuilder(text.Length);
        var wordStart = -1;

        for (var i = 0; i <= text.Length; i++)
        {
            var isWordChar = i < text.Length && char.IsLetterOrDigit(text[i]);

            if (isWordChar && wordStart < 0)
            {
                // Starting a new word
                wordStart = i;
            }
            else if (!isWordChar && wordStart >= 0)
            {
                // End of word - reverse and append
                for (var j = i - 1; j >= wordStart; j--)
                {
                    result.Append(text[j]);
                }
                wordStart = -1;

                // Append the non-word character
                if (i < text.Length)
                {
                    result.Append(text[i]);
                }
            }
            else if (!isWordChar && i < text.Length)
            {
                // Non-word character outside a word
                result.Append(text[i]);
            }
        }

        return result.ToString();
    }
}

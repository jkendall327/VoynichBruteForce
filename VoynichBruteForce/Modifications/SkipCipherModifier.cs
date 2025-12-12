using System.Text;

namespace VoynichBruteForce.Modifications;

/// <summary>
/// Extracts every Nth character from the text, wrapping around to collect all characters.
/// With skip=2, "HELLO" becomes "HLOEL" (H,L,O then E,L).
///
/// Skip ciphers (also called decimation ciphers) are a simple form of transposition
/// that requires no tools - just counting. The technique of reading every Nth letter
/// was known in antiquity and could easily be performed by any literate person.
/// </summary>
public class SkipCipherModifier : ITextModifier
{
    private readonly int _skip;

    public string Name => $"SkipCipher({_skip})";

    // Moderate cognitive cost - requires careful counting
    public CognitiveComplexity CognitiveCost => new(4);

    /// <summary>
    /// Creates a skip cipher modifier.
    /// </summary>
    /// <param name="skip">Take every Nth character. Must be at least 2.</param>
    public SkipCipherModifier(int skip)
    {
        if (skip < 2)
        {
            throw new ArgumentException("Skip must be at least 2", nameof(skip));
        }

        _skip = skip;
    }

    public string ModifyText(string text)
    {
        if (text.Length == 0)
        {
            return text;
        }

        var result = new StringBuilder(text.Length);
        var used = new bool[text.Length];
        var index = 0;
        var collected = 0;

        while (collected < text.Length)
        {
            if (!used[index])
            {
                result.Append(text[index]);
                used[index] = true;
                collected++;
            }

            index = (index + _skip) % text.Length;

            // If we've wrapped around and the current position is used,
            // find the next unused position
            if (used[index])
            {
                var startIndex = index;
                do
                {
                    index = (index + 1) % text.Length;
                } while (used[index] && index != startIndex);
            }
        }

        return result.ToString();
    }
}

namespace VoynichBruteForce.Modifications;

/// <summary>
/// Applies the Atbash cipher - a substitution cipher where letters are mapped
/// to their reverse position in the alphabet (A↔Z, B↔Y, C↔X, etc.).
///
/// Originally a Hebrew cipher (used in the Book of Jeremiah), it was well-known
/// to 15th-century Italian scholars through biblical and Kabbalistic studies.
/// The name comes from the first, last, second, and second-to-last Hebrew letters:
/// Aleph-Tav-Beth-Shin.
/// </summary>
public class AtbashCipherModifier : ITextModifier
{
    public string Name => "AtbashCipher";

    // Low cognitive cost - simple reversal of alphabet position
    public CognitiveComplexity CognitiveCost => new(2);

    public string ModifyText(string text)
    {
        var result = new char[text.Length];

        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];

            if (char.IsLetter(c))
            {
                var isUpper = char.IsUpper(c);
                var baseChar = isUpper ? 'A' : 'a';
                // Map 0→25, 1→24, 2→23, etc.
                var reversed = (char)(baseChar + (25 - (c - baseChar)));
                result[i] = reversed;
            }
            else
            {
                result[i] = c;
            }
        }

        return new string(result);
    }
}

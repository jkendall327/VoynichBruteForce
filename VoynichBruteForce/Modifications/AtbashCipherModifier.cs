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
public class AtbashCipherModifier : ISpanTextModifier
{
    public string Name => "AtbashCipher";

    // Low cognitive cost - simple reversal of alphabet position
    public CognitiveComplexity CognitiveCost => new(2);

    public string ModifyText(string text) => this.RunWithContext(text);

    public void Modify(ref ProcessingContext context)
    {
        var input = context.InputSpan;
        var output = context.OutputSpan;

        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];

            if (char.IsLetter(c))
            {
                var isUpper = char.IsUpper(c);
                var baseChar = isUpper ? 'A' : 'a';
                // Map 0→25, 1→24, 2→23, etc.
                var reversed = (char)(baseChar + (25 - (c - baseChar)));
                output[i] = reversed;
            }
            else
            {
                output[i] = c;
            }
        }

        context.Commit(input.Length);
    }
}

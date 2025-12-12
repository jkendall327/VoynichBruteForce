namespace VoynichBruteForce.Modifications;

/// <summary>
/// Applies a Caesar cipher (shift cipher) to alphabetic characters.
/// Known since antiquity and famously used by Julius Caesar.
/// A 15th-century Italian scholar would certainly have known this technique.
/// </summary>
public class CaesarCipherModifier : ITextModifier
{
    private readonly int _shift;

    public string Name => $"CaesarCipher({_shift})";

    // Very low cognitive cost - just counting forward in the alphabet
    public CognitiveComplexity CognitiveCost => new(2);

    public CaesarCipherModifier(int shift)
    {
        // Normalize shift to 0-25 range
        _shift = ((shift % 26) + 26) % 26;
    }

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
                var shifted = (char)(((c - baseChar + _shift) % 26) + baseChar);
                output[i] = shifted;
            }
            else
            {
                output[i] = c;
            }
        }

        context.Commit(input.Length);
    }
}
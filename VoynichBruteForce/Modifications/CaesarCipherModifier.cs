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
                var shifted = (char)(((c - baseChar + _shift) % 26) + baseChar);
                result[i] = shifted;
            }
            else
            {
                result[i] = c;
            }
        }

        return new string(result);
    }
}
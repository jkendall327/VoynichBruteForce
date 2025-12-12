namespace VoynichBruteForce.Modifications;

/// <summary>
/// Applies a polyalphabetic substitution cipher using a keyword (Vigenère-style).
/// Each letter is shifted by an amount determined by the corresponding keyword letter.
///
/// Leon Battista Alberti described the concept of polyalphabetic ciphers in his
/// "De componendis cifris" (1467), and invented the cipher disk. While the full
/// Vigenère tableau was published later (1553), the underlying concept was within
/// reach of a 15th-century cryptographer. The Voynich Manuscript's dating overlaps
/// with Alberti's work.
/// </summary>
public class PolyalphabeticModifier : ITextModifier
{
    private readonly string _keyword;
    private readonly int[] _shifts;

    public string Name => $"Polyalphabetic({_keyword})";

    // High cognitive cost - requires remembering position in keyword and computing shifts
    public CognitiveComplexity CognitiveCost => new(7);

    /// <summary>
    /// Creates a polyalphabetic cipher with the given keyword.
    /// </summary>
    /// <param name="keyword">The keyword determining the shift pattern.</param>
    public PolyalphabeticModifier(string keyword)
    {
        if (string.IsNullOrEmpty(keyword))
        {
            throw new ArgumentException("Keyword cannot be empty", nameof(keyword));
        }

        _keyword = keyword.ToUpperInvariant();
        _shifts = _keyword.Select(c => c - 'A').ToArray();
    }

    public string ModifyText(string text)
    {
        var result = new char[text.Length];
        var keyIndex = 0;

        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];

            if (char.IsLetter(c))
            {
                var isUpper = char.IsUpper(c);
                var baseChar = isUpper ? 'A' : 'a';
                var shift = _shifts[keyIndex % _shifts.Length];
                var shifted = (char)(((c - baseChar + shift) % 26) + baseChar);
                result[i] = shifted;
                keyIndex++;
            }
            else
            {
                result[i] = c;
            }
        }

        return new string(result);
    }
}

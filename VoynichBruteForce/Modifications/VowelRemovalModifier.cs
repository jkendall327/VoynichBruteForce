namespace VoynichBruteForce.Modifications;

/// <summary>
/// Removes vowels from the text.
/// This mirrors common scribal abbreviation practices in medieval manuscripts,
/// where vowels were frequently omitted to save space and parchment.
/// Hebrew and Arabic scripts naturally omit most vowels, and this practice
/// was well-known to Renaissance scholars studying Semitic languages.
/// </summary>
public class VowelRemovalModifier : ITextModifier
{
    private static readonly HashSet<char> Vowels = new()
    {
        'a', 'e', 'i', 'o', 'u',
        'A', 'E', 'I', 'O', 'U'
    };

    public string Name => "VowelRemoval";

    // Low cognitive cost - vowels are easy to identify and skip
    public CognitiveComplexity CognitiveCost => new(1);

    public string ModifyText(string text)
    {
        var result = new char[text.Length];
        var index = 0;

        foreach (var c in text)
        {
            if (!Vowels.Contains(c))
            {
                result[index++] = c;
            }
        }

        return new string(result, 0, index);
    }
}

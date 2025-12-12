namespace VoynichBruteForce.Modifications;

/// <summary>
/// Removes vowels from the text.
/// This mirrors common scribal abbreviation practices in medieval manuscripts,
/// where vowels were frequently omitted to save space and parchment.
/// Hebrew and Arabic scripts naturally omit most vowels, and this practice
/// was well-known to Renaissance scholars studying Semitic languages.
/// </summary>
public class VowelRemovalModifier : ISpanTextModifier
{
    private static readonly HashSet<char> Vowels = new()
    {
        'a', 'e', 'i', 'o', 'u',
        'A', 'E', 'I', 'O', 'U'
    };

    public string Name => "VowelRemoval";

    // Low cognitive cost - vowels are easy to identify and skip
    public CognitiveComplexity CognitiveCost => new(1);

    public string ModifyText(string text) => this.RunWithContext(text);

    public void Modify(ref ProcessingContext context)
    {
        var input = context.InputSpan;
        var output = context.OutputSpan;
        var writeIndex = 0;

        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];
            if (!Vowels.Contains(c))
            {
                output[writeIndex++] = c;
            }
        }

        context.Commit(writeIndex);
    }
}

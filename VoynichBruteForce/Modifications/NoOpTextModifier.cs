namespace VoynichBruteForce.Modifications;

public class NoOpTextModifier : ISpanTextModifier
{
    public string Name => "NoOpTextModifier";
    public CognitiveComplexity CognitiveCost => new(0);

    public string ModifyText(string text) => text;

    public void Modify(ref ProcessingContext context)
    {
        var input = context.InputSpan;
        var output = context.OutputSpan;
        input.CopyTo(output);
        context.Commit(input.Length);
    }
}
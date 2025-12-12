namespace VoynichBruteForce.Modifications;

/// <summary>
/// Applies an algorithmic modification to a source text.
/// Examples include simple substitutions, vowel removals, word shuffling, etc.
/// </summary>
public interface ITextModifier
{
    string Name { get; }

    /// <summary>
    /// Arbitrary estimation of how difficult the rule is to apply in a precomputational world.
    /// </summary>
    CognitiveComplexity CognitiveCost { get; }

    /// <summary>
    /// Modifies text using Span-based ping-pong buffers for zero-allocation processing.
    /// Read from context.InputSpan, write to context.OutputSpan, then call context.Commit(newLength).
    /// </summary>
    void Modify(ref ProcessingContext context);
}

public class NoOpTextModifier : ITextModifier
{
    public string Name => "NoOpTextModifier";
    public CognitiveComplexity CognitiveCost => new(0);

    public void Modify(ref ProcessingContext context)
    {
        var input = context.InputSpan;
        var output = context.OutputSpan;
        input.CopyTo(output);
        context.Commit(input.Length);
    }
}
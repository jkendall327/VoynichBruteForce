namespace VoynichBruteForce.Modifications;

/// <summary>
/// Base interface for text modifiers. Provides string-based text modification.
/// Use this for modifiers that cannot use the Span-based API (e.g., variable-length output).
/// </summary>
public interface ITextModifier
{
    string Name { get; }

    /// <summary>
    /// Arbitrary estimation of how difficult the rule is to apply in a precomputational world.
    /// </summary>
    CognitiveComplexity CognitiveCost { get; }

    /// <summary>
    /// Modifies the input text and returns the result.
    /// </summary>
    string ModifyText(string text);
}

/// <summary>
/// Extended interface for text modifiers that support zero-allocation Span-based processing.
/// Modifiers implementing this interface will be run first in the pipeline using pooled buffers.
/// </summary>
public interface ISpanTextModifier : ITextModifier
{
    /// <summary>
    /// Modifies text using Span-based ping-pong buffers for zero-allocation processing.
    /// Read from context.InputSpan, write to context.OutputSpan, then call context.Commit(newLength).
    /// </summary>
    void Modify(ref ProcessingContext context);
}

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
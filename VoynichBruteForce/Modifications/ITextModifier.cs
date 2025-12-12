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

    string ModifyText(string text);
}

public class NoOpTextModifier : ITextModifier
{
    public string Name => "NoOpTextModifier";
    public CognitiveComplexity CognitiveCost => new(0);
    public string ModifyText(string text)
    {
        return text;
    }
}
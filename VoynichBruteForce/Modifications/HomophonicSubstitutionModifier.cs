namespace VoynichBruteForce.Modifications;

/// <summary>
/// Applies a homophonic substitution cipher where common letters can be
/// represented by multiple symbols, cycling through alternatives.
///
/// Homophonic ciphers were used to defeat frequency analysis by giving
/// common letters (like E, T, A) multiple possible substitutes. This
/// technique was documented in Renaissance cryptography manuals and
/// was used in diplomatic correspondence. The concept requires only
/// a lookup table - no special tools.
///
/// NOTE: This modifier is not compatible with the Span-based processing pipeline
/// because it can output multiple characters per input character (variable-length output).
/// </summary>
public class HomophonicSubstitutionModifier : ITextModifier
{
    private readonly int _seed;

    public string Name => $"HomophonicSubstitution(seed:{_seed})";

    // High cognitive cost - requires tracking which substitute was last used
    public CognitiveComplexity CognitiveCost => new(7);

    /// <summary>
    /// Creates a homophonic substitution cipher with multiple substitutes
    /// for common letters.
    /// </summary>
    /// <param name="seed">Seed for generating substitute mappings.</param>
    /// <param name="maxSubstitutes">Maximum number of substitutes for the most common letters.</param>
    public HomophonicSubstitutionModifier(int seed, int maxSubstitutes = 4)
    {
        _seed = seed;
    }

    /// <summary>
    /// Creates a homophonic substitution cipher with explicit substitute mappings.
    /// </summary>
    public HomophonicSubstitutionModifier(Dictionary<char, string[]> substitutes)
    {
        _seed = 0;
    }

    public void Modify(ref ProcessingContext context)
    {
        throw new NotImplementedException(
            "HomophonicSubstitutionModifier cannot use Span<char> because it outputs variable-length " +
            "substitutes (some letters map to multi-character sequences), which breaks the 1:1 " +
            "character assumption of the ping-pong buffer architecture.");
    }
}

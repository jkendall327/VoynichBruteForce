namespace VoynichBruteForce.Modifications;

/// <summary>
/// Factory interface for creating random instances of text modifiers.
/// </summary>
public interface IModifierFactory
{
    /// <summary>
    /// Creates a random instance of a text modifier using the provided random source.
    /// </summary>
    ITextModifier CreateRandom(Random random);
}

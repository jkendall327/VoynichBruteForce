using VoynichBruteForce.Modifications;

namespace VoynichBruteForce.Evolution;

/// <summary>
/// Supplies an arbitrary combination of text modification algorithms.
/// The intent is that they are then applied to a text in sequence.
/// The sequence of algorithms may be entirely random, or determined in part by the nature of the provided source text.
/// </summary>
public interface IGenomeFactory
{
    /// <summary>
    /// Creates a random list of modifiers.
    /// </summary>
    List<ITextModifier> CreateRandomGenome(int length);

    /// <summary>
    /// Applies a minor change to an existing text modification strategy.
    /// E.g. removing one element, adding a new element, changing one element.
    /// </summary>
    List<ITextModifier> Mutate(List<ITextModifier> original);

    /// <summary>
    /// Combines two parent genomes to create a child.
    /// Example strategy: Take first half of A and second half of B.
    /// </summary>
    List<ITextModifier> Crossover(List<ITextModifier> parentA, List<ITextModifier> parentB);
}
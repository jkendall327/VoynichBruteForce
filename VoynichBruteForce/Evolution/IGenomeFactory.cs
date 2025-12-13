namespace VoynichBruteForce.Evolution;

/// <summary>
/// Supplies an arbitrary combination of source text and text modification algorithms.
/// The intent is that the source text and modifiers are then applied in sequence.
/// The genome (source text + modifiers) may be entirely random, or evolved through mutation and crossover.
/// </summary>
public interface IGenomeFactory
{
    /// <summary>
    /// Creates a random genome with a random source text and random modifiers.
    /// </summary>
    Genome CreateRandomGenome(int modifierCount);

    /// <summary>
    /// Applies mutation to an existing genome.
    /// May mutate the source text, the modifiers, or both.
    /// </summary>
    Genome Mutate(Genome original);

    /// <summary>
    /// Combines two parent genomes to create a child.
    /// Source text: 50/50 chance from either parent (uniform crossover for categorical gene).
    /// Modifiers: Single-point crossover taking head from A and tail from B.
    /// </summary>
    Genome Crossover(Genome parentA, Genome parentB);
}

namespace VoynichBruteForce;

public class Hyperparameters
{
    public const string SectionName = "Hyperparameters";

    /// <summary>
    /// Size of the population for the genetic algorithm.
    /// </summary>
    public int PopulationSize { get; init; } = 100;

    /// <summary>
    /// Maximum number of generations to run the evolution for.
    /// </summary>
    public int MaxGenerations { get; init; } = 1000;

    /// <summary>
    /// Probability (0.0-1.0) that a child formed by crossover also gets a random mutation.
    /// </summary>
    public double MutationRate { get; init; } = 0.4;

    /// <summary>
    /// Soft wall for cognitive complexity (warning threshold).
    /// </summary>
    public int SoftWallComplexity { get; init; } = 10;

    /// <summary>
    /// Hard wall for cognitive complexity (maximum allowed).
    /// </summary>
    public int HardWallComplexity { get; init; } = 100;
}

namespace VoynichBruteForce.Evolution;

/// <summary>
/// Represents the best result found during an evolution run.
/// </summary>
public class EvolutionResult
{
    /// <summary>
    /// The pipeline result that achieved the best error score.
    /// </summary>
    public PipelineResult Result { get; init; }

    /// <summary>
    /// The genome that produced the best result.
    /// </summary>
    public Genome Genome { get; init; }

    /// <summary>
    /// The generation at which the best result was found.
    /// </summary>
    public int Generation { get; init; }

    /// <summary>
    /// The total time elapsed during evolution up to when this result was found.
    /// </summary>
    public TimeSpan TotalElapsedTime { get; init; }

    public EvolutionResult(PipelineResult result, Genome genome, int generation, TimeSpan totalElapsedTime)
    {
        Result = result;
        Genome = genome;
        Generation = generation;
        TotalElapsedTime = totalElapsedTime;
    }
}
namespace VoynichBruteForce;

public class VoynichProfile
{
    public const string SectionName = "VoynichProfile";

    /// <summary>
    /// Target H2 (conditional) entropy for the Voynich manuscript.
    /// </summary>
    public float TargetH2Entropy { get; init; } = 2.0f;

    /// <summary>
    /// Target Zipf's law slope for the Voynich manuscript.
    /// </summary>
    public float TargetZipfSlope { get; init; } = 1.05f;

    /// <summary>
    /// Target ratio of repeated adjacent words in the Voynich manuscript.
    /// </summary>
    public double TargetRepeatedAdjacentWordsRatio { get; init; } = 0.08;

    /// <summary>
    /// Target type-token ratio (TTR) for the Voynich manuscript.
    /// </summary>
    public double TargetTypeTokenRatio { get; init; } = 0.25;

    /// <summary>
    /// Target H1 (single-character) entropy for the Voynich manuscript.
    /// </summary>
    public float TargetH1Entropy { get; init; } = 4.0f;

    /// <summary>
    /// Target correlation between word length and frequency in the Voynich manuscript.
    /// </summary>
    public double TargetWordLengthFrequencyCorrelation { get; init; } = -0.5;

    /// <summary>
    /// Target similarity ratio for neighboring words in the Voynich manuscript.
    /// </summary>
    public double TargetNeighboringWordSimilarity { get; init; } = 0.15;
}

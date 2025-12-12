namespace VoynichBruteForce.Rankings;

public static class VoynichConstants
{
    // TODO: find real empirical values for these.
    public const float TargetH2Entropy = 2.0f;
    public const float TargetZipfSlope = 1.05f;
    public const double TargetRepeatedAdjacentWordsRatio = 0.08; // TODO: find real value
    public const double TargetTypeTokenRatio = 0.25; // TODO: find real value (TTR)
    public const float TargetH1Entropy = 4.0f; // TODO: find real value
    public const double TargetWordLengthFrequencyCorrelation = -0.5; // TODO: find real value
    public const double TargetNeighboringWordSimilarity = 0.15; // TODO: find real value
}
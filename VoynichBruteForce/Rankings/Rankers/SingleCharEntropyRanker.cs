using Microsoft.Extensions.Options;

namespace VoynichBruteForce.Rankings;

/// <summary>
/// Measures Shannon entropy (H1) based on single character frequencies.
/// Higher entropy indicates more randomness in character distribution.
/// Formula: H1 = -Σ(p * log₂(p))
/// </summary>
public class SingleCharEntropyRanker(IOptions<VoynichProfile> profile) : IRuleAdherenceRanker
{
    private readonly VoynichProfile _profile = profile.Value;

    public string Name => "H1 Character Entropy";

    public RuleWeight Weight => RuleWeight.Standard;

    public RankerResult CalculateRank(PrecomputedTextAnalysis analysis)
    {
        var actualH1 = ComputeH1(analysis.CharFrequencies);

        var rawDelta = Math.Abs(actualH1 - _profile.TargetH1Entropy);

        // Normalize: 0.5 bits deviation is one error unit
        var tolerance = 0.5;
        var normalizedError = Math.Pow(rawDelta / tolerance, 2);

        return new(Name, actualH1, _profile.TargetH1Entropy, normalizedError, Weight);
    }

    private static double ComputeH1(Dictionary<char, int> charFrequencies)
    {
        if (charFrequencies.Count == 0)
            return 0;

        var totalChars = 0;
        foreach (var count in charFrequencies.Values)
            totalChars += count;

        if (totalChars == 0)
            return 0;

        // Calculate Shannon entropy: H = -Σ(p * log₂(p))
        double entropy = 0;
        foreach (var count in charFrequencies.Values)
        {
            var probability = (double)count / totalChars;
            entropy -= probability * Math.Log2(probability);
        }

        return entropy;
    }
}

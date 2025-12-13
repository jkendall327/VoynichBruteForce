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

    public RankerResult CalculateRank(string text)
    {
        double actualH1 = ComputeH1(text);

        double rawDelta = Math.Abs(actualH1 - _profile.TargetH1Entropy);

        // Normalize: 0.5 bits deviation is one error unit
        double tolerance = 0.5;
        double normalizedError = Math.Pow(rawDelta / tolerance, 2);

        return new(Name, actualH1, _profile.TargetH1Entropy, normalizedError, Weight);
    }

    private double ComputeH1(string text)
    {
        if (string.IsNullOrEmpty(text))
            return 0;

        // Count character frequencies (excluding whitespace)
        var charFrequencies = new Dictionary<char, int>();
        int totalChars = 0;

        foreach (char c in text)
        {
            // Skip whitespace
            if (char.IsWhiteSpace(c))
                continue;

            char normalized = char.ToLowerInvariant(c);
            if (charFrequencies.ContainsKey(normalized))
                charFrequencies[normalized]++;
            else
                charFrequencies[normalized] = 1;

            totalChars++;
        }

        if (totalChars == 0)
            return 0;

        // Calculate Shannon entropy: H = -Σ(p * log₂(p))
        double entropy = 0;
        foreach (var count in charFrequencies.Values)
        {
            double probability = (double)count / totalChars;
            entropy -= probability * Math.Log2(probability);
        }

        return entropy;
    }
}

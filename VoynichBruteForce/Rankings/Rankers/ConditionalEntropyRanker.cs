using Microsoft.Extensions.Options;

namespace VoynichBruteForce.Rankings;

public class ConditionalEntropyRanker(IOptions<VoynichProfile> profile) : IRuleAdherenceRanker
{
    private readonly VoynichProfile _profile = profile.Value;

    public string Name => "H2 Entropy";

    // This is critical because low H2 is the Voynich's defining feature
    public RuleWeight Weight => RuleWeight.Critical;

    public RankerResult CalculateRank(PrecomputedTextAnalysis analysis)
    {
        var actualH2 = ComputeH2(analysis.Bigrams);

        var rawDelta = Math.Abs(actualH2 - _profile.TargetH2Entropy);

        // NORMALIZATION LOGIC:
        // We decide that being off by 0.5 bits is a "Full Error Unit" (1.0).
        // Being off by 1.0 bit is 2.0 error units (or 4.0 if we square it).
        var tolerance = 0.5;
        var normalizedError = rawDelta / tolerance;

        // Optional: Square the error to punish large deviations more severely
        normalizedError = Math.Pow(normalizedError, 2);

        return new(Name, actualH2, _profile.TargetH2Entropy, normalizedError, Weight);
    }

    private static double ComputeH2(BigramData bigrams)
    {
        if (bigrams.TotalBigrams < 1)
            return 0;

        var bigramCounts = bigrams.BigramCounts;
        var charCounts = bigrams.PrevCharCounts;
        var totalChars = bigrams.TotalBigrams;

        // Calculate conditional entropy: H2 = Σ P(c1) * H(c2|c1)
        // H(c2|c1) = -Σ P(c2|c1) * log₂(P(c2|c1))
        double h2 = 0;

        foreach (var prevChar in bigramCounts.Keys)
        {
            var probPrevChar = (double)charCounts[prevChar] / totalChars;
            var totalFollowing = charCounts[prevChar];

            double conditionalEntropy = 0;
            foreach (var currChar in bigramCounts[prevChar].Keys)
            {
                var count = bigramCounts[prevChar][currChar];
                var conditionalProb = (double)count / totalFollowing;
                conditionalEntropy -= conditionalProb * Math.Log2(conditionalProb);
            }

            h2 += probPrevChar * conditionalEntropy;
        }

        return h2;
    }
}
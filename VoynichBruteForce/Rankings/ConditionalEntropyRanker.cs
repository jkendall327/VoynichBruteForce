using Microsoft.Extensions.Options;

namespace VoynichBruteForce.Rankings;

public class ConditionalEntropyRanker(IOptions<VoynichProfile> profile) : IRuleAdherenceRanker
{
    private readonly VoynichProfile _profile = profile.Value;

    public string Name => "H2 Entropy";

    // This is critical because low H2 is the Voynich's defining feature
    public RuleWeight Weight => RuleWeight.Critical;

    public RankerResult CalculateRank(string text)
    {
        var actualH2 = ComputeH2(text);

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

    private double ComputeH2(string text)
    {
        if (string.IsNullOrEmpty(text) || text.Length < 2)
            return 0;

        // Build bigram frequency table: Dictionary<char, Dictionary<char, int>>
        // Outer key: previous character, Inner key: current character, Value: count
        var bigramCounts = new Dictionary<char, Dictionary<char, int>>();
        var charCounts = new Dictionary<char, int>();

        // Normalize text and filter whitespace
        var cleanedText = new string(text.Where(c => !char.IsWhiteSpace(c))
                                         .Select(char.ToLowerInvariant)
                                         .ToArray());

        if (cleanedText.Length < 2)
            return 0;

        // Count bigrams and individual characters
        for (int i = 0; i < cleanedText.Length - 1; i++)
        {
            char prevChar = cleanedText[i];
            char currChar = cleanedText[i + 1];

            // Count bigrams
            if (!bigramCounts.ContainsKey(prevChar))
                bigramCounts[prevChar] = new Dictionary<char, int>();

            if (bigramCounts[prevChar].ContainsKey(currChar))
                bigramCounts[prevChar][currChar]++;
            else
                bigramCounts[prevChar][currChar] = 1;

            // Count individual characters (for probability calculation)
            if (charCounts.ContainsKey(prevChar))
                charCounts[prevChar]++;
            else
                charCounts[prevChar] = 1;
        }

        // Calculate conditional entropy: H2 = Σ P(c1) * H(c2|c1)
        // H(c2|c1) = -Σ P(c2|c1) * log₂(P(c2|c1))
        double h2 = 0;
        int totalChars = cleanedText.Length - 1;

        foreach (var prevChar in bigramCounts.Keys)
        {
            double probPrevChar = (double)charCounts[prevChar] / totalChars;
            int totalFollowing = charCounts[prevChar];

            double conditionalEntropy = 0;
            foreach (var currChar in bigramCounts[prevChar].Keys)
            {
                int count = bigramCounts[prevChar][currChar];
                double conditionalProb = (double)count / totalFollowing;
                conditionalEntropy -= conditionalProb * Math.Log2(conditionalProb);
            }

            h2 += probPrevChar * conditionalEntropy;
        }

        return h2;
    }
}
using Microsoft.Extensions.Options;

namespace VoynichBruteForce.Rankings;

/// <summary>
/// Measures the similarity between neighboring words using Levenshtein distance.
/// The Voynich Manuscript shows unusual patterns of similar adjacent words.
/// Lower distance means words are more similar.
/// </summary>
public class NeighboringWordSimilarityRanker(IOptions<VoynichProfile> profile) : IRuleAdherenceRanker
{
    private readonly VoynichProfile _profile = profile.Value;

    public string Name => "Neighboring Word Similarity";

    public RuleWeight Weight => RuleWeight.Standard;

    public RankerResult CalculateRank(PrecomputedTextAnalysis analysis)
    {
        var words = analysis.Words;

        if (words.Length < 2)
        {
            return new(Name, 0, _profile.TargetNeighboringWordSimilarity, 0, Weight);
        }

        // Only sample 500 pairs max to speed up evolution.
        var step = Math.Max(1, words.Length / 500);

        var similarPairCount = 0;
        var totalPairs = 0;

        for (var i = 1; i < words.Length; i += step)
        {
            // Distinct lengths cannot be distance <= 2 if diff > 2
            if (Math.Abs(words[i].Length - words[i - 1].Length) > 2)
            {
                totalPairs++;

                continue;
            }

            var distance = LevenshteinDistance(words[i - 1], words[i]);
            var maxLength = Math.Max(words[i - 1].Length, words[i].Length);

            // Normalize distance by max word length to get similarity ratio
            // Similar words have distance <= 2 (one or two character changes)
            if (maxLength > 0 && distance <= 2)
            {
                similarPairCount++;
            }

            totalPairs++;
        }

        var similarityRatio = (double) similarPairCount / totalPairs;
        var rawDelta = Math.Abs(similarityRatio - _profile.TargetNeighboringWordSimilarity);

        // Normalize: 0.05 (5% deviation) is one error unit
        var tolerance = 0.05;
        var normalizedError = Math.Pow(rawDelta / tolerance, 2);

        return new(Name, similarityRatio, _profile.TargetNeighboringWordSimilarity, normalizedError, Weight);
    }

    private int LevenshteinDistance(string source, string target)
    {
        if (string.IsNullOrEmpty(source))
        {
            return target?.Length ?? 0;
        }

        if (string.IsNullOrEmpty(target))
        {
            return source.Length;
        }

        var n = source.Length;
        var m = target.Length;
        var distance = new int[n + 1, m + 1];

        // Initialize first column and row
        for (var i = 0; i <= n; i++)
        {
            distance[i, 0] = i;
        }

        for (var j = 0; j <= m; j++)
        {
            distance[0, j] = j;
        }

        // Calculate distances
        for (var i = 1; i <= n; i++)
        {
            for (var j = 1; j <= m; j++)
            {
                var cost = (source[i - 1] == target[j - 1]) ? 0 : 1;

                distance[i, j] = Math.Min(Math.Min(distance[i - 1, j] + 1, // deletion
                        distance[i, j - 1] + 1), // insertion
                    distance[i - 1, j - 1] + cost); // substitution
            }
        }

        return distance[n, m];
    }
}
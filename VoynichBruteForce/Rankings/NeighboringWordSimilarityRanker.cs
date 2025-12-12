namespace VoynichBruteForce.Rankings;

/// <summary>
/// Measures the similarity between neighboring words using Levenshtein distance.
/// The Voynich Manuscript shows unusual patterns of similar adjacent words.
/// Lower distance means words are more similar.
/// </summary>
public class NeighboringWordSimilarityRanker : IRuleAdherenceRanker
{
    public string Name => "Neighboring Word Similarity";

    public RuleWeight Weight => RuleWeight.Standard;

    public RankerResult CalculateRank(string text)
    {
        var words = text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        if (words.Length < 2)
        {
            return new(Name, 0, VoynichConstants.TargetNeighboringWordSimilarity, 0, Weight);
        }

        int similarPairCount = 0;
        int totalPairs = 0;

        for (int i = 1; i < words.Length; i++)
        {
            int distance = LevenshteinDistance(words[i - 1], words[i]);
            int maxLength = Math.Max(words[i - 1].Length, words[i].Length);

            // Normalize distance by max word length to get similarity ratio
            // Similar words have distance <= 2 (one or two character changes)
            if (maxLength > 0 && distance <= 2)
            {
                similarPairCount++;
            }

            totalPairs++;
        }

        double similarityRatio = (double)similarPairCount / totalPairs;
        double rawDelta = Math.Abs(similarityRatio - VoynichConstants.TargetNeighboringWordSimilarity);

        // Normalize: 0.05 (5% deviation) is one error unit
        double tolerance = 0.05;
        double normalizedError = Math.Pow(rawDelta / tolerance, 2);

        return new(Name, similarityRatio, VoynichConstants.TargetNeighboringWordSimilarity, normalizedError, Weight);
    }

    private int LevenshteinDistance(string source, string target)
    {
        if (string.IsNullOrEmpty(source))
            return target?.Length ?? 0;

        if (string.IsNullOrEmpty(target))
            return source.Length;

        int n = source.Length;
        int m = target.Length;
        int[,] distance = new int[n + 1, m + 1];

        // Initialize first column and row
        for (int i = 0; i <= n; i++)
            distance[i, 0] = i;

        for (int j = 0; j <= m; j++)
            distance[0, j] = j;

        // Calculate distances
        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                int cost = (source[i - 1] == target[j - 1]) ? 0 : 1;

                distance[i, j] = Math.Min(
                    Math.Min(distance[i - 1, j] + 1,      // deletion
                             distance[i, j - 1] + 1),     // insertion
                    distance[i - 1, j - 1] + cost);       // substitution
            }
        }

        return distance[n, m];
    }
}

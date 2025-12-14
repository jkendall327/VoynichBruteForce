using System.Buffers;
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

    // Maximum word length for stackalloc. Beyond this, use ArrayPool.
    private const int MaxStackallocLength = 128;

    public string Name => "Neighboring Word Similarity";

    public RuleWeight Weight => RuleWeight.Standard;

    public RankerResult CalculateRank(PrecomputedTextAnalysis analysis)
    {
        var wordCount = analysis.WordCount;

        if (wordCount < 2)
        {
            return new(Name, 0, _profile.TargetNeighboringWordSimilarity, 0, Weight);
        }

        var words = analysis.Words;
        var text = analysis.TextSpan;
        int similarPairCount = 0;
        int totalPairs = 0;

        for (int i = 1; i < wordCount; i++)
        {
            var prev = words.GetWord(text, i - 1);
            var curr = words.GetWord(text, i);

            int distance = LevenshteinDistance(prev, curr);
            int maxLength = Math.Max(prev.Length, curr.Length);

            // Similar words have distance <= 2 (one or two character changes)
            if (maxLength > 0 && distance <= 2)
            {
                similarPairCount++;
            }

            totalPairs++;
        }

        double similarityRatio = (double)similarPairCount / totalPairs;
        double rawDelta = Math.Abs(similarityRatio - _profile.TargetNeighboringWordSimilarity);

        // Normalize: 0.05 (5% deviation) is one error unit
        double tolerance = 0.05;
        double normalizedError = Math.Pow(rawDelta / tolerance, 2);

        return new(Name, similarityRatio, _profile.TargetNeighboringWordSimilarity, normalizedError, Weight);
    }

    /// <summary>
    /// Calculates Levenshtein distance using two-row optimization.
    /// Uses stackalloc for small inputs, ArrayPool for larger ones.
    /// </summary>
    private static int LevenshteinDistance(ReadOnlySpan<char> source, ReadOnlySpan<char> target)
    {
        if (source.IsEmpty)
            return target.Length;

        if (target.IsEmpty)
            return source.Length;

        int n = source.Length;
        int m = target.Length;

        // Use two-row optimization: only need current and previous row
        int rowSize = m + 1;

        // Use stackalloc for reasonable sizes, ArrayPool otherwise
        int[]? pooledPrev = null;
        int[]? pooledCurr = null;

        Span<int> prevRow = rowSize <= MaxStackallocLength
            ? stackalloc int[rowSize]
            : (pooledPrev = ArrayPool<int>.Shared.Rent(rowSize)).AsSpan(0, rowSize);

        Span<int> currRow = rowSize <= MaxStackallocLength
            ? stackalloc int[rowSize]
            : (pooledCurr = ArrayPool<int>.Shared.Rent(rowSize)).AsSpan(0, rowSize);

        try
        {
            // Initialize first row
            for (int j = 0; j <= m; j++)
                prevRow[j] = j;

            // Calculate distances row by row
            for (int i = 1; i <= n; i++)
            {
                currRow[0] = i;
                char sourceChar = char.ToLowerInvariant(source[i - 1]);

                for (int j = 1; j <= m; j++)
                {
                    char targetChar = char.ToLowerInvariant(target[j - 1]);
                    int cost = sourceChar == targetChar ? 0 : 1;

                    currRow[j] = Math.Min(
                        Math.Min(
                            currRow[j - 1] + 1,      // insertion
                            prevRow[j] + 1),         // deletion
                        prevRow[j - 1] + cost);      // substitution
                }

                // Swap rows (can't use tuple swap with Span<T>)
                var temp = prevRow;
                prevRow = currRow;
                currRow = temp;
            }

            // Result is in prevRow after the swap
            return prevRow[m];
        }
        finally
        {
            if (pooledPrev != null)
                ArrayPool<int>.Shared.Return(pooledPrev);
            if (pooledCurr != null)
                ArrayPool<int>.Shared.Return(pooledCurr);
        }
    }
}

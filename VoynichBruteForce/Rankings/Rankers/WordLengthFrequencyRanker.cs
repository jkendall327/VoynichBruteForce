using System.Buffers;
using Microsoft.Extensions.Options;

namespace VoynichBruteForce.Rankings;

/// <summary>
/// Measures the correlation between word length and word frequency.
/// Natural languages typically show negative correlation (shorter words are more frequent).
/// </summary>
public class WordLengthFrequencyRanker(IOptions<VoynichProfile> profile) : IRuleAdherenceRanker
{
    private readonly VoynichProfile _profile = profile.Value;

    public string Name => "Word Length-Frequency Correlation";

    public RuleWeight Weight => RuleWeight.Standard;

    public RankerResult CalculateRank(PrecomputedTextAnalysis analysis)
    {
        if (analysis.WordCount < 10)
        {
            return new(Name, 0, _profile.TargetWordLengthFrequencyCorrelation, double.MaxValue, Weight);
        }

        var freqMap = analysis.WordFrequencyMap;
        var uniqueCount = freqMap.UniqueWordCount;

        if (uniqueCount < 2)
        {
            return new(Name, 0, _profile.TargetWordLengthFrequencyCorrelation, double.MaxValue, Weight);
        }

        // Rent arrays for lengths and frequencies
        var lengths = ArrayPool<int>.Shared.Rent(uniqueCount);
        var frequencies = ArrayPool<int>.Shared.Rent(uniqueCount);

        try
        {
            var count = freqMap.GetLengthsAndFrequencies(lengths, frequencies);

            // Calculate Pearson correlation between word length and frequency
            var correlation = CalculatePearsonCorrelation(
                lengths.AsSpan(0, count),
                frequencies.AsSpan(0, count));

            var rawDelta = Math.Abs(correlation - _profile.TargetWordLengthFrequencyCorrelation);

            // Normalize: 0.2 deviation in correlation is one error unit
            var tolerance = 0.2;
            var normalizedError = Math.Pow(rawDelta / tolerance, 2);

            return new(Name, correlation, _profile.TargetWordLengthFrequencyCorrelation, normalizedError, Weight);
        }
        finally
        {
            ArrayPool<int>.Shared.Return(lengths);
            ArrayPool<int>.Shared.Return(frequencies);
        }
    }

    private static double CalculatePearsonCorrelation(ReadOnlySpan<int> lengths, ReadOnlySpan<int> frequencies)
    {
        var n = lengths.Length;
        if (n < 2) return 0;

        double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0, sumY2 = 0;

        for (var i = 0; i < n; i++)
        {
            double x = lengths[i];
            double y = frequencies[i];

            sumX += x;
            sumY += y;
            sumXY += x * y;
            sumX2 += x * x;
            sumY2 += y * y;
        }

        var numerator = n * sumXY - sumX * sumY;
        var denominator = Math.Sqrt((n * sumX2 - sumX * sumX) * (n * sumY2 - sumY * sumY));

        if (Math.Abs(denominator) < 0.0001)
            return 0;

        return numerator / denominator;
    }
}

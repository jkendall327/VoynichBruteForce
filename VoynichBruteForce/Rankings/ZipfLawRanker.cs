using System.Buffers;
using Microsoft.Extensions.Options;

namespace VoynichBruteForce.Rankings;

/// <summary>
/// Measures how closely word frequency distribution follows Zipf's Law.
/// Zipf's law states that frequency is inversely proportional to rank (f ∝ 1/r).
/// The Voynich Manuscript notably adheres to Zipf's Law.
/// </summary>
public class ZipfLawRanker(IOptions<VoynichProfile> profile) : IRuleAdherenceRanker
{
    private readonly VoynichProfile _profile = profile.Value;

    public string Name => "Zipf's Law";

    public RuleWeight Weight => RuleWeight.High;

    public RankerResult CalculateRank(PrecomputedTextAnalysis analysis)
    {
        if (analysis.WordCount < 10)
        {
            return new(Name, 0, _profile.TargetZipfSlope, double.MaxValue, Weight);
        }

        // Get sorted frequencies from the span-based map
        var (frequencies, count) = analysis.WordFrequencyMap.GetSortedFrequencies();
        try
        {
            // Calculate slope using log-log regression
            // log(frequency) = log(C) - α * log(rank)
            // For ideal Zipf: α ≈ 1.0
            double slope = CalculateLogLogSlope(frequencies.AsSpan(0, count));

            double rawDelta = Math.Abs(slope - _profile.TargetZipfSlope);

            // Normalize: 0.2 deviation in slope is one error unit
            double tolerance = 0.2;
            double normalizedError = Math.Pow(rawDelta / tolerance, 2);

            return new(Name, slope, _profile.TargetZipfSlope, normalizedError, Weight);
        }
        finally
        {
            ArrayPool<int>.Shared.Return(frequencies);
        }
    }

    private static double CalculateLogLogSlope(ReadOnlySpan<int> sortedFrequencies)
    {
        // Use only top words to avoid noise from rare words
        int sampleSize = Math.Min(100, sortedFrequencies.Length);

        double sumLogRank = 0;
        double sumLogFreq = 0;
        double sumLogRankLogFreq = 0;
        double sumLogRankSquared = 0;
        int validPoints = 0;

        for (int rank = 1; rank <= sampleSize; rank++)
        {
            int frequency = sortedFrequencies[rank - 1];
            if (frequency <= 0) continue;

            double logRank = Math.Log(rank);
            double logFreq = Math.Log(frequency);

            sumLogRank += logRank;
            sumLogFreq += logFreq;
            sumLogRankLogFreq += logRank * logFreq;
            sumLogRankSquared += logRank * logRank;
            validPoints++;
        }

        if (validPoints < 2)
        {
            return 0;
        }

        // Linear regression: slope = (n*Σxy - Σx*Σy) / (n*Σx² - (Σx)²)
        double numerator = validPoints * sumLogRankLogFreq - sumLogRank * sumLogFreq;
        double denominator = validPoints * sumLogRankSquared - sumLogRank * sumLogRank;

        if (Math.Abs(denominator) < 0.0001)
        {
            return 0;
        }

        // Return absolute value of slope (Zipf's law has negative slope, but we care about magnitude)
        return Math.Abs(numerator / denominator);
    }
}

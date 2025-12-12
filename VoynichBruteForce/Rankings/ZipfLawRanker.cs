namespace VoynichBruteForce.Rankings;

/// <summary>
/// Measures how closely word frequency distribution follows Zipf's Law.
/// Zipf's law states that frequency is inversely proportional to rank (f ∝ 1/r).
/// The Voynich Manuscript notably adheres to Zipf's Law.
/// </summary>
public class ZipfLawRanker : IRuleAdherenceRanker
{
    public string Name => "Zipf's Law";

    public RuleWeight Weight => RuleWeight.High;

    public RankerResult CalculateRank(string text)
    {
        var words = text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        if (words.Length < 10)
        {
            return new(Name, 0, VoynichConstants.TargetZipfSlope, double.MaxValue, Weight);
        }

        // Count word frequencies
        var frequencyMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var word in words)
        {
            if (frequencyMap.ContainsKey(word))
                frequencyMap[word]++;
            else
                frequencyMap[word] = 1;
        }

        // Sort by frequency descending
        var sortedFrequencies = frequencyMap
            .OrderByDescending(kvp => kvp.Value)
            .Select(kvp => kvp.Value)
            .ToList();

        // Calculate slope using log-log regression
        // log(frequency) = log(C) - α * log(rank)
        // For ideal Zipf: α ≈ 1.0
        double slope = CalculateLogLogSlope(sortedFrequencies);

        double rawDelta = Math.Abs(slope - VoynichConstants.TargetZipfSlope);

        // Normalize: 0.2 deviation in slope is one error unit
        double tolerance = 0.2;
        double normalizedError = Math.Pow(rawDelta / tolerance, 2);

        return new(Name, slope, VoynichConstants.TargetZipfSlope, normalizedError, Weight);
    }

    private double CalculateLogLogSlope(List<int> sortedFrequencies)
    {
        // Use only top words to avoid noise from rare words
        int sampleSize = Math.Min(100, sortedFrequencies.Count);

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

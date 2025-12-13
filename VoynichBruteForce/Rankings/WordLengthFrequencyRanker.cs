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

    public RankerResult CalculateRank(string text)
    {
        var words = text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        if (words.Length < 10)
        {
            return new(Name, 0, _profile.TargetWordLengthFrequencyCorrelation, double.MaxValue, Weight);
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

        // Calculate Pearson correlation between word length and frequency
        var dataPoints = frequencyMap
            .Select(kvp => (Length: (double)kvp.Key.Length, Frequency: (double)kvp.Value))
            .ToList();

        double correlation = CalculatePearsonCorrelation(dataPoints);

        double rawDelta = Math.Abs(correlation - _profile.TargetWordLengthFrequencyCorrelation);

        // Normalize: 0.2 deviation in correlation is one error unit
        double tolerance = 0.2;
        double normalizedError = Math.Pow(rawDelta / tolerance, 2);

        return new(Name, correlation, _profile.TargetWordLengthFrequencyCorrelation, normalizedError, Weight);
    }

    private double CalculatePearsonCorrelation(List<(double Length, double Frequency)> dataPoints)
    {
        if (dataPoints.Count < 2)
            return 0;

        int n = dataPoints.Count;
        double sumX = dataPoints.Sum(p => p.Length);
        double sumY = dataPoints.Sum(p => p.Frequency);
        double sumXY = dataPoints.Sum(p => p.Length * p.Frequency);
        double sumX2 = dataPoints.Sum(p => p.Length * p.Length);
        double sumY2 = dataPoints.Sum(p => p.Frequency * p.Frequency);

        double numerator = n * sumXY - sumX * sumY;
        double denominator = Math.Sqrt((n * sumX2 - sumX * sumX) * (n * sumY2 - sumY * sumY));

        if (Math.Abs(denominator) < 0.0001)
            return 0;

        return numerator / denominator;
    }
}

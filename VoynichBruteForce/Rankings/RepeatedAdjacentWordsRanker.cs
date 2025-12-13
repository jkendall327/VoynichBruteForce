using Microsoft.Extensions.Options;

namespace VoynichBruteForce.Rankings;

/// <summary>
/// Measures the frequency of repeated adjacent words in the text.
/// The Voynich Manuscript exhibits unusual patterns of word repetition.
/// </summary>
public class RepeatedAdjacentWordsRanker(IOptions<VoynichProfile> profile) : IRuleAdherenceRanker
{
    private readonly VoynichProfile _profile = profile.Value;

    public string Name => "Repeated Adjacent Words";

    public RuleWeight Weight => RuleWeight.Standard;

    public RankerResult CalculateRank(PrecomputedTextAnalysis analysis)
    {
        var words = analysis.Words;

        if (words.Length < 2)
        {
            return new(Name, 0, _profile.TargetRepeatedAdjacentWordsRatio, 0, Weight);
        }

        int repetitionCount = 0;
        for (int i = 1; i < words.Length; i++)
        {
            if (words[i].Equals(words[i - 1], StringComparison.OrdinalIgnoreCase))
            {
                repetitionCount++;
            }
        }

        double repetitionRatio = (double)repetitionCount / words.Length;
        double rawDelta = Math.Abs(repetitionRatio - _profile.TargetRepeatedAdjacentWordsRatio);

        // Normalize: 0.05 (5% deviation) is one error unit
        double tolerance = 0.05;
        double normalizedError = Math.Pow(rawDelta / tolerance, 2);

        return new(Name, repetitionRatio, _profile.TargetRepeatedAdjacentWordsRatio, normalizedError, Weight);
    }
}

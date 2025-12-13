using Microsoft.Extensions.Options;

namespace VoynichBruteForce.Rankings;

/// <summary>
/// Measures the Type-Token Ratio (TTR) - the ratio of unique words to total words.
/// Lower TTR indicates more repetition (as seen in the Voynich Manuscript).
/// </summary>
public class VocabularySizeRanker(IOptions<VoynichProfile> profile) : IRuleAdherenceRanker
{
    private readonly VoynichProfile _profile = profile.Value;

    public string Name => "Type-Token Ratio (Vocabulary Size)";

    public RuleWeight Weight => RuleWeight.Standard;

    public RankerResult CalculateRank(string text)
    {
        var words = text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        if (words.Length == 0)
        {
            return new(Name, 0, _profile.TargetTypeTokenRatio, 0, Weight);
        }

        var uniqueWords = new HashSet<string>(words, StringComparer.OrdinalIgnoreCase);
        double typeTokenRatio = (double)uniqueWords.Count / words.Length;

        double rawDelta = Math.Abs(typeTokenRatio - _profile.TargetTypeTokenRatio);

        // Normalize: 0.1 (10% deviation) is one error unit
        double tolerance = 0.1;
        double normalizedError = Math.Pow(rawDelta / tolerance, 2);

        return new(Name, typeTokenRatio, _profile.TargetTypeTokenRatio, normalizedError, Weight);
    }
}

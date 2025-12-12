namespace VoynichBruteForce.Rankings;

public class ConditionalEntropyRanker : IRuleAdherenceRanker
{
    public string Name => "H2 Entropy";

    // This is critical because low H2 is the Voynich's defining feature
    public RuleWeight Weight => RuleWeight.Critical;

    public RankerResult CalculateRank(string text)
    {
        var actualH2 = ComputeH2(text);

        var rawDelta = Math.Abs(actualH2 - VoynichConstants.TargetH2Entropy);

        // NORMALIZATION LOGIC:
        // We decide that being off by 0.5 bits is a "Full Error Unit" (1.0).
        // Being off by 1.0 bit is 2.0 error units (or 4.0 if we square it).
        var tolerance = 0.5;
        var normalizedError = rawDelta / tolerance;

        // Optional: Square the error to punish large deviations more severely
        normalizedError = Math.Pow(normalizedError, 2);

        return new(Name, actualH2, VoynichConstants.TargetH2Entropy, normalizedError, Weight);
    }

    private double ComputeH2(string text)
    {
        throw new NotImplementedException();
    }
}
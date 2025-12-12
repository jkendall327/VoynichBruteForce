namespace VoynichBruteForce.Rankings;

public enum RuleWeight
{
    /// <summary>
    /// Nice to have, but not a dealbreaker (e.g., exact word count). Multiplier: 0.1
    /// </summary>
    Trivia = 0,

    /// <summary>
    /// Standard statistical feature (e.g., Zipf's law). Multiplier: 1.0
    /// </summary>
    Standard = 1,

    /// <summary>
    /// Hard to fake. If this fails, the method is wrong (e.g., H2 Entropy). Multiplier: 10.0
    /// </summary>
    High = 2,

    /// <summary>
    /// The "Golden Standard". If this is wrong, discard immediately. Multiplier: 50.0
    /// </summary>
    Critical = 3
}

public static class RuleWeightExtensions
{
    public static double ToMultiplier(this RuleWeight weight) =>
        weight switch
        {
            RuleWeight.Trivia => 0.1,
            RuleWeight.Standard => 1.0,
            RuleWeight.High => 10.0,
            RuleWeight.Critical => 50.0,
            _ => 1.0
        };
}
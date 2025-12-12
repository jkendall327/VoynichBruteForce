namespace VoynichBruteForce.Rankings;

/// <summary>
/// Measures how closely a given text adheres to a rule of some kind.
/// These are typically statistical rules like Zipf's law.
/// </summary>
public interface IRuleAdherenceRanker
{
    string Name { get; }

    /// <summary>
    /// How important this ranking is to emulating the overall profile of the Voynich.
    /// Some features, like low H2 entropy, are strikingly unique to the text, and hence are important to replicate.
    /// </summary>
    RuleWeight Weight { get; }

    RankerResult CalculateRank(string text);
}
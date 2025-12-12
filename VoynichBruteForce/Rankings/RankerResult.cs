namespace VoynichBruteForce.Rankings;

public record RankerResult(
    string RuleName,
    double RawMeasuredValue, // e.g. 3.5 bits
    double TargetValue, // e.g. 2.0 bits
    double NormalizedError, // e.g. 1.5 (Standardized deviation)
    RuleWeight Weight // Carried through for the final sum
);
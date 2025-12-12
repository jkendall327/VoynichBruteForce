namespace VoynichBruteForce.Modifications;

public readonly struct CognitiveComplexity
{
    public int Value { get; }

    public const int SoftWallComplexity = 10;
    public const int HardWallComplexity = 100;

    public CognitiveComplexity(int value)
    {
        if (value is < 0 or > 10)
        {
            throw new InvalidOperationException("Cognitive complexity is a scale from 0-10.");
        }

        Value = value;
    }
}
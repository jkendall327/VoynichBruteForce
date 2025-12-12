namespace VoynichBruteForce.Sources;

public class EmptySourceTextProvider : ITextProvider
{
    public string GetText() => string.Empty;
}
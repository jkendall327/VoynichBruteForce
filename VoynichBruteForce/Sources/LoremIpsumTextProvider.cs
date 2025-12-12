namespace VoynichBruteForce.Sources;

public class LoremIpsumTextProvider : ITextProvider
{
    public string GetText()
    {
        return "Lorem ipsum dolor sit amet";
    }
}
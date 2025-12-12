namespace VoynichBruteForce.Sources;

/// <summary>
/// Provides a source text prior to any manipulations.
/// This text may contain semantic meaning or be asemic.
/// </summary>
public interface ITextProvider
{
    string GetText();
}
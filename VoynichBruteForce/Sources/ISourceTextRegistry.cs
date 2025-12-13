namespace VoynichBruteForce.Sources;

/// <summary>
/// Registry that resolves SourceTextId enum values to actual text content.
/// Enables the genetic algorithm to work with an identifier while deferring
/// text generation/loading until evaluation time.
/// </summary>
public interface ISourceTextRegistry
{
    /// <summary>
    /// Gets the text content for the specified source text identifier.
    /// </summary>
    string GetText(SourceTextId id);

    /// <summary>
    /// Gets all available source text identifiers.
    /// Used by the genome factory for random selection.
    /// </summary>
    IReadOnlyList<SourceTextId> AvailableIds { get; }

    /// <summary>
    /// Gets a random source text identifier.
    /// </summary>
    SourceTextId GetRandomId(Random random);
}

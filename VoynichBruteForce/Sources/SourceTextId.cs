namespace VoynichBruteForce.Sources;

/// <summary>
/// Identifies available source text types for the genetic algorithm.
/// This is a categorical gene that can be mutated and crossed over.
/// </summary>
public enum SourceTextId
{
    // Asemic generators (existing providers)
    Random,
    LullianCombinator,
    ArithmeticSequence,
    SyllableTable,
    LoremIpsum,

    // Medieval European languages (file-based)
    Latin,
    MedievalGerman,
    OldFrench,
    Italian
}

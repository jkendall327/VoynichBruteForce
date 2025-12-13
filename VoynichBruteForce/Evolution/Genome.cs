using VoynichBruteForce.Modifications;
using VoynichBruteForce.Sources;

namespace VoynichBruteForce.Evolution;

/// <summary>
/// Represents the complete evolvable genome: source text selection + transformation pipeline.
/// This is the unit of inheritance in the genetic algorithm.
/// </summary>
public record Genome(SourceTextId SourceTextId, List<ITextModifier> Modifiers);

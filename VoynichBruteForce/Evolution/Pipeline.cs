using VoynichBruteForce.Modifications;

namespace VoynichBruteForce.Evolution;

public record Pipeline(string SourceText, List<ITextModifier> Modifiers);
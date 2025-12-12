namespace VoynichBruteForce;

/// <summary>
/// Provides a source text prior to any manipulations.
/// This text may contain semantic meaning or be asemic.
/// </summary>
public interface ITextProvider
{
    string GetText();
}

/// <summary>
/// Applies an algorithmic modification to a source text.
/// Examples include simple substitutions, vowel removals, word shuffling, etc.
/// </summary>
public interface ITextModifier
{
    string ModifyText(string text);
}

/// <summary>
/// Supplies an arbitrary combination of text modification algorithms.
/// The intent is that they are then applied to a text in sequence.
/// The sequence of algorithms may be entirely random, or determined in part by the nature of the provided source text.
/// </summary>
public interface IModifierCollectionProvider
{
    List<ITextModifier> GetModificationPipeline(string text);
}

/// <summary>
/// Measures how closely a given text adheres to a rule of some kind.
/// These are typically statistical rules like Zipf's law.
/// </summary>
public interface IRuleAdherenceRanker
{
    (string Rule, float Adherence) AdheresToRule(string text);
}

public interface IRankerProvider
{
    List<IRuleAdherenceRanker> GetRankers();
}

public class MethodRanker(
    ITextProvider textProvider,
    IModifierCollectionProvider modifierProvider,
    IRankerProvider rankerProvider)
{
    public void Run()
    {
        var sourceText = textProvider.GetText();
        var modifiers = modifierProvider.GetModificationPipeline(sourceText);

        sourceText = modifiers.Aggregate(sourceText, (current, modifier) => modifier.ModifyText(current));

        var rankers = rankerProvider.GetRankers();
        
        foreach (var ranker in rankers)
        {
            (var rule, var adherence) = ranker.AdheresToRule(sourceText);

            Console.WriteLine($"{rule}: {adherence}");
        }
    }
}
using Microsoft.Extensions.Logging;

namespace VoynichBruteForce;

/// <summary>
/// Provides a source text prior to any manipulations.
/// This text may contain semantic meaning or be asemic.
/// </summary>
public interface ITextProvider
{
    string GetText();
}

public class EmptySourceTextProvider : ITextProvider
{
    public string GetText() => string.Empty;
}

public class LoremIpsumTextProvider : ITextProvider
{
    public string GetText()
    {
        throw new NotImplementedException("Provide entire lorem ipsum text here or whatever");
    }
}

/// <summary>
/// Applies an algorithmic modification to a source text.
/// Examples include simple substitutions, vowel removals, word shuffling, etc.
/// </summary>
public interface ITextModifier
{
    string Name { get; }

    /// <summary>
    /// Arbitrary estimation of how difficult the rule is to apply in a precomputational world.
    /// </summary>
    CognitiveComplexity CognitiveCost { get; }

    string ModifyText(string text);
}

public class CognitiveComplexity
{
    public int Value { get; private set; }

    public CognitiveComplexity(int value)
    {
        if (value is < 0 or > 10)
        {
            throw new InvalidOperationException("Cognitive complexity is a scale from 0-10.");
        }

        Value = value;
    }
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
    string Name { get; }
    Adherence AdheresToRule(string text);
}

public record Adherence(string RuleName, double DeltaFromVoynich);

public interface IRankerProvider
{
    List<IRuleAdherenceRanker> GetRankers();
}

public class VoynichConstants
{
    // TODO: find real empirical values for these.
    public const float TargetH2 = 2.0f;
    public const float TargetZipfSlope = 1.05f;
}

public class PipelineResult
{
    /// <summary>
    /// Abbreviated description of the algorithm used in this pipeline to modify the source text.
    /// </summary>
    public string PipelineDescription { get; set; }

    /// <summary>
    /// This pipeline's total deviation from the Voynich's empirical statistical profile. 0.0 would be a perfect match.
    /// </summary>
    public double TotalErrorScore { get; set; }

    /// <summary>
    /// Estimation of how difficult this pipeline would have been to execute for the Voynich author(s).
    /// </summary>
    public int TotalCognitiveLoad { get; set; }

    /// <summary>
    /// Deltas from the Voynich for each ranking method.
    /// </summary>
    public Dictionary<string, double> Stats { get; set; }

    public PipelineResult(List<ITextModifier> modifiers, List<Adherence> results)
    {
        TotalCognitiveLoad = modifiers.Sum(s => s.CognitiveCost.Value);
        TotalErrorScore = results.Sum(s => s.DeltaFromVoynich);

        PipelineDescription = string.Join(" -> ", modifiers.Select(m => m.Name));

        Stats = results.ToDictionary(s => s.RuleName, s => s.DeltaFromVoynich);
    }
}

public record Pipeline(string SourceText, List<ITextModifier> Modifiers);

public class PipelineRunner(IRankerProvider rankerProvider, ILogger<PipelineRunner> logger)
{
    public PipelineResult Run(Pipeline pipeline)
    {
        (var sourceText, var modifiers) = pipeline;
        
        sourceText = modifiers.Aggregate(sourceText, (current, modifier) => modifier.ModifyText(current));

        var rankers = rankerProvider.GetRankers();

        var results = new List<Adherence>();

        foreach (var ranker in rankers)
        {
            var delta = ranker.AdheresToRule(sourceText);

            logger.LogInformation("{RankingMethod}: {Error}", ranker.Name, delta);
        }

        return new(modifiers, results);
    }
}

public class EvolutionEngine(
    PipelineRunner runner,
    ITextProvider textProvider,
    IModifierCollectionProvider modifierProvider,
    ILogger<EvolutionEngine> logger)
{
    public void Evolve()
    {
        // TODO: Should use a random text provider?
        var sourceText = textProvider.GetText();
        var modifiers = modifierProvider.GetModificationPipeline(sourceText);

        var result = runner.Run(new(sourceText, modifiers));

        throw new NotImplementedException("Actually do evolution here somehow?");

        // 1. Generate random modifier chains (Gen 0)
        // 2. Measure Error against VoynichProfile
        // 3. Kill the bottom 50%
        // 4. Mutate the survivors (add a step, remove a step, change a param)
        // 5. Repeat
    }
}
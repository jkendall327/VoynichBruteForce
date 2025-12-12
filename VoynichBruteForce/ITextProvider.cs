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

/// <summary>
/// Supplies an arbitrary combination of text modification algorithms.
/// The intent is that they are then applied to a text in sequence.
/// The sequence of algorithms may be entirely random, or determined in part by the nature of the provided source text.
/// </summary>
public interface IGenomeFactory
{
    // Create a random list of modifiers (Gen 0)
    List<ITextModifier> CreateRandomGenome(int length);

    // Mutate an existing list (small change)
    List<ITextModifier> Mutate(List<ITextModifier> original);
}

public enum RuleWeight
{
    /// <summary>
    /// Nice to have, but not a dealbreaker (e.g., exact word count). Multiplier: 0.1
    /// </summary>
    Trivia = 0,

    /// <summary>
    /// Standard statistical feature (e.g., Zipf's law). Multiplier: 1.0
    /// </summary>
    Standard = 1,

    /// <summary>
    /// Hard to fake. If this fails, the method is wrong (e.g., H2 Entropy). Multiplier: 10.0
    /// </summary>
    High = 2,

    /// <summary>
    /// The "Golden Standard". If this is wrong, discard immediately. Multiplier: 50.0
    /// </summary>
    Critical = 3
}

public static class RuleWeightExtensions
{
    public static double ToMultiplier(this RuleWeight weight) =>
        weight switch
        {
            RuleWeight.Trivia => 0.1,
            RuleWeight.Standard => 1.0,
            RuleWeight.High => 10.0,
            RuleWeight.Critical => 50.0,
            _ => 1.0
        };
}

/// <summary>
/// Measures how closely a given text adheres to a rule of some kind.
/// These are typically statistical rules like Zipf's law.
/// </summary>
public interface IRuleAdherenceRanker
{
    string Name { get; }

    /// <summary>
    /// How important this ranking is to emulating the overall profile of the Voynich.
    /// Some features, like low H2 entropy, are strikingly unique to the text, and hence are important to replicate.
    /// </summary>
    RuleWeight Weight { get; }

    RankerResult CalculateRank(string text);
}

public record RankerResult(
    string RuleName,
    double RawMeasuredValue, // e.g. 3.5 bits
    double TargetValue, // e.g. 2.0 bits
    double NormalizedError, // e.g. 1.5 (Standardized deviation)
    RuleWeight Weight // Carried through for the final sum
);

public interface IRankerProvider
{
    List<IRuleAdherenceRanker> GetRankers();
}

public class ConditionalEntropyRanker : IRuleAdherenceRanker
{
    public string Name => "H2 Entropy";

    // This is critical because low H2 is the Voynich's defining feature
    public RuleWeight Weight => RuleWeight.Critical;

    public RankerResult CalculateRank(string text)
    {
        var actualH2 = ComputeH2(text);

        var rawDelta = Math.Abs(actualH2 - VoynichConstants.TargetH2Entropy);

        // NORMALIZATION LOGIC:
        // We decide that being off by 0.5 bits is a "Full Error Unit" (1.0).
        // Being off by 1.0 bit is 2.0 error units (or 4.0 if we square it).
        var tolerance = 0.5;
        var normalizedError = rawDelta / tolerance;

        // Optional: Square the error to punish large deviations more severely
        normalizedError = Math.Pow(normalizedError, 2);

        return new(Name, actualH2, VoynichConstants.TargetH2Entropy, normalizedError, Weight);
    }

    private double ComputeH2(string text)
    {
        throw new NotImplementedException();
    }
}

public class VoynichConstants
{
    // TODO: find real empirical values for these.
    public const float TargetH2Entropy = 2.0f;
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
    public List<RankerResult> Results { get; set; }

    public PipelineResult(List<ITextModifier> modifiers, List<RankerResult> results)
    {
        Results = results;
        PipelineDescription = string.Join(" -> ", modifiers.Select(m => m.Name));
        TotalCognitiveLoad = modifiers.Sum(s => s.CognitiveCost.Value);

        var initialError = results.Sum(r => r.NormalizedError * r.Weight.ToMultiplier());
        double cognitiveLoad = modifiers.Sum(m => m.CognitiveCost.Value);

        /*
         * We want to penalise very complex solutions, because the more complex they are,
         * the less likey they were actually performed by the Voynich authors.
         * But it should not be an overriding factor: the creators of the Voynich were clearly a unique breed!
         * We will try to nudge the evolution towards a *simple* solution which emulates the Voynich, if one exists.
         * But that's just an 'if'. We'll accept a complex one, within reason.
         */

        // Apply a soft penalty.
        if (cognitiveLoad > CognitiveComplexity.SoftWallComplexity)
        {
            initialError *= 2.0;
        }

        // Apply a hard wall - arbitrarily decide this was too much for the original authors.
        if (cognitiveLoad > CognitiveComplexity.HardWallComplexity)
        {
            initialError += 1000;
        }

        TotalErrorScore = initialError;
    }
}

public record Pipeline(string SourceText, List<ITextModifier> Modifiers);

public class PipelineRunner(IRankerProvider rankerProvider, ILogger<PipelineRunner> logger)
{
    public PipelineResult Run(Pipeline pipeline)
    {
        (var sourceText, var modifiers) = pipeline;

        sourceText = modifiers.Aggregate(sourceText, (current, modifier) => modifier.ModifyText(current));

        // Sanity check - prevent degenerate optimisation for empty texts by returning max error immediately.
        if (sourceText.Length < 100)
        {
            return new(modifiers, [])
            {
                TotalErrorScore = double.MaxValue
            };
        }

        var rankers = rankerProvider.GetRankers();

        var results = new List<RankerResult>();

        foreach (var ranker in rankers)
        {
            var delta = ranker.CalculateRank(sourceText);

            logger.LogTrace("{RankingMethod}: {Error}", ranker.Name, delta);
        }

        return new(modifiers, results);
    }
}

public class EvolutionEngine(
    PipelineRunner runner,
    ITextProvider textProvider,
    IGenomeFactory genomeFactory,
    ILogger<EvolutionEngine> logger)
{
    private const int PopulationSize = 100;
    private const int MaxGenerations = 1000;

    public void Evolve(int seed)
    {
        var sourceText = textProvider.GetText();

        // 1. Initialize Population (Gen 0)
        var population = new List<Pipeline>();

        for (var i = 0; i < PopulationSize; i++)
        {
            var modifiers = genomeFactory.CreateRandomGenome(length: 5);
            population.Add(new(sourceText, modifiers));
        }

        for (var gen = 0; gen < MaxGenerations; gen++)
        {
            var rankedResults = new List<(Pipeline Pipeline, PipelineResult Result)>();

            // 2. Evaluate Fitness
            foreach (var creature in population)
            {
                var result = runner.Run(creature);

                // Disqualify if too hard for a human to write
                if (result.TotalCognitiveLoad > 25)
                {
                    result.TotalErrorScore += 9999; // Penalty
                }

                rankedResults.Add((creature, result));
            }

            // Order by lowest error (best fit)
            var sorted = rankedResults
                .OrderBy(x => x.Result.TotalErrorScore)
                .ToList();

            var best = sorted.First();

            logger.LogInformation("Gen {Gen} Best: {Desc} | Error: {Err}",
                gen,
                best.Result.PipelineDescription,
                best.Result.TotalErrorScore);

            if (best.Result.TotalErrorScore < 0.05)
            {
                break; // Found it!
            }

            // 3. Selection & Reproduction
            var nextGen = new List<Pipeline>();

            // Elitism: Keep the top 10% unchanged
            nextGen.AddRange(sorted
                .Take(10)
                .Select(x => x.Pipeline));

            // Fill the rest with mutations of the top 50%
            var survivors = sorted
                .Take(PopulationSize / 2)
                .ToList();

            var random = new Random(seed);

            while (nextGen.Count < PopulationSize)
            {
                // Pick a random parent from survivors
                var randomSurvivor = survivors[random.Next(survivors.Count)];
                var parent = randomSurvivor.Pipeline;

                // Mutate
                var childModifiers = genomeFactory.Mutate(parent.Modifiers);
                nextGen.Add(new(sourceText, childModifiers));
            }

            population = nextGen;
        }
    }
}
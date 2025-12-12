using Microsoft.Extensions.Logging;
using VoynichBruteForce.Rankings;

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
    /// <summary>
    /// Creates a random list of modifiers.
    /// </summary>
    List<ITextModifier> CreateRandomGenome(int length);

    /// <summary>
    /// Applies a minor change to an existing text modification strategy.
    /// E.g. removing one element, adding a new element, changing one element.
    /// </summary>
    List<ITextModifier> Mutate(List<ITextModifier> original);

    /// <summary>
    /// Combines two parent genomes to create a child.
    /// Example strategy: Take first half of A and second half of B.
    /// </summary>
    List<ITextModifier> Crossover(List<ITextModifier> parentA, List<ITextModifier> parentB);
}

public enum GenomeMutationStrategy
{
    RemoveElement,
    AddElement,
    ChangeElement,
    ShuffleOrder
}

public class DefaultGenomeFactory : IGenomeFactory
{
    public List<ITextModifier> CreateRandomGenome(int length)
    {
        throw new NotImplementedException();
    }

    public List<ITextModifier> Mutate(List<ITextModifier> original)
    {
        throw new NotImplementedException("Randomly switch on GenomeMutationStrategy here?");
    }

    public List<ITextModifier> Crossover(List<ITextModifier> parentA, List<ITextModifier> parentB)
    {
        // TODO: properly understand this code instead of cargo-culting it.

        var child = new List<ITextModifier>();
        var random = new Random();

        // Pick a split point based on the shorter parent to avoid index errors
        var minLen = Math.Min(parentA.Count, parentB.Count);
        var splitPoint = random.Next(0, minLen);

        // Take head from A
        for (var i = 0; i < splitPoint; i++)
        {
            child.Add(parentA[i]);
        }

        // Take tail from B
        for (var i = splitPoint; i < parentB.Count; i++)
        {
            child.Add(parentB[i]);
        }

        return child;
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
    public string PipelineDescription { get; }

    /// <summary>
    /// This pipeline's total deviation from the Voynich's empirical statistical profile. 0.0 would be a perfect match.
    /// </summary>
    public double TotalErrorScore { get; init; }

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

    // 40% chance that a child formed by crossover also gets a random mutation
    private const double MutationRate = 0.4;

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
                // Found it. Nobel prize here etc.
                break;
            }

            // 3. Selection & Reproduction
            var nextGen = new List<Pipeline>();

            // Elitism: Keep the top 10% unchanged
            nextGen.AddRange(sorted
                .Take(PopulationSize / 10)
                .Select(x => x.Pipeline));

            // Fill the rest with mutations of the top 50%
            var survivors = sorted
                .Take(PopulationSize / 2)
                .ToList();

            var random = new Random(seed + gen); // Ensure randomness varies per gen

            while (nextGen.Count < PopulationSize)
            {
                // STEP A: Select two distinctive parents
                // (Using random selection from the top 50% is a simple, effective strategy)
                var parentA = survivors[random.Next(survivors.Count)].Pipeline;
                var parentB = survivors[random.Next(survivors.Count)].Pipeline;

                // Try to ensure we aren't breeding a parent with itself, 
                // though in small pools it happens.
                if (survivors.Count > 1)
                {
                    while (parentB == parentA)
                    {
                        parentB = survivors[random.Next(survivors.Count)].Pipeline;
                    }
                }

                // STEP B: Crossover
                // Create a child by mixing traits of A and B
                var childModifiers = genomeFactory.Crossover(parentA.Modifiers, parentB.Modifiers);

                // STEP C: Mutation (The "Spark" of novelty)
                // Crossover rearranges existing solutions. Mutation finds new ones.
                // We apply mutation probabilistically.
                if (random.NextDouble() < MutationRate)
                {
                    childModifiers = genomeFactory.Mutate(childModifiers);
                }

                nextGen.Add(new(sourceText, childModifiers));
            }

            population = nextGen;
        }
    }
}
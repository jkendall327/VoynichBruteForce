using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VoynichBruteForce.Sources;

namespace VoynichBruteForce.Evolution;

public partial class EvolutionEngine(
    PipelineRunner runner,
    ISourceTextRegistry sourceTextRegistry,
    IGenomeFactory genomeFactory,
    IOptions<Hyperparameters> hyperparameters,
    ILogger<EvolutionEngine> logger)
{
    private readonly Hyperparameters _hyperparameters = hyperparameters.Value;

    public void Evolve(int seed)
    {
        using var evolutionScope = logger.BeginScope(new Dictionary<string, object>
        {
            ["Seed"] = seed,
            ["PopulationSize"] = _hyperparameters.PopulationSize,
            ["MaxGenerations"] = _hyperparameters.MaxGenerations,
            ["MutationRate"] = _hyperparameters.MutationRate
        });

        LogEvolutionStarted(logger, seed, _hyperparameters.PopulationSize, _hyperparameters.MaxGenerations, sourceTextRegistry.AvailableIds.Count);

        // 1. Initialize Population (Gen 0)
        var population = new List<Genome>();

        for (var i = 0; i < _hyperparameters.PopulationSize; i++)
        {
            var genome = genomeFactory.CreateRandomGenome(modifierCount: 5);
            population.Add(genome);
        }

        LogPopulationInitialized(logger, _hyperparameters.PopulationSize);

        for (var gen = 0; gen < _hyperparameters.MaxGenerations; gen++)
        {
            var rankedResults = new ConcurrentBag<(Genome Genome, PipelineResult Result)>();

            // 2. Evaluate Fitness
            Parallel.ForEach(population,
                genome =>
                {
                    // Resolve genome to pipeline at evaluation time
                    var sourceText = sourceTextRegistry.GetText(genome.SourceTextId);
                    var pipeline = new Pipeline(sourceText, genome.Modifiers);
                    var result = runner.Run(pipeline, genome.SourceTextId);
                    rankedResults.Add((genome, result));
                });

            // Order by lowest error (best fit)
            var sorted = rankedResults
                .OrderBy(x => x.Result.TotalErrorScore)
                .ToList();

            var best = sorted.First();
            var worst = sorted.Last();
            var avgError = sorted.Average(x => x.Result.TotalErrorScore);

            using (logger.BeginScope(new Dictionary<string, object>
            {
                ["Generation"] = gen,
                ["BestError"] = best.Result.TotalErrorScore,
                ["AvgError"] = avgError,
                ["WorstError"] = worst.Result.TotalErrorScore
            }))
            {
                LogGenerationInfo(logger, gen, best.Result.PipelineDescription,
                    best.Result.TotalErrorScore, avgError, worst.Result.TotalErrorScore);
            }

            if (best.Result.TotalErrorScore < 0.05)
            {
                LogEvolutionSuccess(logger, gen, best.Result.TotalErrorScore);
                break;
            }

            // 3. Selection & Reproduction
            var nextGen = new List<Genome>();

            // Elitism: Keep the top 10% unchanged
            nextGen.AddRange(sorted
                .Take(_hyperparameters.PopulationSize / 10)
                .Select(x => x.Genome));

            // Fill the rest with mutations of the top 50%
            var survivors = sorted
                .Take(_hyperparameters.PopulationSize / 2)
                .ToList();

            var random = new Random(seed + gen); // Ensure randomness varies per gen

            while (nextGen.Count < _hyperparameters.PopulationSize)
            {
                // STEP A: Select two distinctive parents
                // (Using random selection from the top 50% is a simple, effective strategy)
                var parentA = survivors[random.Next(survivors.Count)].Genome;
                var parentB = survivors[random.Next(survivors.Count)].Genome;

                // Try to ensure we aren't breeding a parent with itself,
                // though in small pools it happens.
                if (survivors.Count > 1)
                {
                    while (parentB == parentA)
                    {
                        parentB = survivors[random.Next(survivors.Count)].Genome;
                    }
                }

                // STEP B: Crossover
                // Create a child by mixing traits of A and B (both source text and modifiers)
                var child = genomeFactory.Crossover(parentA, parentB);

                // STEP C: Mutation (The "Spark" of novelty)
                // Crossover rearranges existing solutions. Mutation finds new ones.
                // We apply mutation probabilistically.
                if (random.NextDouble() < _hyperparameters.MutationRate)
                {
                    child = genomeFactory.Mutate(child);
                }

                nextGen.Add(child);
            }

            population = nextGen;
        }

        LogEvolutionCompleted(logger, _hyperparameters.MaxGenerations);
    }

    [LoggerMessage(LogLevel.Information, "Starting evolution: Seed={Seed}, Population={PopulationSize}, MaxGen={MaxGenerations}, AvailableSourceTexts={SourceTextCount}")]
    static partial void LogEvolutionStarted(ILogger<EvolutionEngine> logger, int seed, int populationSize, int maxGenerations, int sourceTextCount);

    [LoggerMessage(LogLevel.Information, "Population initialized with {Count} random genomes")]
    static partial void LogPopulationInitialized(ILogger<EvolutionEngine> logger, int count);

    [LoggerMessage(LogLevel.Information, "Gen {Gen}: Best={BestError:F6} (Avg={AvgError:F6}, Worst={WorstError:F6}) | {Desc}")]
    static partial void LogGenerationInfo(ILogger<EvolutionEngine> logger, int gen, string desc, double bestError, double avgError, double worstError);

    [LoggerMessage(LogLevel.Information, "Evolution succeeded at generation {Gen} with error {Error:F6}")]
    static partial void LogEvolutionSuccess(ILogger<EvolutionEngine> logger, int gen, double error);

    [LoggerMessage(LogLevel.Information, "Evolution completed after {MaxGenerations} generations")]
    static partial void LogEvolutionCompleted(ILogger<EvolutionEngine> logger, int maxGenerations);
}

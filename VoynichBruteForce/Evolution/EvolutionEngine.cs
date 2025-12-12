using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using VoynichBruteForce.Sources;

namespace VoynichBruteForce.Evolution;

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
            var rankedResults = new ConcurrentBag<(Pipeline Pipeline, PipelineResult Result)>();

            // 2. Evaluate Fitness
            Parallel.ForEach(population,
                pipeline =>
                {
                    var result = runner.Run(pipeline);
                    rankedResults.Add((pipeline, result));
                });
            
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
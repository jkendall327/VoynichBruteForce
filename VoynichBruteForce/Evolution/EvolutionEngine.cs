using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VoynichBruteForce.Sources;

namespace VoynichBruteForce.Evolution;

public partial class EvolutionEngine(
    PipelineRunner runner,
    ISourceTextRegistry sourceTextRegistry,
    IGenomeFactory genomeFactory,
    RandomFactory randomFactory,
    IOptions<Hyperparameters> hyperparameters,
    IOptions<AppSettings> appSettings,
    ILogger<EvolutionEngine> logger)
{
    private readonly Hyperparameters _hyperparameters = hyperparameters.Value;
    private readonly AppSettings _appSettings = appSettings.Value;

    private TimeSpan? _elapsedTotal;

    public EvolutionResult? Evolve(int seed)
    {
        // ReSharper disable once HeapView.ObjectAllocation.Evident
        // Only allocates once outside of the evolutionary loop.
        using var evolutionScope = logger.BeginScope(new Dictionary<string, object>
        {
            // ReSharper disable once HeapView.BoxingAllocation
            ["Seed"] = seed,
            // ReSharper disable once HeapView.BoxingAllocation
            ["PopulationSize"] = _hyperparameters.PopulationSize,
            // ReSharper disable once HeapView.BoxingAllocation
            ["MaxGenerations"] = _hyperparameters.MaxGenerations,
            // ReSharper disable once HeapView.BoxingAllocation
            ["MutationRate"] = _hyperparameters.MutationRate
        });

        LogEvolutionStarted(logger,
            seed,
            _hyperparameters.PopulationSize,
            _hyperparameters.MaxGenerations,
            sourceTextRegistry.AvailableIds.Count);

        var popSize = _hyperparameters.PopulationSize;

        // Pre-allocate double buffers for zero-allocation generation swapping
        var populationBufferA = new Genome[popSize];
        var populationBufferB = new Genome[popSize];

        // Pre-allocate ranked results array - reused every generation
        var rankedResults = new (Genome Genome, PipelineResult Result)[popSize];

        // Initialize Generation 0 into Buffer A
        for (var i = 0; i < popSize; i++)
        {
            populationBufferA[i] = genomeFactory.CreateRandomGenome(modifierCount: 5);
        }

        // Pointers to current and next generation buffers
        var currentGen = populationBufferA;
        var nextGen = populationBufferB;

        LogPopulationInitialized(logger, popSize);

        // ReSharper disable once HeapView.ObjectAllocation.Evident
        // Only allocated once outside of the main evolutionary loop.
        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = _appSettings.DegreeOfParallelism
        };

        // Tracking variables for cataclysm feature
        var globalBestError = double.MaxValue;
        var generationsSinceLastImprovement = 0;

        // Evaluate fitness.
        for (var gen = 0; gen < _hyperparameters.MaxGenerations; gen++)
        {
            var start = Stopwatch.GetTimestamp();

            // Capture reference for the parallel loop
            var genPopulation = currentGen;

            Parallel.For(0,
                popSize,
                options,
                i =>
                {
                    var genome = genPopulation[i];

                    // Resolve genome to pipeline at evaluation time
                    var sourceText = sourceTextRegistry.GetText(genome.SourceTextId);
                    var result = runner.Run(genome, sourceText);
                    rankedResults[i] = (genome, result);
                });

            var elapsed = Stopwatch.GetElapsedTime(start);

            _elapsedTotal = (_elapsedTotal ?? TimeSpan.Zero) + elapsed;

            // Order by lowest error (best fit)
            Array.Sort(rankedResults, static (a, b) => a.Result.TotalErrorScore.CompareTo(b.Result.TotalErrorScore));

            var best = rankedResults[0];
            var worst = rankedResults[^1];

            // Check for improvement (using a small epsilon for float comparison)
            var currentBestError = best.Result.TotalErrorScore;

            if (currentBestError < globalBestError - 0.00001)
            {
                globalBestError = currentBestError;
                generationsSinceLastImprovement = 0;
            }
            else
            {
                generationsSinceLastImprovement++;
            }

            LogGenerationInfo(logger,
                gen,
                best.Result.PipelineDescription,
                best.Result.TotalErrorScore,
                worst.Result.TotalErrorScore,
                elapsed,
                _elapsedTotal.Value);

            if (best.Result.TotalErrorScore < 0.05)
            {
                LogEvolutionSuccess(logger, gen, best.Result.TotalErrorScore);

                // ReSharper disable once HeapView.ObjectAllocation.Evident
                // Will only allocate once when we have broken the hot allocation loop.
                return new(best.Result, best.Genome, gen, _elapsedTotal.Value);
            }

            FillNextGeneration(ref generationsSinceLastImprovement, gen, best, rankedResults, nextGen);

            // Swap buffers - next generation becomes current
            (currentGen, nextGen) = (nextGen, currentGen);
        }

        LogEvolutionCompleted(logger, _hyperparameters.MaxGenerations);

        return null;
    }

    private void FillNextGeneration(
        ref int generationsSinceLastImprovement,
        int gen,
        (Genome Genome, PipelineResult Result) best,
        (Genome Genome, PipelineResult Result)[] rankedResults,
        Genome[] targetPopulation)
    {
        var popSize = targetPopulation.Length;
        var writeIndex = 0;

        // Check for cataclysm trigger
        if (generationsSinceLastImprovement >= _hyperparameters.StagnationThreshold)
        {
            LogCataclysmTriggered(logger, gen, generationsSinceLastImprovement);

            // Keep only the absolute best genome (the "Noah's Ark" approach)
            targetPopulation[writeIndex++] = best.Genome;

            // Fill the rest of the population with brand new random genomes
            // (fresh genetic material to escape the local optimum)
            while (writeIndex < popSize)
            {
                targetPopulation[writeIndex++] = genomeFactory.CreateRandomGenome(modifierCount: 5);
            }

            // Reset stagnation counter
            generationsSinceLastImprovement = 0;
            return;
        }

        // Standard evolution logic

        // Elitism: Keep top 10%
        var eliteCount = popSize / 10;

        for (var i = 0; i < eliteCount && writeIndex < popSize; i++)
        {
            targetPopulation[writeIndex++] = rankedResults[i].Genome;
        }

        // Fill the rest with crossover/mutation of the top 50%
        var survivorCount = popSize / 2;
        var survivors = new ReadOnlySpan<(Genome Genome, PipelineResult Result)>(rankedResults, 0, survivorCount);
        var random = randomFactory.GetRandom();

        while (writeIndex < popSize)
        {
            // Select two distinct parents from top 50%
            var parentA = random.NextItem(survivors).Genome;
            var parentB = random.NextItem(survivors).Genome;

            // Try to ensure we aren't breeding a parent with itself
            if (survivors.Length > 1)
            {
                var attempts = 0;
                while (parentB == parentA && attempts < 5)
                {
                    parentB = random.NextItem(survivors).Genome;
                    attempts++;
                }
            }

            // Crossover: Create a child by mixing traits of A and B
            var child = genomeFactory.Crossover(parentA, parentB);

            // Mutation: Apply probabilistically
            if (random.NextDouble() < _hyperparameters.MutationRate)
            {
                child = genomeFactory.Mutate(child);
            }

            targetPopulation[writeIndex++] = child;
        }
    }

    [LoggerMessage(LogLevel.Information,
        "Starting evolution: Seed={Seed}, Population={PopulationSize}, MaxGen={MaxGenerations}, AvailableSourceTexts={SourceTextCount}")]
    static partial void LogEvolutionStarted(ILogger<EvolutionEngine> logger,
        int seed,
        int populationSize,
        int maxGenerations,
        int sourceTextCount);

    [LoggerMessage(LogLevel.Information, "Population initialized with {Count} random genomes")]
    static partial void LogPopulationInitialized(ILogger<EvolutionEngine> logger, int count);

    [LoggerMessage(LogLevel.Information,
        "Gen {gen} ({Elapsed}/{ElapsedTotal}): Best={BestError:F2} (Worst={WorstError:F2}) | {Desc}")]
    static partial void LogGenerationInfo(ILogger<EvolutionEngine> logger,
        int gen,
        string desc,
        double bestError,
        double worstError,
        TimeSpan elapsed,
        TimeSpan elapsedTotal);

    [LoggerMessage(LogLevel.Information, "Evolution succeeded at generation {Gen} with error {Error:F6}")]
    static partial void LogEvolutionSuccess(ILogger<EvolutionEngine> logger, int gen, double error);

    [LoggerMessage(LogLevel.Information, "Evolution completed after {MaxGenerations} generations")]
    static partial void LogEvolutionCompleted(ILogger<EvolutionEngine> logger, int maxGenerations);

    [LoggerMessage(LogLevel.Warning,
        "CATACLYSM TRIGGERED at Gen {Gen}! Stagnation for {Count} generations. Population wiped.")]
    static partial void LogCataclysmTriggered(ILogger<EvolutionEngine> logger, int gen, int count);
}
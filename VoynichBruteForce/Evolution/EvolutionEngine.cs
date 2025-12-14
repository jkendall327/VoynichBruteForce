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
        using var evolutionScope = logger.BeginScope(new Dictionary<string, object>
        {
            ["Seed"] = seed,
            ["PopulationSize"] = _hyperparameters.PopulationSize,
            ["MaxGenerations"] = _hyperparameters.MaxGenerations,
            ["MutationRate"] = _hyperparameters.MutationRate
        });

        LogEvolutionStarted(logger,
            seed,
            _hyperparameters.PopulationSize,
            _hyperparameters.MaxGenerations,
            sourceTextRegistry.AvailableIds.Count);

        // Initialize Population (Gen 0)
        var population = new List<Genome>();

        for (var i = 0; i < _hyperparameters.PopulationSize; i++)
        {
            var genome = genomeFactory.CreateRandomGenome(modifierCount: 5);
            population.Add(genome);
        }

        LogPopulationInitialized(logger, _hyperparameters.PopulationSize);

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
            var rankedResults = new (Genome Genome, PipelineResult Result)[population.Count];

            var start = Stopwatch.GetTimestamp();

            Parallel.For(0,
                population.Count,
                options,
                i =>
                {
                    var genome = population[i];

                    // Resolve genome to pipeline at evaluation time
                    var sourceText = sourceTextRegistry.GetText(genome.SourceTextId);
                    var pipeline = new Pipeline(sourceText, genome.Modifiers);
                    var result = runner.Run(pipeline, genome.SourceTextId);
                    rankedResults[i] = (genome, result);
                });

            var elapsed = Stopwatch.GetElapsedTime(start);

            if (_elapsedTotal is null)
            {
                _elapsedTotal = elapsed;
            }
            else
            {
                _elapsedTotal += elapsed;
            }

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

                return new(best.Result, best.Genome, gen, _elapsedTotal.Value);
            }

            population = PerformSelectionAndReproduction(ref generationsSinceLastImprovement, gen, best, rankedResults);
        }

        LogEvolutionCompleted(logger, _hyperparameters.MaxGenerations);

        return null;
    }

    private List<Genome> PerformSelectionAndReproduction(ref int generationsSinceLastImprovement,
        int gen,
        (Genome Genome, PipelineResult Result) best,
        (Genome Genome, PipelineResult Result)[] rankedResults)
    {
        // 3. Selection & Reproduction
        var nextGen = new List<Genome>();

        // Check for cataclysm trigger
        if (generationsSinceLastImprovement >= _hyperparameters.StagnationThreshold)
        {
            LogCataclysmTriggered(logger, gen, generationsSinceLastImprovement);

            // Keep only the absolute best genome (the "Noah's Ark" approach)
            nextGen.Add(best.Genome);

            // Fill the rest of the population with brand new random genomes
            // (fresh genetic material to escape the local optimum)
            while (nextGen.Count < _hyperparameters.PopulationSize)
            {
                nextGen.Add(genomeFactory.CreateRandomGenome(modifierCount: 5));
            }

            // Reset stagnation counter
            generationsSinceLastImprovement = 0;
        }
        else
        {
            // Standard evolution logic

            // Elitism: Add top 10%
            var eliteCount = _hyperparameters.PopulationSize / 10;

            for (var i = 0; i < eliteCount; i++)
            {
                nextGen.Add(rankedResults[i].Genome);
            }

            // Fill the rest with mutations of the top 50%
            var survivorCount = _hyperparameters.PopulationSize / 2;

            var survivors =
                new ReadOnlySpan<(Genome Genome, PipelineResult Result)>(rankedResults, 0, survivorCount);

            // TODO: we previously did this to 'ensure randomness varies per gen'.
            // But was this actually meaningful? I'm not sure.
            // Trying without for now to reduce allocations.
            // var random = new Random(seed + gen);
            var random = randomFactory.GetRandom();
            
            while (nextGen.Count < _hyperparameters.PopulationSize)
            {
                // STEP A: Select two distinctive parents
                // (Using random selection from the top 50% is a simple, effective strategy)
                var parentA = random.NextItem(survivors)
                    .Genome;

                var parentB = random.NextItem(survivors)
                    .Genome;

                // Try to ensure we aren't breeding a parent with itself,
                // though in small pools it happens.
                if (survivors.Length > 1)
                {
                    while (parentB == parentA)
                    {
                        parentB = random.NextItem(survivors)
                            .Genome;
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
        }

        return nextGen;
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
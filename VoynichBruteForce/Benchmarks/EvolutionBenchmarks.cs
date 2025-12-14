using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VoynichBruteForce.Evolution;
using VoynichBruteForce.Sources;

namespace VoynichBruteForce.Benchmarks;

[MemoryDiagnoser]
[GcServer(true)]
public class EvolutionBenchmarks
{
    private IHost? _host;
    private EvolutionEngine? _engine;
    private PipelineRunner? _runner;
    private ISourceTextRegistry? _sourceTextRegistry;
    private IGenomeFactory? _genomeFactory;
    private string? _sourceText;
    private Genome? _genome;
    private int _seed;

    [GlobalSetup]
    public void Setup()
    {
        var builder = Host.CreateApplicationBuilder();

        builder.Services.AddVoynichServices();
        builder.Services.AddVoynichConfiguration(builder.Configuration);

        // Disable console logging for benchmarks
        builder.Logging.ClearProviders();

        _host = builder.Build();

        _engine = _host.Services.GetRequiredService<EvolutionEngine>();
        _runner = _host.Services.GetRequiredService<PipelineRunner>();
        _sourceTextRegistry = _host.Services.GetRequiredService<ISourceTextRegistry>();
        _genomeFactory = _host.Services.GetRequiredService<IGenomeFactory>();

        var settings = _host.Services.GetRequiredService<IOptions<AppSettings>>();
        _seed = settings.Value.Seed;

        // Setup test data for individual operations
        _genome = _genomeFactory.CreateRandomGenome(modifierCount: 5);
        _sourceText = _sourceTextRegistry.GetText(_genome.SourceTextId);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _host?.Dispose();
    }

    [Benchmark(Description = "Single pipeline execution")]
    public PipelineResult RunSinglePipeline()
    {
        return _runner!.Run(_genome!, _sourceText!);
    }

    [Benchmark(Description = "Single generation evaluation (population of 100)")]
    public void RunSingleGeneration()
    {
        var population = new List<Genome>();
        for (var i = 0; i < 500; i++)
        {
            population.Add(_genomeFactory!.CreateRandomGenome(modifierCount: 5));
        }

        var rankedResults = new (Genome Genome, PipelineResult Result)[population.Count];

        Parallel.For(0, population.Count, i =>
        {
            var genome = population[i];
            var sourceText = _sourceTextRegistry!.GetText(genome.SourceTextId);
            var result = _runner!.Run(_genome!, sourceText!);
            rankedResults[i] = (genome, result);
        });
    }

    [Benchmark(Description = "Genome factory - create random genome")]
    public Genome CreateRandomGenome()
    {
        return _genomeFactory!.CreateRandomGenome(modifierCount: 5);
    }

    [Benchmark(Description = "Genome factory - mutation")]
    public Genome MutateGenome()
    {
        return _genomeFactory!.Mutate(_genome!);
    }

    [Benchmark(Description = "Genome factory - crossover")]
    public Genome CrossoverGenomes()
    {
        var genome2 = _genomeFactory!.CreateRandomGenome(modifierCount: 5);
        return _genomeFactory.Crossover(_genome!, genome2);
    }
}

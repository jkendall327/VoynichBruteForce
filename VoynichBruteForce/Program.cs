using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using VoynichBruteForce;
using VoynichBruteForce.Benchmarks;
using VoynichBruteForce.Evolution;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddVoynichServices();
builder.Services.AddVoynichConfiguration(builder.Configuration);

var app = builder.Build();

var settings = app.Services.GetRequiredService<IOptions<AppSettings>>();

if (settings.Value.RunBenchmark)
{
    EvolutionBenchmarkRunner.Run();
    return;
}

var engine = app.Services.GetRequiredService<EvolutionEngine>();

var result = engine.Evolve(settings.Value.Seed);

if (result is not null)
{
    var json = System.Text.Json.JsonSerializer.Serialize(result);
    File.WriteAllText("result.json", json);
}
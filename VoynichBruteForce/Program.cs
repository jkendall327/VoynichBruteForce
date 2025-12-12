using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using VoynichBruteForce;
using VoynichBruteForce.Evolution;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddVoynichServices();
builder.Services.AddVoynichConfiguration(builder.Configuration);

var app = builder.Build();

var settings = app.Services.GetRequiredService<IOptions<AppSettings>>();

var engine = app.Services.GetRequiredService<EvolutionEngine>();

engine.Evolve(settings.Value.Seed);
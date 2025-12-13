using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VoynichBruteForce.Evolution;

namespace VoynichBruteForce.Tests;

public class EvolutionEngineTests
{
    [Fact]
    public void EvolutionRunConcludesSuccessfully()
    {
        var services = new ServiceCollection();
        var configuration = BuildTestConfiguration();

        services.AddLogging();

        services.AddVoynichConfiguration(configuration);
        services.AddVoynichServices();

        var serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });

        var engine = serviceProvider.GetRequiredService<EvolutionEngine>();

        var result = engine.Evolve(1);

        // Result may be null if no sufficiently good solution was found within MaxGenerations
        // This test simply verifies that evolution completes successfully
    }

    private static IConfiguration BuildTestConfiguration()
    {
        var configData = new Dictionary<string, string?>
        {
            [$"{AppSettings.SectionName}:Seed"] = "42",
            [$"{AppSettings.SectionName}:DegreeOfParallelism"] = "10",
            [$"{Hyperparameters.SectionName}:MaxGenerations"] = "10",
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();
    }
}
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VoynichBruteForce.Evolution;
using VoynichBruteForce.Rankings;
using VoynichBruteForce.Sources;

namespace VoynichBruteForce.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddVoynichServices_ShouldBuildServiceProviderSuccessfully()
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

        // Verify we can resolve key services without errors
        Assert.NotNull(serviceProvider.GetRequiredService<IGenomeFactory>());
        Assert.NotNull(serviceProvider.GetRequiredService<ISourceTextRegistry>());
        Assert.NotNull(serviceProvider.GetRequiredService<IRankerProvider>());
        Assert.NotNull(serviceProvider.GetRequiredService<PipelineRunner>());
        Assert.NotNull(serviceProvider.GetRequiredService<EvolutionEngine>());
    }

    private static IConfiguration BuildTestConfiguration()
    {
        var configData = new Dictionary<string, string?>
        {
            [$"{AppSettings.SectionName}:Seed"] = "42"
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();
    }
}

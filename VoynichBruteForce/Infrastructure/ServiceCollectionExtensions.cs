using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VoynichBruteForce.Evolution;
using VoynichBruteForce.Rankings;
using VoynichBruteForce.Sources;

namespace VoynichBruteForce;

public static class ServiceCollectionExtensions
{
    public static void AddVoynichServices(this IServiceCollection services)
    {
        services.AddSingleton<IGenomeFactory, DefaultGenomeFactory>();
        services.AddSingleton<ISourceTextRegistry, DefaultSourceTextRegistry>();
        services.AddSingleton<IRankerProvider, DefaultRankerProvider>();
        services.AddSingleton<PipelineRunner>();
        services.AddSingleton<EvolutionEngine>();
    }

    public static void AddVoynichConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ServiceProviderOptions>(options =>
        {
            options.ValidateOnBuild = true;
            options.ValidateScopes = true;
        });

        services
            .AddOptions<AppSettings>()
            .Bind(configuration.GetSection(AppSettings.SectionName))
            .ValidateOnStart();

        services
            .AddOptions<VoynichProfile>()
            .Bind(configuration.GetSection(VoynichProfile.SectionName))
            .ValidateOnStart();

        services
            .AddOptions<Hyperparameters>()
            .Bind(configuration.GetSection(Hyperparameters.SectionName))
            .ValidateOnStart();
    }
}
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VoynichBruteForce.Evolution;
using VoynichBruteForce.Modifications;
using VoynichBruteForce.Rankings;
using VoynichBruteForce.Sources;

namespace VoynichBruteForce;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddVoynichServices()
        {
            services.AddSingleton<IGenomeFactory, DefaultGenomeFactory>();
            services.AddSingleton<ISourceTextRegistry, DefaultSourceTextRegistry>();
            services.AddSingleton<IRankerProvider, DefaultRankerProvider>();
            services.AddSingleton<RandomFactory>();
            services.AddSingleton<PipelineRunner>();
            services.AddSingleton<EvolutionEngine>();

            services.AddSingleton<IModifierFactory, CaesarCipherModifierFactory>();
            services.AddSingleton<IModifierFactory, AtbashCipherModifierFactory>();
            services.AddSingleton<IModifierFactory, VowelRemovalModifierFactory>();
            services.AddSingleton<IModifierFactory, PositionalExtractionModifierFactory>();
            services.AddSingleton<IModifierFactory, NullInsertionModifierFactory>();
            services.AddSingleton<IModifierFactory, LetterDoublingModifierFactory>();
            services.AddSingleton<IModifierFactory, AnagramModifierFactory>();
            services.AddSingleton<IModifierFactory, AffixModifierFactory>();
            services.AddSingleton<IModifierFactory, ConsonantVowelSplitModifierFactory>();
            services.AddSingleton<IModifierFactory, ColumnarTranspositionModifierFactory>();
            services.AddSingleton<IModifierFactory, SkipCipherModifierFactory>();
            services.AddSingleton<IModifierFactory, InterleaveModifierFactory>();
            services.AddSingleton<IModifierFactory, WordReversalModifierFactory>();
            services.AddSingleton<IModifierFactory, ReverseInterleaveModifierFactory>();
        }

        public void AddVoynichConfiguration(IConfiguration configuration)
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
}
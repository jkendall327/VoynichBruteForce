using Microsoft.Extensions.Options;
using VoynichBruteForce.Modifications;
using VoynichBruteForce.Rankings;
using VoynichBruteForce.Sources;

namespace VoynichBruteForce.Evolution;

public class PipelineRunner(IRankerProvider rankerProvider, IOptions<Hyperparameters> hyperparameters)
{
    private readonly Hyperparameters _hyperparameters = hyperparameters.Value;

    public PipelineResult Run(Genome genome, string sourceText)
    {
        (var sourceTextId, var modifiers) = (genome.SourceTextId, genome.Modifiers);

        var spanModifiers = modifiers
            .OfType<ISpanTextModifier>()
            .ToList();

        var context = new ProcessingContext(sourceText, sourceText.Length * 4);

        try
        {
            foreach (var modifier in spanModifiers)
            {
                modifier.Modify(ref context);
            }

            var resultLength = context.CurrentLength;

            if (resultLength < 100)
            {
                return new(sourceTextId, modifiers, [], _hyperparameters)
                {
                    TotalErrorScore = double.MaxValue
                };
            }

            using var spanAnalysis = new PrecomputedTextAnalysis(context.InputSpan);

            return RunRankers(spanAnalysis, sourceTextId, modifiers);
        }
        finally
        {
            context.Dispose();
        }
    }

    private PipelineResult RunRankers(PrecomputedTextAnalysis analysis,
        SourceTextId sourceTextId,
        List<ITextModifier> modifiers)
    {
        var rankers = rankerProvider.GetRankers();

        var results = rankers
            .Select(ranker => ranker.CalculateRank(analysis))
            .ToList();

        return new(sourceTextId, modifiers, results, _hyperparameters);
    }
}
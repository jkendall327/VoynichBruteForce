using Microsoft.Extensions.Logging;
using VoynichBruteForce.Modifications;
using VoynichBruteForce.Rankings;

namespace VoynichBruteForce.Evolution;

public class PipelineRunner(IRankerProvider rankerProvider, ILogger<PipelineRunner> logger)
{
    public PipelineResult Run(Pipeline pipeline)
    {
        (var sourceText, var modifiers) = pipeline;

        // Separate modifiers: Span-capable run first, then string-only
        var spanModifiers = modifiers.OfType<ISpanTextModifier>().ToList();
        var stringOnlyModifiers = modifiers.Where(m => m is not ISpanTextModifier).ToList();

        string resultText;

        // Phase 1: Run Span modifiers with zero-allocation ping-pong buffers
        if (spanModifiers.Count > 0)
        {
            // Estimate max capacity: 4x for growth scenarios (affixes, letter doubling, etc.)
            var context = new ProcessingContext(sourceText, sourceText.Length * 4);
            try
            {
                foreach (var modifier in spanModifiers)
                {
                    modifier.Modify(ref context);
                }

                resultText = context.InputSpan.ToString();
            }
            finally
            {
                context.Dispose();
            }
        }
        else
        {
            resultText = sourceText;
        }

        // Phase 2: Run string-only modifiers (e.g., HomophonicSubstitution, ReverseInterleave)
        foreach (var modifier in stringOnlyModifiers)
        {
            resultText = modifier.ModifyText(resultText);
        }

        // Sanity check - prevent degenerate optimisation for empty texts by returning max error immediately.
        if (resultText.Length < 100)
        {
            return new(modifiers, [])
            {
                TotalErrorScore = double.MaxValue
            };
        }

        var rankers = rankerProvider.GetRankers();

        var results = new List<RankerResult>();

        foreach (var ranker in rankers)
        {
            var result = ranker.CalculateRank(resultText);

            results.Add(result);

            logger.LogTrace("{RankingMethod}: {Error}", ranker.Name, result);
        }

        return new(modifiers, results);
    }
}
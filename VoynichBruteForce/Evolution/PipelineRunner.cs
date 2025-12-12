using Microsoft.Extensions.Logging;
using VoynichBruteForce.Modifications;
using VoynichBruteForce.Rankings;

namespace VoynichBruteForce.Evolution;

public class PipelineRunner(IRankerProvider rankerProvider, ILogger<PipelineRunner> logger)
{
    public PipelineResult Run(Pipeline pipeline)
    {
        (var sourceText, var modifiers) = pipeline;

        // Estimate max capacity: 4x for growth scenarios (affixes, letter doubling, etc.)
        var context = new ProcessingContext(sourceText, sourceText.Length * 4);
        try
        {
            foreach (var modifier in modifiers)
            {
                modifier.Modify(ref context);
            }

            // Only convert to string at the very end for ranking
            var resultText = context.InputSpan.ToString();

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
        finally
        {
            context.Dispose();
        }
    }
}
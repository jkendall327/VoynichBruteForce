using Microsoft.Extensions.Logging;
using VoynichBruteForce.Rankings;

namespace VoynichBruteForce.Evolution;

public class PipelineRunner(IRankerProvider rankerProvider, ILogger<PipelineRunner> logger)
{
    public PipelineResult Run(Pipeline pipeline)
    {
        (var sourceText, var modifiers) = pipeline;

        sourceText = modifiers.Aggregate(sourceText, (current, modifier) => modifier.ModifyText(current));

        // Sanity check - prevent degenerate optimisation for empty texts by returning max error immediately.
        if (sourceText.Length < 100)
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
            var result = ranker.CalculateRank(sourceText);

            results.Add(result);

            logger.LogTrace("{RankingMethod}: {Error}", ranker.Name, result);
        }

        return new(modifiers, results);
    }
}
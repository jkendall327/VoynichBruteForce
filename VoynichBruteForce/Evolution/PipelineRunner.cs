using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VoynichBruteForce.Modifications;
using VoynichBruteForce.Rankings;
using VoynichBruteForce.Sources;

namespace VoynichBruteForce.Evolution;

public partial class PipelineRunner(IRankerProvider rankerProvider, IOptions<Hyperparameters> hyperparameters, ILogger<PipelineRunner> logger)
{
    private readonly Hyperparameters _hyperparameters = hyperparameters.Value;

    public PipelineResult Run(Pipeline pipeline, SourceTextId sourceTextId)
    {
        (var sourceText, var modifiers) = pipeline;

        // Separate modifiers: Span-capable run first, then string-only
        var spanModifiers = modifiers.OfType<ISpanTextModifier>().ToList();
        var stringOnlyModifiers = modifiers.Where(m => m is not ISpanTextModifier).ToList();

        using var pipelineScope = logger.BeginScope(new Dictionary<string, object>
        {
            ["ModifierCount"] = modifiers.Count,
            ["SpanModifiers"] = spanModifiers.Count,
            ["StringModifiers"] = stringOnlyModifiers.Count
        });

        LogPipelineStarted(logger, modifiers.Count, spanModifiers.Count, stringOnlyModifiers.Count, sourceText.Length);

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

        LogTextTransformed(logger, sourceText.Length, resultText.Length);

        // Sanity check - prevent degenerate optimisation for empty texts by returning max error immediately.
        if (resultText.Length < 100)
        {
            LogDegenerateTextDetected(logger, resultText.Length);
            return new(sourceTextId, modifiers, [], _hyperparameters)
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

        return new(sourceTextId, modifiers, results, _hyperparameters);
    }

    [LoggerMessage(LogLevel.Debug, "Pipeline: {totalModifiers} modifiers ({spanModifiers} span, {stringModifiers} string) | SourceLength={sourceLength}")]
    static partial void LogPipelineStarted(ILogger<PipelineRunner> logger, int totalModifiers, int spanModifiers, int stringModifiers, int sourceLength);

    [LoggerMessage(LogLevel.Debug, "Text transformed: {sourceLength} -> {resultLength} chars")]
    static partial void LogTextTransformed(ILogger<PipelineRunner> logger, int sourceLength, int resultLength);

    [LoggerMessage(LogLevel.Warning, "Degenerate text detected: {length} chars (minimum 100)")]
    static partial void LogDegenerateTextDetected(ILogger<PipelineRunner> logger, int length);
}
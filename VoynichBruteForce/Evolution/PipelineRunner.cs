using Microsoft.Extensions.Options;
using VoynichBruteForce.Modifications;
using VoynichBruteForce.Rankings;
using VoynichBruteForce.Sources;

namespace VoynichBruteForce.Evolution;

public class PipelineRunner(IRankerProvider rankerProvider, IOptions<Hyperparameters> hyperparameters)
{
    private readonly Hyperparameters _hyperparameters = hyperparameters.Value;

    public PipelineResult Run(Pipeline pipeline, SourceTextId sourceTextId)
    {
        (var sourceText, var modifiers) = pipeline;

        // Separate modifiers: Span-capable run first, then string-only
        var spanModifiers = modifiers
            .OfType<ISpanTextModifier>()
            .ToList();

        var stringOnlyModifiers = modifiers
            .Where(m => m is not ISpanTextModifier)
            .ToList();

        // Phase 1: Run Span modifiers with zero-allocation ping-pong buffers
        var context = new ProcessingContext(sourceText, sourceText.Length * 4);

        try
        {
            foreach (var modifier in spanModifiers)
            {
                modifier.Modify(ref context);
            }

            // Phase 2: Run string-only modifiers (if any exist)
            // Currently all modifiers implement ISpanTextModifier, so this is typically a no-op
            if (stringOnlyModifiers.Count > 0)
            {
                // Fall back to string for legacy modifiers
                var resultText = context.InputSpan.ToString();

                foreach (var modifier in stringOnlyModifiers)
                {
                    resultText = modifier.ModifyText(resultText);
                }

                if (resultText.Length < 100)
                {
                    return new(sourceTextId, modifiers, [], _hyperparameters)
                    {
                        TotalErrorScore = double.MaxValue
                    };
                }

                // Create analysis from string (legacy path)
                using var stringAnalysis = new PrecomputedTextAnalysis(resultText);

                return RunRankers(stringAnalysis, sourceTextId, modifiers);
            }

            // Fast path: no string allocation needed
            var resultLength = context.CurrentLength;

            if (resultLength < 100)
            {
                return new(sourceTextId, modifiers, [], _hyperparameters)
                {
                    TotalErrorScore = double.MaxValue
                };
            }

            // Create analysis directly from span - NO .ToString()!
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
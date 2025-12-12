namespace VoynichBruteForce.Modifications;

/// <summary>
/// Interleaves characters from the first and second halves of the text,
/// or performs a "riffle shuffle" of the text characters.
/// "ABCDEF" → "ADBECF" (interleaving halves).
///
/// Interleaving text is conceptually similar to shuffling cards - a technique
/// that was certainly possible to perform by hand. The idea of splitting and
/// weaving text requires no special tools, just careful attention.
/// </summary>
public class InterleaveModifier : ISpanTextModifier
{
    private readonly InterleaveMode _mode;

    public string Name => _mode switch
    {
        InterleaveMode.HalvesAlternate => "Interleave(halves)",
        InterleaveMode.OddEvenSplit => "Interleave(odd-even)",
        InterleaveMode.ReverseInterleave => "Interleave(reverse)",
        _ => "Interleave"
    };

    // Moderate cognitive cost - requires tracking two streams
    public CognitiveComplexity CognitiveCost => new(5);

    public InterleaveModifier(InterleaveMode mode = InterleaveMode.HalvesAlternate)
    {
        if (mode == InterleaveMode.ReverseInterleave)
        {
            throw new ArgumentException(
                "ReverseInterleave mode is not supported by InterleaveModifier. Use ReverseInterleaveModifier instead.",
                nameof(mode));
        }
        _mode = mode;
    }

    public string ModifyText(string text) => this.RunWithContext(text);

    public void Modify(ref ProcessingContext context)
    {
        var input = context.InputSpan;
        var output = context.OutputSpan;

        if (input.Length <= 1)
        {
            input.CopyTo(output);
            context.Commit(input.Length);
            return;
        }

        switch (_mode)
        {
            case InterleaveMode.HalvesAlternate:
                InterleaveHalves(input, output, ref context);
                break;
            case InterleaveMode.OddEvenSplit:
                SplitOddEven(input, output, ref context);
                break;
            default:
                input.CopyTo(output);
                context.Commit(input.Length);
                break;
        }
    }

    /// <summary>
    /// Splits text into two halves and interleaves them.
    /// "ABCDEF" → "ADBECF"
    /// </summary>
    private static void InterleaveHalves(ReadOnlySpan<char> input, Span<char> output, ref ProcessingContext context)
    {
        var mid = (input.Length + 1) / 2;
        var firstHalf = input.Slice(0, mid);
        var secondHalf = input.Slice(mid);

        var writeIndex = 0;
        var maxLen = Math.Max(firstHalf.Length, secondHalf.Length);

        for (var i = 0; i < maxLen; i++)
        {
            if (i < firstHalf.Length)
            {
                output[writeIndex++] = firstHalf[i];
            }
            if (i < secondHalf.Length)
            {
                output[writeIndex++] = secondHalf[i];
            }
        }

        context.Commit(writeIndex);
    }

    /// <summary>
    /// Takes odd-indexed characters then even-indexed characters.
    /// "ABCDEF" → "ACEBDF"
    /// </summary>
    private static void SplitOddEven(ReadOnlySpan<char> input, Span<char> output, ref ProcessingContext context)
    {
        var writeIndex = 0;

        // Even indices first (0, 2, 4...)
        for (var i = 0; i < input.Length; i += 2)
        {
            output[writeIndex++] = input[i];
        }

        // Then odd indices (1, 3, 5...)
        for (var i = 1; i < input.Length; i += 2)
        {
            output[writeIndex++] = input[i];
        }

        context.Commit(writeIndex);
    }
}

public enum InterleaveMode
{
    HalvesAlternate,
    OddEvenSplit,
    ReverseInterleave
}

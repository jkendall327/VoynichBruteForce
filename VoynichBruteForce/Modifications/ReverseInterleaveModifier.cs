namespace VoynichBruteForce.Modifications;

/// <summary>
/// Interleaves text with its reverse.
/// "ABC" → "ACBBCA" (original interleaved with reverse)
/// </summary>
public class ReverseInterleaveModifier : ISpanTextModifier
{
    public string Name => "Interleave (reverse)";

    // Moderate cognitive cost - requires tracking two streams
    public CognitiveComplexity CognitiveCost => new(5);

    public string ModifyText(string text) => this.RunWithContext(text);

    public void Modify(ref ProcessingContext context)
    {
        var input = context.InputSpan;
        var n = input.Length;

        if (n == 0)
        {
            return;
        }

        var outLen = n * 2;
        context.EnsureCapacity(outLen);

        var output = context.OutputSpan;

        // "A" -> "AA"
        if (n == 1)
        {
            output[0] = input[0];
            output[1] = input[0];
            context.Commit(2);
            return;
        }

        for (var i = 0; i < n; i++)
        {
            // Even indexes come from forward pass of the input.
            output[2 * i] = input[i];
            
            // Odd indexes from reverse pass of the input.
            output[2 * i + 1] = input[n - 1 - i];
        }

        context.Commit(outLen);
    }
}

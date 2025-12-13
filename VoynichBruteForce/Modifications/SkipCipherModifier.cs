using System.Buffers;

namespace VoynichBruteForce.Modifications;

/// <summary>
/// Extracts every Nth character from the text, wrapping around to collect all characters.
/// With skip=2, "HELLO" becomes "HLOEL" (H,L,O then E,L).
///
/// Skip ciphers (also called decimation ciphers) are a simple form of transposition
/// that requires no tools - just counting. The technique of reading every Nth letter
/// was known in antiquity and could easily be performed by any literate person.
/// </summary>
public class SkipCipherModifier : ISpanTextModifier, IPerturbable
{
    private readonly int _skip;

    public string Name => $"SkipCipher({_skip})";

    // Moderate cognitive cost - requires careful counting
    public CognitiveComplexity CognitiveCost => new(4);

    /// <summary>
    /// Creates a skip cipher modifier.
    /// </summary>
    /// <param name="skip">Take every Nth character. Must be at least 2.</param>
    public SkipCipherModifier(int skip)
    {
        if (skip < 2)
        {
            throw new ArgumentException("Skip must be at least 2", nameof(skip));
        }

        _skip = skip;
    }

    public string ModifyText(string text) => this.RunWithContext(text);

    public void Modify(ref ProcessingContext context)
    {
        var input = context.InputSpan;
        var output = context.OutputSpan;

        if (input.Length == 0)
        {
            context.Commit(0);
            return;
        }

        // Rent a buffer for tracking used positions - thread-safe
        var usedBuffer = ArrayPool<bool>.Shared.Rent(input.Length);
        try
        {
            // Clear the portion we'll use
            Array.Clear(usedBuffer, 0, input.Length);

            var writeIndex = 0;
            var index = 0;
            var collected = 0;

            while (collected < input.Length)
            {
                if (!usedBuffer[index])
                {
                    output[writeIndex++] = input[index];
                    usedBuffer[index] = true;
                    collected++;
                }

                index = (index + _skip) % input.Length;

                // If we've wrapped around and the current position is used,
                // find the next unused position
                if (usedBuffer[index])
                {
                    var startIndex = index;
                    do
                    {
                        index = (index + 1) % input.Length;
                    } while (usedBuffer[index] && index != startIndex);
                }
            }

            context.Commit(writeIndex);
        }
        finally
        {
            ArrayPool<bool>.Shared.Return(usedBuffer);
        }
    }

    public ITextModifier Perturb(Random random)
    {
        // Adjust skip by ±1 (minimum 2)
        var delta = random.NextBool() ? 1 : -1;
        var newSkip = Math.Max(2, _skip + delta);
        return new SkipCipherModifier(newSkip);
    }
}

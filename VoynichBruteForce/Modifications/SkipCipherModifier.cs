namespace VoynichBruteForce.Modifications;

/// <summary>
/// Extracts every Nth character from the text, wrapping around to collect all characters.
/// With skip=2, "HELLO" becomes "HLOEL" (H,L,O then E,L).
///
/// Skip ciphers (also called decimation ciphers) are a simple form of transposition
/// that requires no tools - just counting. The technique of reading every Nth letter
/// was known in antiquity and could easily be performed by any literate person.
/// </summary>
public class SkipCipherModifier : ISpanTextModifier
{
    private readonly int _skip;

    // Scratch buffer for tracking used positions
    private bool[] _usedBuffer = new bool[1024];

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

        // Ensure buffer is large enough and clear it
        if (_usedBuffer.Length < input.Length)
        {
            _usedBuffer = new bool[input.Length];
        }
        else
        {
            Array.Clear(_usedBuffer, 0, input.Length);
        }

        var writeIndex = 0;
        var index = 0;
        var collected = 0;

        while (collected < input.Length)
        {
            if (!_usedBuffer[index])
            {
                output[writeIndex++] = input[index];
                _usedBuffer[index] = true;
                collected++;
            }

            index = (index + _skip) % input.Length;

            // If we've wrapped around and the current position is used,
            // find the next unused position
            if (_usedBuffer[index])
            {
                var startIndex = index;
                do
                {
                    index = (index + 1) % input.Length;
                } while (_usedBuffer[index] && index != startIndex);
            }
        }

        context.Commit(writeIndex);
    }
}

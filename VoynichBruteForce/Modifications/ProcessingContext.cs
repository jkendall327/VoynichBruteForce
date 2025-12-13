using System.Buffers;

namespace VoynichBruteForce.Modifications;

/// <summary>
/// Extension methods for ISpanTextModifier to support the string-based API.
/// </summary>
public static class SpanTextModifierExtensions
{
    /// <summary>
    /// Runs a Span-based modifier using a temporary ProcessingContext and returns the result as a string.
    /// This allows ISpanTextModifier implementations to easily implement ModifyText(string).
    /// </summary>
    public static string RunWithContext(this ISpanTextModifier modifier, string input)
    {
        var context = new ProcessingContext(input, Math.Max(input.Length * 4, 256));
        try
        {
            modifier.Modify(ref context);
            return context.InputSpan.ToString();
        }
        finally
        {
            context.Dispose();
        }
    }
}

/// <summary>
/// A ref struct that manages ping-pong buffers from ArrayPool for zero-allocation
/// text processing. Modifiers read from InputSpan and write to OutputSpan, then
/// call Commit() to swap buffers.
/// </summary>
public ref struct ProcessingContext
{
    private char[] _sourceBuffer;
    private char[] _destinationBuffer;

    /// <summary>
    /// The current length of valid data in the source buffer.
    /// </summary>
    public int CurrentLength { get; private set; }

    /// <summary>
    /// Read-only view of the current text to be processed.
    /// </summary>
    public ReadOnlySpan<char> InputSpan => _sourceBuffer.AsSpan(0, CurrentLength);

    /// <summary>
    /// Writable view where the modifier should write its output.
    /// </summary>
    public Span<char> OutputSpan => _destinationBuffer.AsSpan();

    /// <summary>
    /// The total capacity of the buffers.
    /// </summary>
    public int Capacity => _destinationBuffer.Length;

    /// <summary>
    /// Creates a new ProcessingContext with the initial text.
    /// </summary>
    /// <param name="initialText">The starting text to process.</param>
    /// <param name="maxCapacity">Maximum buffer size (should account for potential growth).</param>
    public ProcessingContext(string initialText, int maxCapacity)
    {
        _sourceBuffer = ArrayPool<char>.Shared.Rent(maxCapacity);
        _destinationBuffer = ArrayPool<char>.Shared.Rent(maxCapacity);
        initialText.CopyTo(_sourceBuffer.AsSpan());
        CurrentLength = initialText.Length;
    }

    /// <summary>
    /// Ensures both ping-pong buffers have at least <paramref name="required"/> capacity.
    /// Copies the current input (CurrentLength chars) into the new source buffer.
    /// </summary>
    public void EnsureCapacity(int required)
    {
        if (required <= Capacity)
        {
            return;
        }

        // Doubling for decent amortisation, similar to List<T> etc.
        var newCapacity = Math.Max(required, Capacity * 2);

        var pool = ArrayPool<char>.Shared;

        var newSource = pool.Rent(newCapacity);
        var newDest = pool.Rent(newCapacity);

        _sourceBuffer.AsSpan(0, CurrentLength).CopyTo(newSource);

        var oldSource = _sourceBuffer;
        var oldDest = _destinationBuffer;

        _sourceBuffer = newSource;
        _destinationBuffer = newDest;

        pool.Return(oldSource);
        pool.Return(oldDest);
    }

    /// <summary>
    /// Called by a modifier after it finishes writing to OutputSpan.
    /// This swaps the buffers so the next modifier reads from the output of the previous one.
    /// </summary>
    /// <param name="newLength">The length of data written to OutputSpan.</param>
    public void Commit(int newLength)
    {
        CurrentLength = newLength;
        (_sourceBuffer, _destinationBuffer) = (_destinationBuffer, _sourceBuffer);
    }

    /// <summary>
    /// Returns the rented buffers to the ArrayPool.
    /// </summary>
    public void Dispose()
    {
        if (_sourceBuffer != null)
        {
            ArrayPool<char>.Shared.Return(_sourceBuffer);
            _sourceBuffer = null!;
        }

        if (_destinationBuffer != null)
        {
            ArrayPool<char>.Shared.Return(_destinationBuffer);
            _destinationBuffer = null!;
        }
    }
}

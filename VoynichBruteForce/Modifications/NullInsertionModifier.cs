namespace VoynichBruteForce.Modifications;

/// <summary>
/// Inserts "null" characters at regular intervals throughout the text.
/// The null character and interval are configurable.
///
/// Nulls (meaningless characters inserted to confuse cryptanalysis) were a
/// well-documented technique in medieval and Renaissance cryptography.
/// They appear in correspondence of the Italian city-states and the Vatican.
/// This technique was described by Leon Battista Alberti in his cryptographic
/// treatise "De componendis cifris" (1467).
/// </summary>
public class NullInsertionModifier : ISpanTextModifier, IPerturbable
{
    private readonly char _nullChar;
    private readonly int _interval;

    public string Name => $"NullInsertion('{_nullChar}',{_interval})";

    // Moderate cognitive cost - requires counting while writing
    public CognitiveComplexity CognitiveCost => new(4);

    /// <summary>
    /// Creates a null insertion modifier.
    /// </summary>
    /// <param name="nullChar">The character to insert as a null.</param>
    /// <param name="interval">Insert a null after every N characters.</param>
    public NullInsertionModifier(char nullChar, int interval)
    {
        if (interval < 1)
        {
            throw new ArgumentException("Interval must be at least 1", nameof(interval));
        }

        _nullChar = nullChar;
        _interval = interval;
    }

    public string ModifyText(string text) => this.RunWithContext(text);

    public void Modify(ref ProcessingContext context)
    {
        var input = context.InputSpan;

        if (input.Length == 0)
        {
            return;
        }

        // Calculate max expansion: each interval chars adds one null char
        // Worst case: input.Length + ceil(input.Length / interval)
        var nullCount = (input.Length + _interval - 1) / _interval;
        var maxOutputLength = input.Length + nullCount;
        context.EnsureCapacity(maxOutputLength);

        var output = context.OutputSpan;
        var writeIndex = 0;
        var count = 0;

        for (var i = 0; i < input.Length; i++)
        {
            output[writeIndex++] = input[i];
            count++;

            if (count >= _interval)
            {
                output[writeIndex++] = _nullChar;
                count = 0;
            }
        }

        context.Commit(writeIndex);
    }

    public ITextModifier Perturb(Random random)
    {
        // Either adjust interval by ±1 or shift the null char by ±1
        if (random.NextBool())
        {
            // Adjust interval (minimum 1)
            var delta = random.NextBool() ? 1 : -1;
            var newInterval = Math.Max(1, _interval + delta);
            return new NullInsertionModifier(_nullChar, newInterval);
        }
        else
        {
            // Shift null char by ±1 within a-z range
            var delta = random.NextBool() ? 1 : -1;
            var newChar = (char)('a' + (((_nullChar - 'a') + delta) % 26 + 26) % 26);
            return new NullInsertionModifier(newChar, _interval);
        }
    }
}

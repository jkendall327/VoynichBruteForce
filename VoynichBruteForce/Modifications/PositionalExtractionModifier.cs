namespace VoynichBruteForce.Modifications;

/// <summary>
/// Extracts characters from specific positions within each word.
/// Can extract first letters (acrostic), last letters (telestich),
/// or Nth letters from each word.
///
/// Acrostics and similar positional encodings were extremely popular
/// in classical and medieval literature. They appear in the Hebrew Bible,
/// Greek poetry, and throughout medieval manuscripts. Any educated scholar
/// would be familiar with composing and reading acrostics.
/// </summary>
public class PositionalExtractionModifier : ITextModifier
{
    private readonly int _position;
    private readonly bool _fromEnd;

    public string Name => _fromEnd
        ? $"PositionalExtraction(end-{_position})"
        : $"PositionalExtraction({_position})";

    // Low cognitive cost - simple position identification
    public CognitiveComplexity CognitiveCost => new(2);

    /// <summary>
    /// Creates a positional extraction modifier.
    /// </summary>
    /// <param name="position">
    /// The position to extract (0-indexed).
    /// 0 = first letter, 1 = second letter, etc.
    /// </param>
    /// <param name="fromEnd">
    /// If true, count from the end of the word instead of the beginning.
    /// </param>
    public PositionalExtractionModifier(int position = 0, bool fromEnd = false)
    {
        if (position < 0)
        {
            throw new ArgumentException("Position must be non-negative", nameof(position));
        }

        _position = position;
        _fromEnd = fromEnd;
    }

    /// <summary>
    /// Creates an acrostic extractor (first letter of each word).
    /// </summary>
    public static PositionalExtractionModifier Acrostic() => new(0, false);

    /// <summary>
    /// Creates a telestich extractor (last letter of each word).
    /// </summary>
    public static PositionalExtractionModifier Telestich() => new(0, true);

    public void Modify(ref ProcessingContext context)
    {
        var input = context.InputSpan;
        var output = context.OutputSpan;
        var writeIndex = 0;
        var wordStart = -1;

        for (var i = 0; i <= input.Length; i++)
        {
            var isWordChar = i < input.Length && char.IsLetterOrDigit(input[i]);

            if (isWordChar && wordStart < 0)
            {
                wordStart = i;
            }
            else if (!isWordChar && wordStart >= 0)
            {
                var wordEnd = i;

                // Extract from this word
                int extractIndex;

                if (_fromEnd)
                {
                    extractIndex = wordEnd - 1 - _position;
                }
                else
                {
                    extractIndex = wordStart + _position;
                }

                if (extractIndex >= wordStart && extractIndex < wordEnd)
                {
                    output[writeIndex++] = input[extractIndex];
                }

                wordStart = -1;
            }
        }

        context.Commit(writeIndex);
    }
}

namespace VoynichBruteForce.Modifications;

/// <summary>
/// Doubles specific letters or all letters in the text.
/// "hello" with vowel doubling becomes "heelloo".
///
/// Letter doubling was a common feature in medieval manuscripts, both as
/// scribal convention and as a simple encoding technique. Italian scribes
/// often doubled consonants in ways that differed from Latin conventions.
/// This simple expansion technique requires no tools or memorization.
/// </summary>
public class LetterDoublingModifier : ISpanTextModifier
{
    private readonly HashSet<char>? _lettersToDouble;

    public string Name =>
        _lettersToDouble == null ? "LetterDoubling(all)" : $"LetterDoubling({string.Join(",", _lettersToDouble)})";

    // Low cognitive cost - simple repetition
    public CognitiveComplexity CognitiveCost => new(2);

    /// <summary>
    /// Creates a letter doubling modifier.
    /// </summary>
    /// <param name="lettersToDouble">
    /// Specific letters to double, or null to double all letters.
    /// </param>
    public LetterDoublingModifier(IEnumerable<char>? lettersToDouble = null)
    {
        _lettersToDouble = lettersToDouble?.ToHashSet(CharComparer.Instance);
    }

    public string ModifyText(string text) => this.RunWithContext(text);

    public void Modify(ref ProcessingContext context)
    {
        var input = context.InputSpan;

        if (input.Length == 0)
        {
            return;
        }

        // Max growth: each letter could double -> 2x length
        context.EnsureCapacity(input.Length * 2);

        var output = context.OutputSpan;
        var writeIndex = 0;

        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];
            output[writeIndex++] = c;

            if (char.IsLetter(c) && ShouldDouble(c))
            {
                output[writeIndex++] = c;
            }
        }

        context.Commit(writeIndex);
    }

    private bool ShouldDouble(char c)
    {
        if (_lettersToDouble == null)
        {
            return true;
        }

        return _lettersToDouble.Contains(c);
    }

    private class CharComparer : IEqualityComparer<char>
    {
        public static readonly CharComparer Instance = new();

        public bool Equals(char x, char y) => char.ToLowerInvariant(x) == char.ToLowerInvariant(y);

        public int GetHashCode(char obj) =>
            char
                .ToLowerInvariant(obj)
                .GetHashCode();
    }
}
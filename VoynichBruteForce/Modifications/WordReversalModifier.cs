namespace VoynichBruteForce.Modifications;

/// <summary>
/// Reverses each word in the text while preserving word boundaries.
/// "hello world" becomes "olleh dlrow".
///
/// This is a simple transformation that requires no memorization or tools,
/// just careful attention. Mirror writing was famously used by Leonardo da Vinci
/// in the same period. Palindromes and word reversals were popular among
/// Renaissance scholars as puzzles and in mystical/Kabbalistic contexts.
/// </summary>
public class WordReversalModifier : ISpanTextModifier
{
    public string Name => "WordReversal";

    // Moderate cognitive cost - requires attention but no memorization
    public CognitiveComplexity CognitiveCost => new(3);

    public string ModifyText(string text) => this.RunWithContext(text);

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
                // Starting a new word
                wordStart = i;
            }
            else if (!isWordChar && wordStart >= 0)
            {
                // End of word - reverse and write
                for (var j = i - 1; j >= wordStart; j--)
                {
                    output[writeIndex++] = input[j];
                }
                wordStart = -1;

                // Write the non-word character
                if (i < input.Length)
                {
                    output[writeIndex++] = input[i];
                }
            }
            else if (!isWordChar && i < input.Length)
            {
                // Non-word character outside a word
                output[writeIndex++] = input[i];
            }
        }

        context.Commit(writeIndex);
    }
}

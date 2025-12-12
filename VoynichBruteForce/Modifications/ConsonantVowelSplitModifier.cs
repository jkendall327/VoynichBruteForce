namespace VoynichBruteForce.Modifications;

/// <summary>
/// Separates text into consonants and vowels, outputting all consonants
/// followed by all vowels (or vice versa).
/// "HELLO" becomes "HLLЕО" (consonants: HLL, vowels: EO).
///
/// The distinction between vowels and consonants was fundamental to
/// classical and medieval grammar. This separation technique requires
/// no tools and leverages basic linguistic knowledge that any educated
/// person would possess.
/// </summary>
public class ConsonantVowelSplitModifier : ISpanTextModifier
{
    private readonly bool _consonantsFirst;

    private static readonly HashSet<char> Vowels = new()
    {
        'a', 'e', 'i', 'o', 'u',
        'A', 'E', 'I', 'O', 'U'
    };

    // Scratch buffers - reused to avoid allocations
    private char[] _consonantBuffer = new char[1024];
    private char[] _vowelBuffer = new char[1024];
    private (int Index, char Char)[] _nonLetterBuffer = new (int, char)[256];

    public string Name => _consonantsFirst ? "ConsonantVowelSplit(C,V)" : "ConsonantVowelSplit(V,C)";

    // Low cognitive cost - simple categorization
    public CognitiveComplexity CognitiveCost => new(2);

    /// <summary>
    /// Creates a consonant-vowel split modifier.
    /// </summary>
    /// <param name="consonantsFirst">If true, consonants come first; otherwise vowels first.</param>
    public ConsonantVowelSplitModifier(bool consonantsFirst = true)
    {
        _consonantsFirst = consonantsFirst;
    }

    public string ModifyText(string text) => this.RunWithContext(text);

    public void Modify(ref ProcessingContext context)
    {
        var input = context.InputSpan;
        var output = context.OutputSpan;

        // Ensure buffers are large enough
        if (_consonantBuffer.Length < input.Length)
        {
            _consonantBuffer = new char[input.Length];
            _vowelBuffer = new char[input.Length];
        }
        if (_nonLetterBuffer.Length < input.Length)
        {
            _nonLetterBuffer = new (int, char)[input.Length];
        }

        var consonantCount = 0;
        var vowelCount = 0;
        var nonLetterCount = 0;
        var letterIndex = 0;

        // First pass: categorize characters
        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];
            if (char.IsLetter(c))
            {
                if (Vowels.Contains(c))
                {
                    _vowelBuffer[vowelCount++] = c;
                }
                else
                {
                    _consonantBuffer[consonantCount++] = c;
                }
                letterIndex++;
            }
            else
            {
                _nonLetterBuffer[nonLetterCount++] = (letterIndex, c);
            }
        }

        // Build combined letter sequence
        var lettersSpan = _consonantsFirst
            ? _consonantBuffer.AsSpan(0, consonantCount)
            : _vowelBuffer.AsSpan(0, vowelCount);
        var secondSpan = _consonantsFirst
            ? _vowelBuffer.AsSpan(0, vowelCount)
            : _consonantBuffer.AsSpan(0, consonantCount);

        var totalLetters = consonantCount + vowelCount;

        // If no non-letters, just copy the reordered letters
        if (nonLetterCount == 0)
        {
            lettersSpan.CopyTo(output);
            secondSpan.CopyTo(output.Slice(lettersSpan.Length));
            context.Commit(totalLetters);
            return;
        }

        // Reinsert non-letters at their relative positions
        var writeIndex = 0;
        var letterPos = 0;
        var nonLetterPos = 0;

        for (var i = 0; i < input.Length; i++)
        {
            if (nonLetterPos < nonLetterCount && _nonLetterBuffer[nonLetterPos].Index == letterPos)
            {
                output[writeIndex++] = _nonLetterBuffer[nonLetterPos].Char;
                nonLetterPos++;
            }
            else if (letterPos < totalLetters)
            {
                // Get letter from combined sequence
                if (letterPos < lettersSpan.Length)
                {
                    output[writeIndex++] = lettersSpan[letterPos];
                }
                else
                {
                    output[writeIndex++] = secondSpan[letterPos - lettersSpan.Length];
                }
                letterPos++;
            }
        }

        // Append any remaining letters
        while (letterPos < totalLetters)
        {
            if (letterPos < lettersSpan.Length)
            {
                output[writeIndex++] = lettersSpan[letterPos];
            }
            else
            {
                output[writeIndex++] = secondSpan[letterPos - lettersSpan.Length];
            }
            letterPos++;
        }

        context.Commit(writeIndex);
    }
}

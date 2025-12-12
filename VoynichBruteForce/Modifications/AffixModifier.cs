namespace VoynichBruteForce.Modifications;

/// <summary>
/// Adds prefixes or suffixes to words, or moves parts of words around
/// (similar to Pig Latin transformations).
///
/// Affixation and word transformation games have ancient roots. Latin
/// itself uses extensive prefixes and suffixes, and medieval scholars
/// were intimately familiar with morphological manipulation. Language
/// games involving syllable movement existed across many cultures.
/// </summary>
public class AffixModifier : ITextModifier
{
    private readonly string? _prefix;
    private readonly string? _suffix;
    private readonly AffixMode _mode;

    private static readonly HashSet<char> Vowels = new()
    {
        'a', 'e', 'i', 'o', 'u', 'A', 'E', 'I', 'O', 'U'
    };

    public string Name => _mode switch
    {
        AffixMode.AddPrefix => $"Affix(+{_prefix})",
        AffixMode.AddSuffix => $"Affix({_suffix}+)",
        AffixMode.MoveFirstToEnd => "Affix(first→end)",
        AffixMode.MoveLastToStart => "Affix(last→start)",
        AffixMode.PigLatin => "Affix(PigLatin)",
        _ => "Affix"
    };

    // Low to moderate cognitive cost depending on mode
    public CognitiveComplexity CognitiveCost => _mode switch
    {
        AffixMode.AddPrefix => new(1),
        AffixMode.AddSuffix => new(1),
        AffixMode.MoveFirstToEnd => new(2),
        AffixMode.MoveLastToStart => new(2),
        AffixMode.PigLatin => new(3),
        _ => new(2)
    };

    public AffixModifier(AffixMode mode, string? affix = null)
    {
        _mode = mode;

        switch (mode)
        {
            case AffixMode.AddPrefix:
                _prefix = affix ?? throw new ArgumentNullException(nameof(affix));
                break;
            case AffixMode.AddSuffix:
                _suffix = affix ?? throw new ArgumentNullException(nameof(affix));
                break;
        }
    }

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
                var wordSpan = input.Slice(wordStart, i - wordStart);
                writeIndex = TransformWord(wordSpan, output, writeIndex);
                wordStart = -1;

                if (i < input.Length)
                {
                    output[writeIndex++] = input[i];
                }
            }
            else if (!isWordChar && i < input.Length)
            {
                output[writeIndex++] = input[i];
            }
        }

        context.Commit(writeIndex);
    }

    private int TransformWord(ReadOnlySpan<char> word, Span<char> output, int writeIndex)
    {
        if (word.Length == 0)
        {
            return writeIndex;
        }

        switch (_mode)
        {
            case AffixMode.AddPrefix:
                // Write prefix then word
                _prefix!.CopyTo(output.Slice(writeIndex));
                writeIndex += _prefix.Length;
                word.CopyTo(output.Slice(writeIndex));
                writeIndex += word.Length;
                break;

            case AffixMode.AddSuffix:
                // Write word then suffix
                word.CopyTo(output.Slice(writeIndex));
                writeIndex += word.Length;
                _suffix!.CopyTo(output.Slice(writeIndex));
                writeIndex += _suffix.Length;
                break;

            case AffixMode.MoveFirstToEnd:
                if (word.Length > 1)
                {
                    // Write chars 1..end, then first char
                    word.Slice(1).CopyTo(output.Slice(writeIndex));
                    writeIndex += word.Length - 1;
                    output[writeIndex++] = word[0];
                }
                else
                {
                    word.CopyTo(output.Slice(writeIndex));
                    writeIndex += word.Length;
                }
                break;

            case AffixMode.MoveLastToStart:
                if (word.Length > 1)
                {
                    // Write last char, then chars 0..end-1
                    output[writeIndex++] = word[^1];
                    word.Slice(0, word.Length - 1).CopyTo(output.Slice(writeIndex));
                    writeIndex += word.Length - 1;
                }
                else
                {
                    word.CopyTo(output.Slice(writeIndex));
                    writeIndex += word.Length;
                }
                break;

            case AffixMode.PigLatin:
                writeIndex = ApplyPigLatin(word, output, writeIndex);
                break;

            default:
                word.CopyTo(output.Slice(writeIndex));
                writeIndex += word.Length;
                break;
        }

        return writeIndex;
    }

    private static int ApplyPigLatin(ReadOnlySpan<char> word, Span<char> output, int writeIndex)
    {
        if (word.Length == 0)
        {
            return writeIndex;
        }

        // Find first vowel
        var firstVowelIndex = -1;
        for (var i = 0; i < word.Length; i++)
        {
            if (Vowels.Contains(word[i]))
            {
                firstVowelIndex = i;
                break;
            }
        }

        if (firstVowelIndex <= 0)
        {
            // Word starts with vowel or has no vowels - add "ay"
            word.CopyTo(output.Slice(writeIndex));
            writeIndex += word.Length;
            output[writeIndex++] = 'a';
            output[writeIndex++] = 'y';
        }
        else
        {
            // Move consonant cluster to end and add "ay"
            var restLength = word.Length - firstVowelIndex;
            var consonantLength = firstVowelIndex;

            // Preserve case of first letter
            var wasFirstUpper = char.IsUpper(word[0]);

            // Write rest of word (from first vowel)
            for (var i = 0; i < restLength; i++)
            {
                var c = word[firstVowelIndex + i];
                if (i == 0 && wasFirstUpper)
                {
                    c = char.ToUpper(c);
                }
                output[writeIndex++] = c;
            }

            // Write consonant cluster
            for (var i = 0; i < consonantLength; i++)
            {
                var c = word[i];
                if (i == 0 && wasFirstUpper)
                {
                    c = char.ToLower(c);
                }
                output[writeIndex++] = c;
            }

            // Add "ay"
            output[writeIndex++] = 'a';
            output[writeIndex++] = 'y';
        }

        return writeIndex;
    }
}

public enum AffixMode
{
    AddPrefix,
    AddSuffix,
    MoveFirstToEnd,
    MoveLastToStart,
    PigLatin
}

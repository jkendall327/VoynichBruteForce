using System.Buffers;

namespace VoynichBruteForce.Modifications;

/// <summary>
/// Rearranges letters within each word according to a deterministic pattern.
/// Different modes are available: alphabetical sorting, reverse alphabetical,
/// or a seeded shuffle.
///
/// Anagrams were a beloved intellectual pursuit in the Renaissance, used
/// for wordplay, pseudonyms, and encoding. The practice of rearranging
/// letters was well-known to any educated person.
/// </summary>
public class AnagramModifier : ISpanTextModifier
{
    private readonly AnagramMode _mode;
    private readonly int _seed;

    public string Name => _mode switch
    {
        AnagramMode.Alphabetical => "Anagram(alpha)",
        AnagramMode.ReverseAlphabetical => "Anagram(reverse)",
        AnagramMode.Seeded => $"Anagram(seed:{_seed})",
        _ => "Anagram"
    };

    // Moderate cognitive cost - requires attention to rearrange consistently
    public CognitiveComplexity CognitiveCost => _mode switch
    {
        AnagramMode.Alphabetical => new(4),
        AnagramMode.ReverseAlphabetical => new(4),
        AnagramMode.Seeded => new(6),
        _ => new(5)
    };

    public AnagramModifier(AnagramMode mode, int seed = 0)
    {
        _mode = mode;
        _seed = seed;
    }

    public string ModifyText(string text) => this.RunWithContext(text);

    public void Modify(ref ProcessingContext context)
    {
        var input = context.InputSpan;
        var output = context.OutputSpan;
        var writeIndex = 0;
        var wordStart = -1;

        var wordBuffer = ArrayPool<char>.Shared.Rent(Math.Max(256, input.Length));
        try
        {
            for (var i = 0; i <= input.Length; i++)
            {
                var isWordChar = i < input.Length && char.IsLetterOrDigit(input[i]);

                if (isWordChar && wordStart < 0)
                {
                    wordStart = i;
                }
                else if (!isWordChar && wordStart >= 0)
                {
                    var wordLength = i - wordStart;

                    // Copy word to buffer
                    input.Slice(wordStart, wordLength).CopyTo(wordBuffer);

                    // Anagram the word in the buffer
                    AnagramWord(wordBuffer.AsSpan(0, wordLength));

                    // Write anagrammed word to output
                    wordBuffer.AsSpan(0, wordLength).CopyTo(output.Slice(writeIndex));
                    writeIndex += wordLength;

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
        }
        finally
        {
            ArrayPool<char>.Shared.Return(wordBuffer);
        }

        context.Commit(writeIndex);
    }

    private void AnagramWord(Span<char> chars)
    {
        switch (_mode)
        {
            case AnagramMode.Alphabetical:
                chars.Sort((a, b) =>
                    char.ToLowerInvariant(a).CompareTo(char.ToLowerInvariant(b)));
                break;

            case AnagramMode.ReverseAlphabetical:
                chars.Sort((a, b) =>
                    char.ToLowerInvariant(b).CompareTo(char.ToLowerInvariant(a)));
                break;

            case AnagramMode.Seeded:
                // Use a deterministic shuffle based on word content and seed
                var wordHash = _seed;
                for (var i = 0; i < chars.Length; i++)
                {
                    wordHash = wordHash * 31 + chars[i];
                }
                var random = new Random(wordHash);

                for (var i = chars.Length - 1; i > 0; i--)
                {
                    var j = random.Next(i + 1);
                    (chars[i], chars[j]) = (chars[j], chars[i]);
                }
                break;
        }
    }
}

public enum AnagramMode
{
    Alphabetical,
    ReverseAlphabetical,
    Seeded
}

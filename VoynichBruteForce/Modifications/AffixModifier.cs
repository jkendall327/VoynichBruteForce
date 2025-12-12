using System.Text;

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

    public string ModifyText(string text)
    {
        var result = new StringBuilder(text.Length * 2);
        var wordStart = -1;

        for (var i = 0; i <= text.Length; i++)
        {
            var isWordChar = i < text.Length && char.IsLetterOrDigit(text[i]);

            if (isWordChar && wordStart < 0)
            {
                wordStart = i;
            }
            else if (!isWordChar && wordStart >= 0)
            {
                var word = text[wordStart..i];
                var transformed = TransformWord(word);
                result.Append(transformed);
                wordStart = -1;

                if (i < text.Length)
                {
                    result.Append(text[i]);
                }
            }
            else if (!isWordChar && i < text.Length)
            {
                result.Append(text[i]);
            }
        }

        return result.ToString();
    }

    private string TransformWord(string word)
    {
        if (word.Length == 0)
        {
            return word;
        }

        return _mode switch
        {
            AffixMode.AddPrefix => _prefix + word,
            AffixMode.AddSuffix => word + _suffix,
            AffixMode.MoveFirstToEnd => word.Length > 1
                ? word[1..] + word[0]
                : word,
            AffixMode.MoveLastToStart => word.Length > 1
                ? word[^1] + word[..^1]
                : word,
            AffixMode.PigLatin => ApplyPigLatin(word),
            _ => word
        };
    }

    private static string ApplyPigLatin(string word)
    {
        if (word.Length == 0)
        {
            return word;
        }

        var vowels = new HashSet<char> { 'a', 'e', 'i', 'o', 'u', 'A', 'E', 'I', 'O', 'U' };

        // Find first vowel
        var firstVowelIndex = -1;
        for (var i = 0; i < word.Length; i++)
        {
            if (vowels.Contains(word[i]))
            {
                firstVowelIndex = i;
                break;
            }
        }

        if (firstVowelIndex <= 0)
        {
            // Word starts with vowel or has no vowels - add "ay"
            return word + "ay";
        }

        // Move consonant cluster to end and add "ay"
        var consonants = word[..firstVowelIndex];
        var rest = word[firstVowelIndex..];

        // Preserve case of first letter
        if (char.IsUpper(word[0]) && rest.Length > 0)
        {
            rest = char.ToUpper(rest[0]) + rest[1..];
            consonants = char.ToLower(consonants[0]) + consonants[1..];
        }

        return rest + consonants + "ay";
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

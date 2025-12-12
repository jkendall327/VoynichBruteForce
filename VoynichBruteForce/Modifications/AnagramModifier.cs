using System.Text;

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
public class AnagramModifier : ITextModifier
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

    public string ModifyText(string text)
    {
        var result = new StringBuilder(text.Length);
        var wordChars = new List<char>();
        var wordStart = -1;

        for (var i = 0; i <= text.Length; i++)
        {
            var isWordChar = i < text.Length && char.IsLetterOrDigit(text[i]);

            if (isWordChar)
            {
                if (wordStart < 0)
                {
                    wordStart = i;
                }
                wordChars.Add(text[i]);
            }
            else
            {
                if (wordChars.Count > 0)
                {
                    var anagrammed = AnagramWord(wordChars);
                    foreach (var c in anagrammed)
                    {
                        result.Append(c);
                    }
                    wordChars.Clear();
                    wordStart = -1;
                }

                if (i < text.Length)
                {
                    result.Append(text[i]);
                }
            }
        }

        return result.ToString();
    }

    private char[] AnagramWord(List<char> chars)
    {
        var arr = chars.ToArray();

        switch (_mode)
        {
            case AnagramMode.Alphabetical:
                Array.Sort(arr, (a, b) =>
                    char.ToLowerInvariant(a).CompareTo(char.ToLowerInvariant(b)));
                break;

            case AnagramMode.ReverseAlphabetical:
                Array.Sort(arr, (a, b) =>
                    char.ToLowerInvariant(b).CompareTo(char.ToLowerInvariant(a)));
                break;

            case AnagramMode.Seeded:
                // Use a deterministic shuffle based on word content and seed
                var wordHash = chars.Aggregate(_seed, (h, c) => h * 31 + c);
                var random = new Random(wordHash);

                for (var i = arr.Length - 1; i > 0; i--)
                {
                    var j = random.Next(i + 1);
                    (arr[i], arr[j]) = (arr[j], arr[i]);
                }
                break;
        }

        return arr;
    }
}

public enum AnagramMode
{
    Alphabetical,
    ReverseAlphabetical,
    Seeded
}

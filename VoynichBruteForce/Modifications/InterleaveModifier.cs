using System.Text;

namespace VoynichBruteForce.Modifications;

/// <summary>
/// Interleaves characters from the first and second halves of the text,
/// or performs a "riffle shuffle" of the text characters.
/// "ABCDEF" → "ADBECF" (interleaving halves).
///
/// Interleaving text is conceptually similar to shuffling cards - a technique
/// that was certainly possible to perform by hand. The idea of splitting and
/// weaving text requires no special tools, just careful attention.
/// </summary>
public class InterleaveModifier : ITextModifier
{
    private readonly InterleaveMode _mode;

    public string Name => _mode switch
    {
        InterleaveMode.HalvesAlternate => "Interleave(halves)",
        InterleaveMode.OddEvenSplit => "Interleave(odd-even)",
        InterleaveMode.ReverseInterleave => "Interleave(reverse)",
        _ => "Interleave"
    };

    // Moderate cognitive cost - requires tracking two streams
    public CognitiveComplexity CognitiveCost => new(5);

    public InterleaveModifier(InterleaveMode mode = InterleaveMode.HalvesAlternate)
    {
        _mode = mode;
    }

    public string ModifyText(string text)
    {
        if (text.Length <= 1)
        {
            return text;
        }

        return _mode switch
        {
            InterleaveMode.HalvesAlternate => InterleaveHalves(text),
            InterleaveMode.OddEvenSplit => SplitOddEven(text),
            InterleaveMode.ReverseInterleave => InterleaveWithReverse(text),
            _ => text
        };
    }

    /// <summary>
    /// Splits text into two halves and interleaves them.
    /// "ABCDEF" → "ADBECF"
    /// </summary>
    private static string InterleaveHalves(string text)
    {
        var mid = (text.Length + 1) / 2;
        var firstHalf = text[..mid];
        var secondHalf = text[mid..];

        var result = new StringBuilder(text.Length);
        var maxLen = Math.Max(firstHalf.Length, secondHalf.Length);

        for (var i = 0; i < maxLen; i++)
        {
            if (i < firstHalf.Length)
            {
                result.Append(firstHalf[i]);
            }
            if (i < secondHalf.Length)
            {
                result.Append(secondHalf[i]);
            }
        }

        return result.ToString();
    }

    /// <summary>
    /// Takes odd-indexed characters then even-indexed characters.
    /// "ABCDEF" → "ACEBDF"
    /// </summary>
    private static string SplitOddEven(string text)
    {
        var result = new StringBuilder(text.Length);

        // Even indices first (0, 2, 4...)
        for (var i = 0; i < text.Length; i += 2)
        {
            result.Append(text[i]);
        }

        // Then odd indices (1, 3, 5...)
        for (var i = 1; i < text.Length; i += 2)
        {
            result.Append(text[i]);
        }

        return result.ToString();
    }

    /// <summary>
    /// Interleaves text with its reverse.
    /// "ABC" → "ACBBCA" (original interleaved with reverse)
    /// </summary>
    private static string InterleaveWithReverse(string text)
    {
        var reversed = new string(text.Reverse().ToArray());
        var result = new StringBuilder(text.Length * 2);

        for (var i = 0; i < text.Length; i++)
        {
            result.Append(text[i]);
            result.Append(reversed[i]);
        }

        return result.ToString();
    }
}

public enum InterleaveMode
{
    HalvesAlternate,
    OddEvenSplit,
    ReverseInterleave
}

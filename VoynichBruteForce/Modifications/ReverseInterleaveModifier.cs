using System.Text;

namespace VoynichBruteForce.Modifications;

/// <summary>
/// Interleaves text with its reverse.
/// "ABC" → "ACBBCA" (original interleaved with reverse)
///
/// This modifier uses string-based processing because it doubles the text length,
/// which is incompatible with the fixed-capacity Span-based buffer architecture.
/// </summary>
public class ReverseInterleaveModifier : ITextModifier
{
    public string Name => "Interleave(reverse)";

    // Moderate cognitive cost - requires tracking two streams
    public CognitiveComplexity CognitiveCost => new(5);

    public string ModifyText(string text)
    {
        if (text.Length <= 1)
        {
            return text.Length == 1 ? text + text : text;
        }

        var reversed = new char[text.Length];
        for (var i = 0; i < text.Length; i++)
        {
            reversed[i] = text[text.Length - 1 - i];
        }

        var result = new StringBuilder(text.Length * 2);

        for (var i = 0; i < text.Length; i++)
        {
            result.Append(text[i]);
            result.Append(reversed[i]);
        }

        return result.ToString();
    }
}

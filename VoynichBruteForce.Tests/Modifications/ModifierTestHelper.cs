using VoynichBruteForce.Modifications;

namespace VoynichBruteForce.Tests.Modifications;

/// <summary>
/// Helper extension for testing ITextModifier implementations with the new Span-based API.
/// </summary>
public static class ModifierTestHelper
{
    /// <summary>
    /// Runs a modifier on the input text and returns the result as a string.
    /// This helper creates a ProcessingContext, runs the modifier, and extracts the result.
    /// </summary>
    public static string ModifyText(this ITextModifier modifier, string input)
    {
        // Allocate 4x capacity to handle growth scenarios
        var context = new ProcessingContext(input, Math.Max(input.Length * 4, 256));
        try
        {
            modifier.Modify(ref context);
            return context.InputSpan.ToString();
        }
        finally
        {
            context.Dispose();
        }
    }
}

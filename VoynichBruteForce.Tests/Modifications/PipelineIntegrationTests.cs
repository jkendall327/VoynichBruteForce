using VoynichBruteForce.Modifications;

namespace VoynichBruteForce.Tests.Modifications;

public class PipelineIntegrationTests
{
    [Fact]
    public void MultipleSpanModifiers_ChainCorrectly()
    {
        var input = "HELLO WORLD";
        var context = new ProcessingContext(input, input.Length * 8);
        try
        {
            // Chain letter doubling then Caesar cipher
            var letterDoubling = new LetterDoublingModifier();
            var caesarCipher = new CaesarCipherModifier(3);

            letterDoubling.Modify(ref context);
            caesarCipher.Modify(ref context);

            var result = context.InputSpan.ToString();

            // Letter doubling: "HHEELLLLOO  WWOORRLLDD" (22 chars)
            // Caesar shift: each letter +3
            Assert.NotEqual(input, result);
            Assert.True(result.Length >= input.Length);
        }
        finally
        {
            context.Dispose();
        }
    }

    [Fact]
    public void GrowthModifiers_InSequence_DoNotOverflow()
    {
        var input = "ABCDEF";
        var context = new ProcessingContext(input, input.Length * 8);
        try
        {
            // ReverseInterleave doubles length, then LetterDoubling can double again
            var reverseInterleave = new ReverseInterleaveModifier();
            var letterDoubling = new LetterDoublingModifier();

            reverseInterleave.Modify(ref context);
            letterDoubling.Modify(ref context);

            var result = context.InputSpan.ToString();

            // ReverseInterleave: 6 -> 12 chars
            // LetterDoubling: 12 -> up to 24 chars
            Assert.True(result.Length >= 12);
            Assert.True(result.Length <= 24);
        }
        finally
        {
            context.Dispose();
        }
    }

    [Fact]
    public void ShrinkingThenGrowingModifiers_WorkCorrectly()
    {
        var input = "AEIOU HELLO WORLD";
        var context = new ProcessingContext(input, input.Length * 8);
        try
        {
            // Vowel removal shrinks, then letter doubling expands
            var vowelRemoval = new VowelRemovalModifier();
            var letterDoubling = new LetterDoublingModifier();

            vowelRemoval.Modify(ref context);
            letterDoubling.Modify(ref context);

            var result = context.InputSpan.ToString();

            // Should contain no vowels, all letters doubled
            Assert.DoesNotContain("A", result);
            Assert.DoesNotContain("E", result);
            Assert.DoesNotContain("I", result);
            Assert.DoesNotContain("O", result);
            Assert.DoesNotContain("U", result);
        }
        finally
        {
            context.Dispose();
        }
    }

    [Fact]
    public void HomophonicSubstitution_InPipeline_DoesNotCauseIndexOutOfRange()
    {
        var substitutes = new Dictionary<char, string[]>
        {
            ['E'] = ["XX", "YY"],  // 2-char substitutes
            ['A'] = ["ZZZ"]        // 3-char substitute
        };
        var modifier = new HomophonicSubstitutionModifier(substitutes);

        // This should not throw
        var result = modifier.ModifyText("EEEEAAAAEEEEA");

        Assert.NotNull(result);
        Assert.True(result.Length > 13);  // Should expand
    }

    [Fact]
    public void LongPipelineWithMultipleGrowthModifiers_CompletesSuccessfully()
    {
        var input = new string('A', 100);
        var context = new ProcessingContext(input, input.Length * 16);
        try
        {
            // Chain multiple growth-inducing modifiers
            var nullInsertion = new NullInsertionModifier('X', 2);  // Insert X every 2 chars
            var letterDoubling = new LetterDoublingModifier();

            nullInsertion.Modify(ref context);
            letterDoubling.Modify(ref context);

            var result = context.InputSpan.ToString();

            // NullInsertion at interval 2: ~150 chars (100 + 50 Xs)
            // LetterDoubling: doubles all letters
            Assert.True(result.Length > 100);
        }
        finally
        {
            context.Dispose();
        }
    }

    [Fact]
    public void ReverseInterleave_ThenHomophonic_WorksTogether()
    {
        var input = "ABC";

        // First apply reverse interleave via span
        var context = new ProcessingContext(input, input.Length * 16);
        string afterInterleave;
        try
        {
            var reverseInterleave = new ReverseInterleaveModifier();
            reverseInterleave.Modify(ref context);
            afterInterleave = context.InputSpan.ToString();
        }
        finally
        {
            context.Dispose();
        }

        // Then apply homophonic with 2-char substitutes
        var substitutes = new Dictionary<char, string[]>
        {
            ['A'] = ["XX"],
            ['B'] = ["YY"],
            ['C'] = ["ZZ"]
        };
        var homophonic = new HomophonicSubstitutionModifier(substitutes);
        var result = homophonic.ModifyText(afterInterleave);

        // ABC -> ACBBCA (6 chars) -> each letter becomes 2 chars = 12 chars
        Assert.Equal(12, result.Length);
    }
}

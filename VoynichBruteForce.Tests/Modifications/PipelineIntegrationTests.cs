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

    [Fact]
    public void AffixModifier_AfterGrowthModifier_DoesNotOverflowBuffer()
    {
        // This test reproduces the "Destination is too short" error
        // when AffixModifier runs after a growth modifier like LetterDoubling
        var input = "hello world test";
        var context = new ProcessingContext(input, input.Length * 4);
        try
        {
            // First double the letters (growth modifier)
            var letterDoubling = new LetterDoublingModifier();
            letterDoubling.Modify(ref context);

            // Then add a prefix to each word (more growth)
            var affixModifier = new AffixModifier(AffixMode.AddPrefix, "pre");
            affixModifier.Modify(ref context);

            var result = context.InputSpan.ToString();

            // Should contain "pre" before each word
            Assert.Contains("pre", result);
        }
        finally
        {
            context.Dispose();
        }
    }

    [Fact]
    public void AffixModifier_PigLatin_AfterGrowthModifier_DoesNotOverflow()
    {
        var input = "hello world";
        var context = new ProcessingContext(input, input.Length * 4);
        try
        {
            var letterDoubling = new LetterDoublingModifier();
            letterDoubling.Modify(ref context);

            var pigLatin = new AffixModifier(AffixMode.PigLatin);
            pigLatin.Modify(ref context);

            var result = context.InputSpan.ToString();

            // PigLatin adds "ay" to each word
            Assert.Contains("ay", result);
        }
        finally
        {
            context.Dispose();
        }
    }

    [Fact]
    public void AffixModifier_WithLongPrefix_DoesNotOverflow()
    {
        // Many short words with a long prefix - worst case for buffer growth
        var input = "a b c d e f g h i j k l m n o p";
        var affixModifier = new AffixModifier(AffixMode.AddPrefix, "prefix");

        var result = affixModifier.ModifyText(input);

        // Each single letter word gets "prefix" added
        Assert.Contains("prefixa", result);
        Assert.Contains("prefixb", result);
    }

    [Fact]
    public void AffixModifier_AfterMultipleGrowthModifiers_DoesNotOverflow()
    {
        // This reproduces the actual failure: ReverseInterleave (2x) + LetterDoubling (2x) + Affix
        // Input: 100 chars -> ReverseInterleave: 200 chars -> LetterDoubling: 400 chars -> Affix with prefix
        // With 4x initial allocation (400), we might overflow when adding prefixes to many words
        var input = "a b c d e f g h i j a b c d e f g h i j a b c d e f g h i j a b c d e f g h i j a b c d e";
        var context = new ProcessingContext(input, input.Length * 4);  // Standard 4x allocation
        try
        {
            // Chain growth modifiers to exceed 4x
            var reverseInterleave = new ReverseInterleaveModifier();  // 2x
            var letterDoubling = new LetterDoublingModifier();         // 2x more
            var affixModifier = new AffixModifier(AffixMode.AddPrefix, "test");

            reverseInterleave.Modify(ref context);
            letterDoubling.Modify(ref context);
            affixModifier.Modify(ref context);  // This should work with EnsureCapacity fix

            var result = context.InputSpan.ToString();
            Assert.Contains("test", result);
        }
        finally
        {
            context.Dispose();
        }
    }

    [Fact]
    public void AffixModifier_WithTightBuffer_MustCallEnsureCapacity()
    {
        // Simulate a scenario where prior modifiers haven't left enough room
        // Many short words with long prefix on a minimal buffer
        var input = "a b c d e f g h i j k l m n o p q r s t";
        // Allocate only slightly more than input - this will fail without EnsureCapacity
        var context = new ProcessingContext(input, input.Length + 10);
        try
        {
            var affixModifier = new AffixModifier(AffixMode.AddPrefix, "verylongprefix");
            affixModifier.Modify(ref context);

            var result = context.InputSpan.ToString();
            Assert.Contains("verylongprefixa", result);
        }
        finally
        {
            context.Dispose();
        }
    }

    [Fact]
    public void NullInsertionModifier_WithTightBuffer_MustCallEnsureCapacity()
    {
        // NullInsertion with interval=1 doubles the text length
        var input = "abcdefghij";
        // Allocate minimal buffer - should fail without EnsureCapacity
        var context = new ProcessingContext(input, input.Length + 2);
        try
        {
            var nullInsertion = new NullInsertionModifier('X', 1);  // Insert X after every char = 2x growth
            nullInsertion.Modify(ref context);

            var result = context.InputSpan.ToString();
            Assert.Equal(20, result.Length);  // 10 original + 10 nulls
        }
        finally
        {
            context.Dispose();
        }
    }

    [Fact]
    public void ColumnarTranspositionModifier_WithTightBuffer_MustCallEnsureCapacity()
    {
        // Columnar transposition can pad with spaces, potentially exceeding input length
        var input = "HELLO";  // 5 chars
        // With 3 columns: numRows=2, gridSize=6, could write 6 chars before trimming
        var context = new ProcessingContext(input, input.Length);  // Exact size - no room for padding
        try
        {
            var columnar = new ColumnarTranspositionModifier([2, 0, 1]);
            columnar.Modify(ref context);

            var result = context.InputSpan.ToString();
            Assert.True(result.Length <= input.Length + 2);  // Allow small padding
        }
        finally
        {
            context.Dispose();
        }
    }
}

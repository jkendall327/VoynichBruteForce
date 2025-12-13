using VoynichBruteForce.Modifications;

namespace VoynichBruteForce.Tests.Modifications;

public class ConsonantVowelSplitModifierTests
{
    [Fact]
    public void ModifyText_ConsonantsFirst_SeparatesCorrectly()
    {
        var modifier = new ConsonantVowelSplitModifier(consonantsFirst: true);

        // "HELLO" -> consonants: H, L, L -> vowels: E, O
        // Result: HLLEO
        var result = modifier.ModifyText("HELLO");

        Assert.Equal("HLLEO", result);
    }

    [Fact]
    public void ModifyText_VowelsFirst_SeparatesCorrectly()
    {
        var modifier = new ConsonantVowelSplitModifier(consonantsFirst: false);

        // "HELLO" -> vowels: E, O -> consonants: H, L, L
        // Result: EOHLL
        var result = modifier.ModifyText("HELLO");

        Assert.Equal("EOHLL", result);
    }

    [Fact]
    public void ModifyText_PreservesNonLetters()
    {
        var modifier = new ConsonantVowelSplitModifier(consonantsFirst: true);

        // With non-letters, they should be reinserted at their relative positions
        var result = modifier.ModifyText("A B");

        // The space should be preserved
        Assert.Contains(" ", result);
    }

    [Fact]
    public void ModifyText_AllVowels_ReturnsUnchanged()
    {
        var modifier = new ConsonantVowelSplitModifier(consonantsFirst: true);

        var result = modifier.ModifyText("AEIOU");

        Assert.Equal("AEIOU", result);
    }

    [Fact]
    public void ModifyText_AllConsonants_ReturnsUnchanged()
    {
        var modifier = new ConsonantVowelSplitModifier(consonantsFirst: true);

        var result = modifier.ModifyText("BCDFG");

        Assert.Equal("BCDFG", result);
    }

    [Fact]
    public void ModifyText_WithEmptyString_ReturnsEmpty()
    {
        var modifier = new ConsonantVowelSplitModifier(consonantsFirst: true);

        var result = modifier.ModifyText("");

        Assert.Equal("", result);
    }

    [Fact]
    public void ModifyText_IsDeterministic_OnRepeatedCalls()
    {
        var modifier = new ConsonantVowelSplitModifier(consonantsFirst: true);

        var result1 = modifier.ModifyText("HELLO WORLD");
        var result2 = modifier.ModifyText("HELLO WORLD");

        Assert.Equal(result1, result2);
    }

    [Fact]
    public void ModifyText_PreservesCase()
    {
        var modifier = new ConsonantVowelSplitModifier(consonantsFirst: true);

        var result = modifier.ModifyText("Hello");

        // Should preserve the original case of letters
        Assert.Contains("H", result);
        Assert.Contains("e", result);
    }
}

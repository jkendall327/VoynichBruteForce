using VoynichBruteForce.Modifications;

namespace VoynichBruteForce.Tests.Modifications;

public class HomophonicSubstitutionModifierTests
{
    [Fact]
    public void ModifyText_WithExplicitSubstitutes_CyclesThroughAlternatives()
    {
        var substitutes = new Dictionary<char, string[]>
        {
            ['E'] = ["1", "2", "3"]
        };
        var modifier = new HomophonicSubstitutionModifier(substitutes);

        var result = modifier.ModifyText("EEEEE");

        // Should cycle through 1, 2, 3, 1, 2
        Assert.Equal("12312", result);
    }

    [Fact]
    public void ModifyText_PreservesNonMappedCharacters()
    {
        var substitutes = new Dictionary<char, string[]>
        {
            ['A'] = ["X"]
        };
        var modifier = new HomophonicSubstitutionModifier(substitutes);

        var result = modifier.ModifyText("ABCABC");

        Assert.Equal("XBCXBC", result);
    }

    [Fact]
    public void ModifyText_WithSameSeed_ProducesSameResult()
    {
        var modifier1 = new HomophonicSubstitutionModifier(42);
        var modifier2 = new HomophonicSubstitutionModifier(42);

        var result1 = modifier1.ModifyText("HELLO WORLD");
        var result2 = modifier2.ModifyText("HELLO WORLD");

        Assert.Equal(result1, result2);
    }

    [Fact]
    public void ModifyText_IsDeterministic_OnRepeatedCalls()
    {
        var modifier = new HomophonicSubstitutionModifier(42);

        var result1 = modifier.ModifyText("TEST");
        var result2 = modifier.ModifyText("TEST");

        Assert.Equal(result1, result2);
    }

    [Fact]
    public void ModifyText_WithEmptyString_ReturnsEmpty()
    {
        var modifier = new HomophonicSubstitutionModifier(42);

        var result = modifier.ModifyText("");

        Assert.Equal("", result);
    }

    [Fact]
    public void ModifyText_HandlesLowercase()
    {
        var substitutes = new Dictionary<char, string[]>
        {
            ['E'] = ["X"]
        };
        var modifier = new HomophonicSubstitutionModifier(substitutes);

        var result = modifier.ModifyText("hello");

        // 'e' should map using the uppercase mapping, preserving case for single-char
        Assert.Equal("hxllo", result);
    }

    [Fact]
    public void ModifyText_WithMultiCharSubstitutes_ExpandsCorrectly()
    {
        var substitutes = new Dictionary<char, string[]>
        {
            ['A'] = ["XY", "ZW"]
        };
        var modifier = new HomophonicSubstitutionModifier(substitutes);

        var result = modifier.ModifyText("AAA");

        // Should cycle through XY, ZW, XY
        Assert.Equal("XYZWXY", result);
    }

    [Fact]
    public void ModifyText_WithMixedLengthSubstitutes_HandlesCorrectly()
    {
        var substitutes = new Dictionary<char, string[]>
        {
            ['E'] = ["X", "YZ", "W"]  // Mix of 1-char and 2-char substitutes
        };
        var modifier = new HomophonicSubstitutionModifier(substitutes);

        var result = modifier.ModifyText("EEEE");

        // Should cycle through X, YZ, W, X
        Assert.Equal("XYZWX", result);
    }

    [Fact]
    public void ModifyText_LongText_DoesNotThrowIndexOutOfRange()
    {
        var modifier = new HomophonicSubstitutionModifier(42, maxSubstitutes: 4);
        var longText = new string('E', 10000);

        var result = modifier.ModifyText(longText);

        // Should complete without throwing
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
    }

    [Fact]
    public void ModifyText_WithMultiCharSubstitutesOnLongText_HandlesCorrectly()
    {
        var substitutes = new Dictionary<char, string[]>
        {
            ['A'] = ["XXX"]  // 3x expansion
        };
        var modifier = new HomophonicSubstitutionModifier(substitutes);
        var input = new string('A', 1000);

        var result = modifier.ModifyText(input);

        Assert.Equal(3000, result.Length);
        Assert.All(result, c => Assert.Equal('X', c));
    }
}

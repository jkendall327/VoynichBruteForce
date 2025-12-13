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
}

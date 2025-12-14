using VoynichBruteForce.Modifications;

namespace VoynichBruteForce.Tests.Modifications;

public class PolyalphabeticModifierTests
{
    [Fact]
    public void ModifyText_WithSimpleKeyword_ShiftsCorrectly()
    {
        var modifier = new PolyalphabeticModifier("ABC");

        var result = modifier.ModifyText("AAA");

        // A shifted by A(0): A
        // A shifted by B(1): B
        // A shifted by C(2): C
        Assert.Equal("ABC", result);
    }

    [Fact]
    public void ModifyText_KeywordRepeats_WhenTextLongerThanKey()
    {
        var modifier = new PolyalphabeticModifier("AB");

        var result = modifier.ModifyText("AAAA");

        // A+0=A, A+1=B, A+0=A, A+1=B
        Assert.Equal("ABAB", result);
    }

    [Fact]
    public void ModifyText_PreservesCase()
    {
        var modifier = new PolyalphabeticModifier("A");

        var result = modifier.ModifyText("HeLLo");

        // All shifted by 0, so preserved
        Assert.Equal("HeLLo", result);
    }

    [Fact]
    public void ModifyText_PreservesNonLetters()
    {
        var modifier = new PolyalphabeticModifier("A");

        var result = modifier.ModifyText("Hello, World! 123");

        Assert.Equal("Hello, World! 123", result);
    }

    [Fact]
    public void ModifyText_NonLettersDontAdvanceKeyPosition()
    {
        var modifier = new PolyalphabeticModifier("AB");

        var result = modifier.ModifyText("A A A");

        // A+0=A, space (not shifted, key stays at position 1), A+1=B, space, A+0=A
        Assert.Equal("A B A", result);
    }

    [Fact]
    public void ModifyText_WithKeyZ_ShiftsBy25()
    {
        var modifier = new PolyalphabeticModifier("Z");

        var result = modifier.ModifyText("AB");

        // A+25=Z, B+25=A (wraps around)
        Assert.Equal("ZA", result);
    }

    [Fact]
    public void Constructor_WithEmptyKeyword_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new PolyalphabeticModifier(""));
    }

    [Fact]
    public void ModifyText_WithEmptyString_ReturnsEmpty()
    {
        var modifier = new PolyalphabeticModifier("KEY");

        var result = modifier.ModifyText("");

        Assert.Equal("", result);
    }

    [Fact]
    public void ModifyText_LowercaseKeyword_TreatedAsUppercase()
    {
        var modifier1 = new PolyalphabeticModifier("abc");
        var modifier2 = new PolyalphabeticModifier("ABC");

        var result1 = modifier1.ModifyText("XYZ");
        var result2 = modifier2.ModifyText("XYZ");

        Assert.Equal(result1, result2);
    }
}

using VoynichBruteForce.Modifications;

namespace VoynichBruteForce.Tests.Modifications;

public class PositionalExtractionModifierTests
{
    [Fact]
    public void Acrostic_ExtractsFirstLetterOfEachWord()
    {
        var modifier = PositionalExtractionModifier.Acrostic();

        var result = modifier.ModifyText("Hello Everyone Lives Longer Over");

        Assert.Equal("HELLO", result);
    }

    [Fact]
    public void Telestich_ExtractsLastLetterOfEachWord()
    {
        var modifier = PositionalExtractionModifier.Telestich();

        var result = modifier.ModifyText("path age bell cool memo");

        Assert.Equal("hello", result);
    }

    [Fact]
    public void ModifyText_ExtractsSecondLetter()
    {
        var modifier = new PositionalExtractionModifier(position: 1);

        var result = modifier.ModifyText("HELLO WORLD");

        Assert.Equal("EO", result);
    }

    [Fact]
    public void ModifyText_ExtractsSecondFromEnd()
    {
        var modifier = new PositionalExtractionModifier(position: 1, fromEnd: true);

        var result = modifier.ModifyText("HELLO WORLD");

        Assert.Equal("LL", result);
    }

    [Fact]
    public void ModifyText_SkipsShortWords()
    {
        var modifier = new PositionalExtractionModifier(position: 2);

        var result = modifier.ModifyText("I am too short");

        Assert.Equal("oo", result);
    }

    [Fact]
    public void ModifyText_HandlesMultipleSpaces()
    {
        var modifier = PositionalExtractionModifier.Acrostic();

        var result = modifier.ModifyText("HELLO   WORLD");

        Assert.Equal("HW", result);
    }

    [Fact]
    public void ModifyText_HandlesPunctuation()
    {
        var modifier = PositionalExtractionModifier.Acrostic();

        var result = modifier.ModifyText("Hello, beautiful world!");

        Assert.Equal("Hbw", result);
    }

    [Fact]
    public void ModifyText_WithEmptyString_ReturnsEmpty()
    {
        var modifier = PositionalExtractionModifier.Acrostic();

        var result = modifier.ModifyText("");

        Assert.Equal("", result);
    }

    [Fact]
    public void ModifyText_WithSingleWord_ExtractsOneCharacter()
    {
        var modifier = PositionalExtractionModifier.Acrostic();

        var result = modifier.ModifyText("HELLO");

        Assert.Equal("H", result);
    }

    [Fact]
    public void ModifyText_WithNumbers_TreatsThemAsWordCharacters()
    {
        var modifier = PositionalExtractionModifier.Acrostic();

        var result = modifier.ModifyText("ABC 123 DEF");

        Assert.Equal("A1D", result);
    }
}

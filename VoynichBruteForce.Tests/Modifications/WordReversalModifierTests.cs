using VoynichBruteForce.Modifications;

namespace VoynichBruteForce.Tests.Modifications;

public class WordReversalModifierTests
{
    [Fact]
    public void ModifyText_ReversesSingleWord()
    {
        var modifier = new WordReversalModifier();

        var result = modifier.ModifyText("hello");

        Assert.Equal("olleh", result);
    }

    [Fact]
    public void ModifyText_ReversesMultipleWords_PreservesSpaces()
    {
        var modifier = new WordReversalModifier();

        var result = modifier.ModifyText("hello world");

        Assert.Equal("olleh dlrow", result);
    }

    [Fact]
    public void ModifyText_PreservesNonWordCharacters()
    {
        var modifier = new WordReversalModifier();

        var result = modifier.ModifyText("hello, world!");

        Assert.Equal("olleh, dlrow!", result);
    }

    [Fact]
    public void ModifyText_HandlesMultipleSpaces()
    {
        var modifier = new WordReversalModifier();

        var result = modifier.ModifyText("hello  world");

        Assert.Equal("olleh  dlrow", result);
    }

    [Fact]
    public void ModifyText_WithEmptyString_ReturnsEmpty()
    {
        var modifier = new WordReversalModifier();

        var result = modifier.ModifyText("");

        Assert.Equal("", result);
    }

    [Fact]
    public void ModifyText_WithOnlySpaces_PreservesSpaces()
    {
        var modifier = new WordReversalModifier();

        var result = modifier.ModifyText("   ");

        Assert.Equal("   ", result);
    }

    [Fact]
    public void ModifyText_WithSingleLetter_ReturnsSameLetter()
    {
        var modifier = new WordReversalModifier();

        var result = modifier.ModifyText("a");

        Assert.Equal("a", result);
    }

    [Fact]
    public void ModifyText_HandlesDigits()
    {
        var modifier = new WordReversalModifier();

        var result = modifier.ModifyText("abc123 def456");

        Assert.Equal("321cba 654fed", result);
    }

    [Fact]
    public void ModifyText_PreservesPunctuation()
    {
        var modifier = new WordReversalModifier();

        var result = modifier.ModifyText("test! another? word.");

        Assert.Equal("tset! rehtona? drow.", result);
    }
}

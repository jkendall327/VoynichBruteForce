using VoynichBruteForce.Modifications;

namespace VoynichBruteForce.Tests.Modifications;

public class LetterDoublingModifierTests
{
    [Fact]
    public void ModifyText_WithAllLetters_DoublesAllLetters()
    {
        var modifier = new LetterDoublingModifier();

        var result = modifier.ModifyText("abc");

        Assert.Equal("aabbcc", result);
    }

    [Fact]
    public void ModifyText_WithSpecificLetters_DoublesOnlySpecified()
    {
        var modifier = new LetterDoublingModifier(['a', 'e']);

        var result = modifier.ModifyText("hello");

        // Only 'e' should be doubled (not 'o' since it's not in the set)
        Assert.Equal("heello", result);
    }

    [Fact]
    public void ModifyText_PreservesNonLetters()
    {
        var modifier = new LetterDoublingModifier();

        var result = modifier.ModifyText("a1b!");

        Assert.Equal("aa1bb!", result);
    }

    [Fact]
    public void ModifyText_WithEmptyString_ReturnsEmpty()
    {
        var modifier = new LetterDoublingModifier();

        var result = modifier.ModifyText("");

        Assert.Equal("", result);
    }

    [Fact]
    public void ModifyText_CaseInsensitiveMatching_DoublesMatchingCase()
    {
        var modifier = new LetterDoublingModifier(['a']);

        var result = modifier.ModifyText("AbA");

        Assert.Equal("AAbAA", result);
    }

    [Fact]
    public void ModifyText_PreservesCase()
    {
        var modifier = new LetterDoublingModifier();

        var result = modifier.ModifyText("HeLLo");

        Assert.Equal("HHeeLLLLoo", result);
    }

    [Fact]
    public void ModifyText_LongText_HandlesCorrectly()
    {
        var modifier = new LetterDoublingModifier();
        var input = new string('a', 1000);

        var result = modifier.ModifyText(input);

        Assert.Equal(2000, result.Length);
        Assert.All(result, c => Assert.Equal('a', c));
    }
}

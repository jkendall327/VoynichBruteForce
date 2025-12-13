using VoynichBruteForce.Modifications;

namespace VoynichBruteForce.Tests.Modifications;

public class AnagramModifierTests
{
    [Fact]
    public void ModifyText_Alphabetical_SortsWordsAlphabetically()
    {
        var modifier = new AnagramModifier(AnagramMode.Alphabetical);

        var result = modifier.ModifyText("dcba");

        Assert.Equal("abcd", result);
    }

    [Fact]
    public void ModifyText_Alphabetical_PreservesWordBoundaries()
    {
        var modifier = new AnagramModifier(AnagramMode.Alphabetical);

        var result = modifier.ModifyText("cba fed");

        Assert.Equal("abc def", result);
    }

    [Fact]
    public void ModifyText_ReverseAlphabetical_SortsWordsReverseAlphabetically()
    {
        var modifier = new AnagramModifier(AnagramMode.ReverseAlphabetical);

        var result = modifier.ModifyText("abcd");

        Assert.Equal("dcba", result);
    }

    [Fact]
    public void ModifyText_Seeded_ProducesDeterministicResult()
    {
        var modifier1 = new AnagramModifier(AnagramMode.Seeded, seed: 42);
        var modifier2 = new AnagramModifier(AnagramMode.Seeded, seed: 42);

        var result1 = modifier1.ModifyText("hello world");
        var result2 = modifier2.ModifyText("hello world");

        Assert.Equal(result1, result2);
    }

    [Fact]
    public void ModifyText_PreservesPunctuation()
    {
        var modifier = new AnagramModifier(AnagramMode.Alphabetical);

        var result = modifier.ModifyText("cba, fed!");

        Assert.Equal("abc, def!", result);
    }

    [Fact]
    public void ModifyText_WithEmptyString_ReturnsEmpty()
    {
        var modifier = new AnagramModifier(AnagramMode.Alphabetical);

        var result = modifier.ModifyText("");

        Assert.Equal("", result);
    }

    [Fact]
    public void ModifyText_SingleCharacterWords_RemainUnchanged()
    {
        var modifier = new AnagramModifier(AnagramMode.Alphabetical);

        var result = modifier.ModifyText("a b c");

        Assert.Equal("a b c", result);
    }

    [Fact]
    public void ModifyText_IsDeterministic_OnRepeatedCalls()
    {
        var modifier = new AnagramModifier(AnagramMode.Seeded, seed: 123);

        var result1 = modifier.ModifyText("testing");
        var result2 = modifier.ModifyText("testing");

        Assert.Equal(result1, result2);
    }
}

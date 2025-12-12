using VoynichBruteForce.Modifications;

namespace VoynichBruteForce.Tests.Modifications;

public class VowelRemovalModifierTests
{
    [Fact]
    public void ModifyText_RemovesAllVowels()
    {
        var modifier = new VowelRemovalModifier();

        var result = modifier.ModifyText("HELLO WORLD");

        Assert.Equal("HLL WRLD", result);
    }

    [Fact]
    public void ModifyText_RemovesUppercaseVowels()
    {
        var modifier = new VowelRemovalModifier();

        var result = modifier.ModifyText("AEIOU");

        Assert.Equal("", result);
    }

    [Fact]
    public void ModifyText_RemovesLowercaseVowels()
    {
        var modifier = new VowelRemovalModifier();

        var result = modifier.ModifyText("aeiou");

        Assert.Equal("", result);
    }

    [Fact]
    public void ModifyText_RemovesMixedCaseVowels()
    {
        var modifier = new VowelRemovalModifier();

        var result = modifier.ModifyText("Hello World");

        Assert.Equal("Hll Wrld", result);
    }

    [Fact]
    public void ModifyText_PreservesConsonants()
    {
        var modifier = new VowelRemovalModifier();

        var result = modifier.ModifyText("bcdfghjklmnpqrstvwxyz");

        Assert.Equal("bcdfghjklmnpqrstvwxyz", result);
    }

    [Fact]
    public void ModifyText_PreservesNonLetters()
    {
        var modifier = new VowelRemovalModifier();

        var result = modifier.ModifyText("Hello, World! 123");

        Assert.Equal("Hll, Wrld! 123", result);
    }

    [Fact]
    public void ModifyText_WithEmptyString_ReturnsEmpty()
    {
        var modifier = new VowelRemovalModifier();

        var result = modifier.ModifyText("");

        Assert.Equal("", result);
    }

    [Fact]
    public void ModifyText_WithOnlyVowels_ReturnsEmpty()
    {
        var modifier = new VowelRemovalModifier();

        var result = modifier.ModifyText("AEIOUaeiou");

        Assert.Equal("", result);
    }
}

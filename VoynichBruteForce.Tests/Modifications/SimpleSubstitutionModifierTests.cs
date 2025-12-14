using VoynichBruteForce.Modifications;

namespace VoynichBruteForce.Tests.Modifications;

public class SimpleSubstitutionModifierTests
{
    [Fact]
    public void ModifyText_WithBasicKey_SubstitutesCorrectly()
    {
        var modifier = new SimpleSubstitutionModifier("ZYXWVUTSRQPONMLKJIHGFEDCBA");

        var result = modifier.ModifyText("ABC");

        Assert.Equal("ZYX", result);
    }

    [Fact]
    public void ModifyText_PreservesCase()
    {
        var modifier = new SimpleSubstitutionModifier("ZYXWVUTSRQPONMLKJIHGFEDCBA");

        var result = modifier.ModifyText("AbC");

        Assert.Equal("ZyX", result);
    }

    [Fact]
    public void ModifyText_PreservesNonLetters()
    {
        var modifier = new SimpleSubstitutionModifier("ZYXWVUTSRQPONMLKJIHGFEDCBA");

        var result = modifier.ModifyText("A B, C! 123");

        Assert.Equal("Z Y, X! 123", result);
    }

    [Fact]
    public void Constructor_WithInvalidKeyLength_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new SimpleSubstitutionModifier("ABC"));
        Assert.Throws<ArgumentException>(() => new SimpleSubstitutionModifier("ABCDEFGHIJKLMNOPQRSTUVWXYZ123"));
    }

    [Fact]
    public void CreateRandom_WithSeed_GeneratesConsistentMapping()
    {
        var modifier1 = SimpleSubstitutionModifier.CreateRandom(42);
        var modifier2 = SimpleSubstitutionModifier.CreateRandom(42);

        var result1 = modifier1.ModifyText("HELLO");
        var result2 = modifier2.ModifyText("HELLO");

        Assert.Equal(result1, result2);
    }

    [Fact]
    public void CreateRandom_WithDifferentSeeds_GeneratesDifferentMappings()
    {
        var modifier1 = SimpleSubstitutionModifier.CreateRandom(1);
        var modifier2 = SimpleSubstitutionModifier.CreateRandom(2);

        var result1 = modifier1.ModifyText("HELLO");
        var result2 = modifier2.ModifyText("HELLO");

        Assert.NotEqual(result1, result2);
    }

    [Fact]
    public void ModifyText_WithEmptyString_ReturnsEmpty()
    {
        var modifier = new SimpleSubstitutionModifier("ZYXWVUTSRQPONMLKJIHGFEDCBA");

        var result = modifier.ModifyText("");

        Assert.Equal("", result);
    }

    [Fact]
    public void ModifyText_WithFullAlphabet_MapsCorrectly()
    {
        var modifier = new SimpleSubstitutionModifier("BCDEFGHIJKLMNOPQRSTUVWXYZA");

        var result = modifier.ModifyText("ABCDEFGHIJKLMNOPQRSTUVWXYZ");

        Assert.Equal("BCDEFGHIJKLMNOPQRSTUVWXYZA", result);
    }
}
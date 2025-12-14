using VoynichBruteForce.Modifications;

namespace VoynichBruteForce.Tests.Modifications;

public class NullInsertionModifierTests
{
    [Fact]
    public void ModifyText_InsertsNullAtInterval()
    {
        var modifier = new NullInsertionModifier('X', 2);

        var result = modifier.ModifyText("ABCD");

        // After every 2 characters: AB, X, CD, X
        Assert.Equal("ABXCDX", result);
    }

    [Fact]
    public void ModifyText_WithInterval1_InsertsAfterEveryChar()
    {
        var modifier = new NullInsertionModifier('X', 1);

        var result = modifier.ModifyText("ABC");

        // After every character: A, X, B, X, C, X
        Assert.Equal("AXBXCX", result);
    }

    [Fact]
    public void ModifyText_WithInterval3()
    {
        var modifier = new NullInsertionModifier('*', 3);

        var result = modifier.ModifyText("ABCDEFGH");

        // After every 3 characters: ABC, *, DEF, *, GH
        Assert.Equal("ABC*DEF*GH", result);
    }

    [Fact]
    public void ModifyText_WithEmptyString_ReturnsEmpty()
    {
        var modifier = new NullInsertionModifier('X', 2);

        var result = modifier.ModifyText("");

        Assert.Equal("", result);
    }

    [Fact]
    public void ModifyText_TextShorterThanInterval_NoNullInserted()
    {
        var modifier = new NullInsertionModifier('X', 5);

        var result = modifier.ModifyText("ABC");

        Assert.Equal("ABC", result);
    }

    [Fact]
    public void ModifyText_TextExactlyInterval_InsertsOneNull()
    {
        var modifier = new NullInsertionModifier('X', 3);

        var result = modifier.ModifyText("ABC");

        Assert.Equal("ABCX", result);
    }

    [Fact]
    public void Constructor_WithIntervalZero_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new NullInsertionModifier('X', 0));
    }

    [Fact]
    public void Constructor_WithNegativeInterval_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new NullInsertionModifier('X', -1));
    }

    [Fact]
    public void Perturb_AdjustsInterval()
    {
        var modifier = new NullInsertionModifier('X', 3);
        var random = new Random(42);

        var perturbed = modifier.Perturb(random);

        // Ensure it's a different instance
        Assert.NotSame(modifier, perturbed);
        Assert.IsType<NullInsertionModifier>(perturbed);
    }
}

using VoynichBruteForce.Modifications;

namespace VoynichBruteForce.Tests.Modifications;

public class InterleaveModifierTests
{
    [Fact]
    public void ModifyText_HalvesMode_InterleavesTwoHalves()
    {
        var modifier = new InterleaveModifier(InterleaveMode.HalvesAlternate);

        var result = modifier.ModifyText("ABCDEF");

        // First half: ABC, Second half: DEF
        // Interleaved: A, D, B, E, C, F
        Assert.Equal("ADBECF", result);
    }

    [Fact]
    public void ModifyText_HalvesMode_WithOddLength_HandlesCorrectly()
    {
        var modifier = new InterleaveModifier(InterleaveMode.HalvesAlternate);

        var result = modifier.ModifyText("ABCDE");

        // First half: ABC (mid = 3), Second half: DE
        // Interleaved: A, D, B, E, C
        Assert.Equal("ADBEC", result);
    }

    [Fact]
    public void ModifyText_OddEvenMode_SplitsOddAndEven()
    {
        var modifier = new InterleaveModifier(InterleaveMode.OddEvenSplit);

        var result = modifier.ModifyText("ABCDEF");

        // Even indices (0,2,4): A, C, E
        // Odd indices (1,3,5): B, D, F
        // Result: ACE then BDF
        Assert.Equal("ACEBDF", result);
    }

    [Fact]
    public void ModifyText_OddEvenMode_WithOddLength()
    {
        var modifier = new InterleaveModifier(InterleaveMode.OddEvenSplit);

        var result = modifier.ModifyText("ABCDE");

        // Even indices (0,2,4): A, C, E
        // Odd indices (1,3): B, D
        Assert.Equal("ACEBD", result);
    }

    [Fact]
    public void ModifyText_WithEmptyString_ReturnsEmpty()
    {
        var modifier = new InterleaveModifier(InterleaveMode.HalvesAlternate);

        var result = modifier.ModifyText("");

        Assert.Equal("", result);
    }

    [Fact]
    public void ModifyText_WithSingleCharacter_ReturnsSameCharacter()
    {
        var modifier = new InterleaveModifier(InterleaveMode.HalvesAlternate);

        var result = modifier.ModifyText("A");

        Assert.Equal("A", result);
    }

    [Fact]
    public void Constructor_WithReverseInterleaveMode_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new InterleaveModifier(InterleaveMode.ReverseInterleave));
    }

    [Fact]
    public void ModifyText_HalvesMode_WithTwoCharacters()
    {
        var modifier = new InterleaveModifier(InterleaveMode.HalvesAlternate);

        var result = modifier.ModifyText("AB");

        // First half: A, Second half: B
        Assert.Equal("AB", result);
    }
}

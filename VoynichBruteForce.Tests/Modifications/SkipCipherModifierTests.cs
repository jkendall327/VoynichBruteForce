using VoynichBruteForce.Modifications;

namespace VoynichBruteForce.Tests.Modifications;

public class SkipCipherModifierTests
{
    [Fact]
    public void ModifyText_WithSkip2_ExtractsEveryOtherCharacter()
    {
        var modifier = new SkipCipherModifier(2);

        // "HELLO" with skip 2:
        // Start at 0: H (pos 0), L (pos 2), O (pos 4), then wrap
        // Continue: E (pos 1), L (pos 3)
        // Result: H, L, O, E, L
        var result = modifier.ModifyText("HELLO");

        Assert.Equal("HLOEL", result);
    }

    [Fact]
    public void ModifyText_WithSkip3_ExtractsEveryThirdCharacter()
    {
        var modifier = new SkipCipherModifier(3);

        // "ABCDEF" with skip 3:
        // Start at 0: A (pos 0), D (pos 3), wrap to 1: B (pos 1), E (pos 4), wrap to 2: C (pos 2), F (pos 5)
        var result = modifier.ModifyText("ABCDEF");

        Assert.Equal("ADBECF", result);
    }

    [Fact]
    public void ModifyText_PreservesAllCharacters()
    {
        var modifier = new SkipCipherModifier(2);

        var input = "ABCDEFGH";
        var result = modifier.ModifyText(input);

        // All characters should be present
        Assert.Equal(input.Length, result.Length);
        Assert.True(input.All(c => result.Contains(c)));
    }

    [Fact]
    public void ModifyText_WithEmptyString_ReturnsEmpty()
    {
        var modifier = new SkipCipherModifier(2);

        var result = modifier.ModifyText("");

        Assert.Equal("", result);
    }

    [Fact]
    public void ModifyText_IsDeterministic_OnRepeatedCalls()
    {
        var modifier = new SkipCipherModifier(3);

        var result1 = modifier.ModifyText("HELLO WORLD");
        var result2 = modifier.ModifyText("HELLO WORLD");

        Assert.Equal(result1, result2);
    }

    [Fact]
    public void Constructor_WithSkipLessThan2_Throws()
    {
        Assert.Throws<ArgumentException>(() => new SkipCipherModifier(1));
        Assert.Throws<ArgumentException>(() => new SkipCipherModifier(0));
    }

    [Fact]
    public void ModifyText_SingleCharacter_ReturnsSameCharacter()
    {
        var modifier = new SkipCipherModifier(2);

        var result = modifier.ModifyText("A");

        Assert.Equal("A", result);
    }
}

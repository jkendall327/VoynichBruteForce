using VoynichBruteForce.Modifications;

namespace VoynichBruteForce.Tests.Modifications;

public class AtbashCipherModifierTests
{
    [Fact]
    public void ModifyText_EncryptsBasicText()
    {
        var modifier = new AtbashCipherModifier();

        var result = modifier.ModifyText("ABCXYZ");

        Assert.Equal("ZYXCBA", result);
    }

    [Fact]
    public void ModifyText_IsReversible()
    {
        var modifier = new AtbashCipherModifier();

        var encrypted = modifier.ModifyText("HELLO");
        var decrypted = modifier.ModifyText(encrypted);

        Assert.Equal("HELLO", decrypted);
    }

    [Fact]
    public void ModifyText_PreservesCase()
    {
        var modifier = new AtbashCipherModifier();

        var result = modifier.ModifyText("Hello World");

        Assert.Equal("Svool Dliow", result);
    }

    [Fact]
    public void ModifyText_PreservesNonLetters()
    {
        var modifier = new AtbashCipherModifier();

        var result = modifier.ModifyText("Hello, World! 123");

        Assert.Equal("Svool, Dliow! 123", result);
    }

    [Fact]
    public void ModifyText_MiddleLettersMapsCorrectly()
    {
        var modifier = new AtbashCipherModifier();

        // M (13th letter) should map to N (14th letter from end)
        var result = modifier.ModifyText("MN");

        Assert.Equal("NM", result);
    }

    [Fact]
    public void ModifyText_WithEmptyString_ReturnsEmpty()
    {
        var modifier = new AtbashCipherModifier();

        var result = modifier.ModifyText("");

        Assert.Equal("", result);
    }

    [Fact]
    public void ModifyText_FullAlphabet_ReversesCorrectly()
    {
        var modifier = new AtbashCipherModifier();

        var result = modifier.ModifyText("ABCDEFGHIJKLMNOPQRSTUVWXYZ");

        Assert.Equal("ZYXWVUTSRQPONMLKJIHGFEDCBA", result);
    }
}

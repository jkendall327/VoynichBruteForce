using VoynichBruteForce.Modifications;

namespace VoynichBruteForce.Tests.Modifications;

public class CaesarCipherModifierTests
{
    [Fact]
    public void ModifyText_WithShift3_EncryptsCorrectly()
    {
        var modifier = new CaesarCipherModifier(3);

        var result = modifier.ModifyText("HELLO");

        Assert.Equal("KHOOR", result);
    }

    [Fact]
    public void ModifyText_WithShift13_AppliesRot13()
    {
        var modifier = new CaesarCipherModifier(13);

        var result = modifier.ModifyText("HELLO");

        Assert.Equal("URYYB", result);
    }

    [Fact]
    public void ModifyText_PreservesCase()
    {
        var modifier = new CaesarCipherModifier(1);

        var result = modifier.ModifyText("Hello World");

        Assert.Equal("Ifmmp Xpsme", result);
    }

    [Fact]
    public void ModifyText_PreservesNonLetters()
    {
        var modifier = new CaesarCipherModifier(5);

        var result = modifier.ModifyText("Hello, World! 123");

        Assert.Equal("Mjqqt, Btwqi! 123", result);
    }

    [Fact]
    public void ModifyText_WithShift26_ReturnsOriginal()
    {
        var modifier = new CaesarCipherModifier(26);

        var result = modifier.ModifyText("ABCXYZ");

        Assert.Equal("ABCXYZ", result);
    }

    [Fact]
    public void ModifyText_WithNegativeShift_ShiftsBackward()
    {
        var modifier = new CaesarCipherModifier(-3);

        var result = modifier.ModifyText("KHOOR");

        Assert.Equal("HELLO", result);
    }

    [Fact]
    public void ModifyText_WithEmptyString_ReturnsEmpty()
    {
        var modifier = new CaesarCipherModifier(5);

        var result = modifier.ModifyText("");

        Assert.Equal("", result);
    }
}

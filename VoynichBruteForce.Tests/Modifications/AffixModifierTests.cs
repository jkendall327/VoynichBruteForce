using VoynichBruteForce.Modifications;

namespace VoynichBruteForce.Tests.Modifications;

public class AffixModifierTests
{
    [Fact]
    public void ModifyText_AddPrefix_AddsToWords()
    {
        var modifier = new AffixModifier(AffixMode.AddPrefix, "pre");

        var result = modifier.ModifyText("hello world");

        Assert.Equal("prehello preworld", result);
    }

    [Fact]
    public void ModifyText_AddSuffix_AddsToWords()
    {
        var modifier = new AffixModifier(AffixMode.AddSuffix, "ed");

        var result = modifier.ModifyText("walk run");

        Assert.Equal("walked runed", result);
    }

    [Fact]
    public void ModifyText_MoveFirstToEnd_MovesFirstCharacter()
    {
        var modifier = new AffixModifier(AffixMode.MoveFirstToEnd);

        var result = modifier.ModifyText("hello");

        Assert.Equal("elloh", result);
    }

    [Fact]
    public void ModifyText_MoveLastToStart_MovesLastCharacter()
    {
        var modifier = new AffixModifier(AffixMode.MoveLastToStart);

        var result = modifier.ModifyText("hello");

        Assert.Equal("ohell", result);
    }

    [Fact]
    public void ModifyText_PigLatin_StartsWithConsonant()
    {
        var modifier = new AffixModifier(AffixMode.PigLatin);

        var result = modifier.ModifyText("hello");

        // "hello" -> "ello" + "h" + "ay" = "ellohay"
        Assert.Equal("ellohay", result);
    }

    [Fact]
    public void ModifyText_PigLatin_StartsWithVowel()
    {
        var modifier = new AffixModifier(AffixMode.PigLatin);

        var result = modifier.ModifyText("apple");

        // "apple" -> "apple" + "ay" = "appleay"
        Assert.Equal("appleay", result);
    }

    [Fact]
    public void ModifyText_PigLatin_PreservesCapitalization()
    {
        var modifier = new AffixModifier(AffixMode.PigLatin);

        var result = modifier.ModifyText("Hello");

        // "Hello" -> "Ello" + "h" + "ay" = "Ellohay"
        Assert.Equal("Ellohay", result);
    }

    [Fact]
    public void ModifyText_PigLatin_MultipleWords()
    {
        var modifier = new AffixModifier(AffixMode.PigLatin);

        var result = modifier.ModifyText("hello world");

        Assert.Equal("ellohay orldway", result);
    }

    [Fact]
    public void ModifyText_PreservesSpacesAndPunctuation()
    {
        var modifier = new AffixModifier(AffixMode.AddPrefix, "x");

        var result = modifier.ModifyText("hello, world!");

        Assert.Equal("xhello, xworld!", result);
    }

    [Fact]
    public void ModifyText_WithEmptyString_ReturnsEmpty()
    {
        var modifier = new AffixModifier(AffixMode.AddPrefix, "pre");

        var result = modifier.ModifyText("");

        Assert.Equal("", result);
    }

    [Fact]
    public void ModifyText_MoveFirstToEnd_SingleCharWord_Unchanged()
    {
        var modifier = new AffixModifier(AffixMode.MoveFirstToEnd);

        var result = modifier.ModifyText("a b c");

        Assert.Equal("a b c", result);
    }

    [Fact]
    public void Constructor_AddPrefixWithoutAffix_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new AffixModifier(AffixMode.AddPrefix, null));
    }

    [Fact]
    public void Constructor_AddSuffixWithoutAffix_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new AffixModifier(AffixMode.AddSuffix, null));
    }
}

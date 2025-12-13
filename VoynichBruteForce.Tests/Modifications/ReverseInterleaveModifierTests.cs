using VoynichBruteForce.Modifications;

namespace VoynichBruteForce.Tests.Modifications;

public class ReverseInterleaveModifierTests
{
    [Fact]
    public void Modify_WithOneCharacter_DoublesTheCharacter()
    {
        var sut = new ReverseInterleaveModifier();

        var result = sut.ModifyText("A");
        
        Assert.Equal("AA", result);
    }
    
    [Theory]
    [InlineData("ABC", "ACBBCA")]
    public void Modify_WithString_ReversesAndInterleaves(string source, string expected)
    {
        var sut = new ReverseInterleaveModifier();

        var result = sut.ModifyText(source);
        
        Assert.Equal(expected, result);
    }
}
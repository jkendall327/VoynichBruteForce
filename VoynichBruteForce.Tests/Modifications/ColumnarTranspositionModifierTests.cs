using VoynichBruteForce.Modifications;

namespace VoynichBruteForce.Tests.Modifications;

public class ColumnarTranspositionModifierTests
{
    [Fact]
    public void ModifyText_WithSimpleColumnOrder_TransposesCorrectly()
    {
        // Column order [1, 0] means read column 1 first, then column 0
        // Grid for "ABCD" with 2 columns:
        // A B
        // C D
        // Reading column 1 then 0: B, D, A, C
        var modifier = new ColumnarTranspositionModifier([1, 0]);

        var result = modifier.ModifyText("ABCD");

        Assert.Equal("BDAC", result);
    }

    [Fact]
    public void ModifyText_WithThreeColumns_TransposesCorrectly()
    {
        // Column order [2, 0, 1] for "ABCDEF":
        // Grid:
        // A B C
        // D E F
        // Reading columns 2, 0, 1: C, F, A, D, B, E
        var modifier = new ColumnarTranspositionModifier([2, 0, 1]);

        var result = modifier.ModifyText("ABCDEF");

        Assert.Equal("CFADBE", result);
    }

    [Fact]
    public void ModifyText_FromKeyword_TransposesCorrectly()
    {
        // Keyword "CAB" -> alphabetically sorted gives column order
        // C=2, A=0, B=1 -> reading order is A(col0), B(col1), C(col2) -> [1, 2, 0] as indices
        var modifier = ColumnarTranspositionModifier.FromKeyword("CAB");

        var result = modifier.ModifyText("ABCDEF");

        // Let's verify: with "CAB", column positions are:
        // C is at position 0 in keyword, gets rank 2 (3rd alphabetically)
        // A is at position 1 in keyword, gets rank 0 (1st alphabetically)
        // B is at position 2 in keyword, gets rank 1 (2nd alphabetically)
        // So we read column 1 first, then column 2, then column 0
        // Grid: A B C / D E F
        // Reading col 1, 2, 0: B, E, C, F, A, D
        Assert.Equal("BECFAD", result);
    }

    [Fact]
    public void ModifyText_WithEmptyString_ReturnsEmpty()
    {
        var modifier = new ColumnarTranspositionModifier([0, 1]);

        var result = modifier.ModifyText("");

        Assert.Equal("", result);
    }

    [Fact]
    public void ModifyText_IsDeterministic_OnRepeatedCalls()
    {
        var modifier = new ColumnarTranspositionModifier([2, 0, 1, 3]);

        var result1 = modifier.ModifyText("HELLO WORLD");
        var result2 = modifier.ModifyText("HELLO WORLD");

        Assert.Equal(result1, result2);
    }

    [Fact]
    public void Constructor_WithInvalidColumnOrder_Throws()
    {
        // [0, 2] is invalid - missing 1
        Assert.Throws<ArgumentException>(() => new ColumnarTranspositionModifier([0, 2]));
    }

    [Fact]
    public void Constructor_WithEmptyColumnOrder_Throws()
    {
        Assert.Throws<ArgumentException>(() => new ColumnarTranspositionModifier([]));
    }
}

using Microsoft.Extensions.Options;
using VoynichBruteForce.Rankings;

namespace VoynichBruteForce.Tests.Rankings;

public class SingleCharEntropyRankerTests
{
    private static SingleCharEntropyRanker CreateRanker() =>
        new(Options.Create(new VoynichProfile()));

    private static PrecomputedTextAnalysis Analyze(string text) => new(text);

    [Fact]
    public void CalculateRank_WithUniformDistribution_ReturnsMaxEntropy()
    {
        var ranker = CreateRanker();

        // All characters appear with equal frequency
        var result = ranker.CalculateRank(Analyze("ABCD"));

        // H1 = -4 * (0.25 * log2(0.25)) = 2.0
        Assert.Equal(2.0, result.RawMeasuredValue, precision: 2);
    }

    [Fact]
    public void CalculateRank_WithSingleCharacter_ReturnsZeroEntropy()
    {
        var ranker = CreateRanker();

        // Only one character, no entropy
        var result = ranker.CalculateRank(Analyze("AAAA"));

        // H1 = 0 for single character
        Assert.Equal(0.0, result.RawMeasuredValue, precision: 2);
    }

    [Fact]
    public void CalculateRank_WithTwoCharacters_CalculatesCorrectly()
    {
        var ranker = CreateRanker();

        // Two characters with equal frequency
        var result = ranker.CalculateRank(Analyze("AABB"));

        // H1 = -2 * (0.5 * log2(0.5)) = 1.0
        Assert.Equal(1.0, result.RawMeasuredValue, precision: 2);
    }

    [Fact]
    public void CalculateRank_WithEmptyText_ReturnsZero()
    {
        var ranker = CreateRanker();

        var result = ranker.CalculateRank(Analyze(""));

        Assert.Equal(0.0, result.RawMeasuredValue);
    }

    [Fact]
    public void CalculateRank_ReturnsCorrectRuleName()
    {
        var ranker = CreateRanker();

        var result = ranker.CalculateRank(Analyze("test text"));

        Assert.Equal("H1 Character Entropy", result.RuleName);
    }

    [Fact]
    public void CalculateRank_ReturnsTargetValue()
    {
        var ranker = CreateRanker();

        var result = ranker.CalculateRank(Analyze("test text"));

        Assert.Equal(new VoynichProfile().TargetH1Entropy, result.TargetValue);
    }

    [Fact]
    public void Weight_ReturnsStandard()
    {
        var ranker = CreateRanker();

        Assert.Equal(RuleWeight.Standard, ranker.Weight);
    }

    [Fact]
    public void CalculateRank_CalculatesNormalizedError()
    {
        var ranker = CreateRanker();

        var result = ranker.CalculateRank(Analyze("the quick brown fox"));

        // Should have non-zero normalized error
        Assert.True(result.NormalizedError >= 0);
    }
}

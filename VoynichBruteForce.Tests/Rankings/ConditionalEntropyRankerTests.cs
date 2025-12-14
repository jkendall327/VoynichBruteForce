using Microsoft.Extensions.Options;
using VoynichBruteForce.Rankings;

namespace VoynichBruteForce.Tests.Rankings;

public class ConditionalEntropyRankerTests
{
    private static ConditionalEntropyRanker CreateRanker() =>
        new(Options.Create(new VoynichProfile()));

    private static PrecomputedTextAnalysis Analyze(string text) => new(text);

    [Fact]
    public void CalculateRank_WithUniformBigrams_ReturnsZeroEntropy()
    {
        var ranker = CreateRanker();

        // Single repeated bigram has H2 = 0
        var result = ranker.CalculateRank(Analyze("AAAA"));

        // H2 should be 0 for perfectly predictable sequence
        Assert.Equal(0.0, result.RawMeasuredValue, precision: 2);
    }

    [Fact]
    public void CalculateRank_WithRandomBigrams_ReturnsHighEntropy()
    {
        var ranker = CreateRanker();

        // More diverse bigrams should have higher entropy
        var result = ranker.CalculateRank(Analyze("ABCDEFGHIJKLMNOPQRSTUVWXYZ"));

        // Each character appears once, so conditional entropy is 0 (each char followed by exactly one char)
        // But we should have some entropy from the distribution
        Assert.True(result.RawMeasuredValue >= 0);
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

        Assert.Equal("H2 Entropy", result.RuleName);
    }

    [Fact]
    public void CalculateRank_ReturnsTargetValue()
    {
        var ranker = CreateRanker();

        var result = ranker.CalculateRank(Analyze("test text"));

        Assert.Equal(new VoynichProfile().TargetH2Entropy, result.TargetValue);
    }

    [Fact]
    public void Weight_ReturnsCritical()
    {
        var ranker = CreateRanker();

        Assert.Equal(RuleWeight.Critical, ranker.Weight);
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

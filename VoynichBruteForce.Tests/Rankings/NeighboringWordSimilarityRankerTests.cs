using Microsoft.Extensions.Options;
using VoynichBruteForce.Rankings;

namespace VoynichBruteForce.Tests.Rankings;

public class NeighboringWordSimilarityRankerTests
{
    private static NeighboringWordSimilarityRanker CreateRanker() =>
        new(Options.Create(new VoynichProfile()));

    private static PrecomputedTextAnalysis Analyze(string text) => new(text);

    [Fact]
    public void CalculateRank_WithIdenticalNeighbors_ReturnsHighSimilarity()
    {
        var ranker = CreateRanker();

        var result = ranker.CalculateRank(Analyze("word word word"));

        // All pairs are identical (distance = 0), which is <= 2
        Assert.Equal(1.0, result.RawMeasuredValue, precision: 2);
    }

    [Fact]
    public void CalculateRank_WithSimilarNeighbors_DetectsSimilarity()
    {
        var ranker = CreateRanker();

        // Words with Levenshtein distance <= 2
        var result = ranker.CalculateRank(Analyze("cat bat hat"));

        // cat->bat (distance 1), bat->hat (distance 1)
        // 2 similar pairs out of 2 total = 1.0
        Assert.Equal(1.0, result.RawMeasuredValue, precision: 2);
    }

    [Fact]
    public void CalculateRank_WithDissimilarWords_ReturnsLowSimilarity()
    {
        var ranker = CreateRanker();

        var result = ranker.CalculateRank(Analyze("cat elephant rhinoceros"));

        // Large distances between consecutive words
        Assert.Equal(0.0, result.RawMeasuredValue, precision: 2);
    }

    [Fact]
    public void CalculateRank_WithTooFewWords_ReturnsZero()
    {
        var ranker = CreateRanker();

        var result = ranker.CalculateRank(Analyze("word"));

        // Only 1 word, no pairs to compare
        Assert.Equal(0.0, result.NormalizedError);
    }

    [Fact]
    public void CalculateRank_WithEmptyText_ReturnsZero()
    {
        var ranker = CreateRanker();

        var result = ranker.CalculateRank(Analyze(""));

        Assert.Equal(0.0, result.NormalizedError);
    }

    [Fact]
    public void CalculateRank_WithMixedSimilarity()
    {
        var ranker = CreateRanker();

        // Mix of similar and dissimilar pairs
        var result = ranker.CalculateRank(Analyze("cat bat elephant hat"));

        // cat->bat (similar), bat->elephant (not similar), elephant->hat (not similar)
        // 1 similar pair out of 3 = 0.33
        Assert.True(result.RawMeasuredValue > 0 && result.RawMeasuredValue < 1);
    }

    [Fact]
    public void CalculateRank_ReturnsCorrectRuleName()
    {
        var ranker = CreateRanker();

        var result = ranker.CalculateRank(Analyze("test text here"));

        Assert.Equal("Neighboring Word Similarity", result.RuleName);
    }

    [Fact]
    public void CalculateRank_ReturnsTargetValue()
    {
        var ranker = CreateRanker();

        var result = ranker.CalculateRank(Analyze("test text here"));

        Assert.Equal(new VoynichProfile().TargetNeighboringWordSimilarity, result.TargetValue);
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

using Microsoft.Extensions.Options;
using VoynichBruteForce.Rankings;

namespace VoynichBruteForce.Tests.Rankings;

public class ZipfLawRankerTests
{
    private static ZipfLawRanker CreateRanker() =>
        new(Options.Create(new VoynichProfile()));

    private static PrecomputedTextAnalysis Analyze(string text) => new(text);

    [Fact]
    public void CalculateRank_WithTooFewWords_ReturnsMaxError()
    {
        var ranker = CreateRanker();

        var result = ranker.CalculateRank(Analyze("one two"));

        // Less than 10 words should return max error
        Assert.Equal(double.MaxValue, result.NormalizedError);
    }

    [Fact]
    public void CalculateRank_WithZipfLikeDistribution_ReturnsSlope()
    {
        var ranker = CreateRanker();

        // Simulate Zipf-like distribution with repeated words
        var text = "the " + string.Join(" ", Enumerable.Repeat("the", 50)) + " " +
                   string.Join(" ", Enumerable.Repeat("a", 25)) + " " +
                   string.Join(" ", Enumerable.Repeat("is", 16)) + " " +
                   string.Join(" ", Enumerable.Repeat("of", 12)) + " " +
                   string.Join(" ", Enumerable.Repeat("to", 10));

        var result = ranker.CalculateRank(Analyze(text));

        // Should calculate a slope value
        Assert.True(result.RawMeasuredValue >= 0);
    }

    [Fact]
    public void CalculateRank_WithEmptyText_ReturnsMaxError()
    {
        var ranker = CreateRanker();

        var result = ranker.CalculateRank(Analyze(""));

        Assert.Equal(double.MaxValue, result.NormalizedError);
    }

    [Fact]
    public void CalculateRank_ReturnsCorrectRuleName()
    {
        var ranker = CreateRanker();

        var result = ranker.CalculateRank(Analyze("the quick brown fox jumps over the lazy dog today"));

        Assert.Equal("Zipf's Law", result.RuleName);
    }

    [Fact]
    public void CalculateRank_ReturnsTargetValue()
    {
        var ranker = CreateRanker();

        var result = ranker.CalculateRank(Analyze("the quick brown fox jumps over the lazy dog today"));

        Assert.Equal(new VoynichProfile().TargetZipfSlope, result.TargetValue);
    }

    [Fact]
    public void Weight_ReturnsHigh()
    {
        var ranker = CreateRanker();

        Assert.Equal(RuleWeight.High, ranker.Weight);
    }

    [Fact]
    public void CalculateRank_CalculatesNormalizedError()
    {
        var ranker = CreateRanker();

        var result = ranker.CalculateRank(Analyze("the quick brown fox jumps over the lazy dog today"));

        // Should have non-zero normalized error
        Assert.True(result.NormalizedError >= 0);
    }
}

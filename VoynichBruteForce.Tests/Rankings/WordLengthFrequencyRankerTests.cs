using Microsoft.Extensions.Options;
using VoynichBruteForce.Rankings;

namespace VoynichBruteForce.Tests.Rankings;

public class WordLengthFrequencyRankerTests
{
    private static WordLengthFrequencyRanker CreateRanker() =>
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
    public void CalculateRank_WithNegativeCorrelation_ShorterWordsMoreFrequent()
    {
        var ranker = CreateRanker();

        // Shorter words repeated more often
        var text = string.Join(" ", Enumerable.Repeat("a", 20)) + " " +
                   string.Join(" ", Enumerable.Repeat("the", 10)) + " " +
                   string.Join(" ", Enumerable.Repeat("quick", 5)) + " " +
                   string.Join(" ", Enumerable.Repeat("jumps", 3)) + " " +
                   string.Join(" ", Enumerable.Repeat("running", 1));

        var result = ranker.CalculateRank(Analyze(text));

        // Should show negative correlation (shorter = more frequent)
        Assert.True(result.RawMeasuredValue < 0);
    }

    [Fact]
    public void CalculateRank_WithEmptyText_ReturnsMaxError()
    {
        var ranker = CreateRanker();

        var result = ranker.CalculateRank(Analyze(""));

        Assert.Equal(double.MaxValue, result.NormalizedError);
    }

    [Fact]
    public void CalculateRank_WithSingleUniqueWord_ReturnsMaxError()
    {
        var ranker = CreateRanker();

        var result = ranker.CalculateRank(Analyze("word word word word word word word word word word"));

        // Only 1 unique word, can't calculate correlation
        Assert.Equal(double.MaxValue, result.NormalizedError);
    }

    [Fact]
    public void CalculateRank_ReturnsCorrectRuleName()
    {
        var ranker = CreateRanker();

        var result = ranker.CalculateRank(Analyze("the quick brown fox jumps over the lazy dog today"));

        Assert.Equal("Word Length-Frequency Correlation", result.RuleName);
    }

    [Fact]
    public void CalculateRank_ReturnsTargetValue()
    {
        var ranker = CreateRanker();

        var result = ranker.CalculateRank(Analyze("the quick brown fox jumps over the lazy dog today"));

        Assert.Equal(new VoynichProfile().TargetWordLengthFrequencyCorrelation, result.TargetValue);
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

        var result = ranker.CalculateRank(Analyze("the quick brown fox jumps over the lazy dog today"));

        // Should have non-zero normalized error
        Assert.True(result.NormalizedError >= 0);
    }
}

using Microsoft.Extensions.Options;
using VoynichBruteForce.Rankings;

namespace VoynichBruteForce.Tests.Rankings;

public class RepeatedAdjacentWordsRankerTests
{
    private static RepeatedAdjacentWordsRanker CreateRanker() => new(Options.Create(new VoynichProfile()));

    private static PrecomputedTextAnalysis Analyze(string text) => new(text);

    [Fact]
    public void CalculateRank_WithNoRepetitions_ReturnsZeroRatio()
    {
        var ranker = CreateRanker();

        var result = ranker.CalculateRank(Analyze("the quick brown fox jumps over lazy dog"));

        Assert.Equal(0.0, result.RawMeasuredValue);
    }

    [Fact]
    public void CalculateRank_WithOneRepetition_CalculatesCorrectRatio()
    {
        var ranker = CreateRanker();

        // "the the" is one repetition out of 4 words = 1/4 = 0.25
        var result = ranker.CalculateRank(Analyze("the the cat dog"));

        Assert.Equal(0.25, result.RawMeasuredValue, precision: 2);
    }

    [Fact]
    public void CalculateRank_WithMultipleRepetitions_CalculatesCorrectRatio()
    {
        var ranker = CreateRanker();

        // "word word" and "test test" = 2 repetitions out of 6 words = 2/6 ≈ 0.333
        var result = ranker.CalculateRank(Analyze("word word test test hello world"));

        Assert.Equal(0.333, result.RawMeasuredValue, precision: 2);
    }

    [Fact]
    public void CalculateRank_IsCaseInsensitive()
    {
        var ranker = CreateRanker();

        var result = ranker.CalculateRank(Analyze("The THE cat CAT"));

        // 2 repetitions out of 4 words = 0.5
        Assert.Equal(0.5, result.RawMeasuredValue);
    }

    [Fact]
    public void CalculateRank_WithEmptyString_ReturnsZero()
    {
        var ranker = CreateRanker();

        var result = ranker.CalculateRank(Analyze(""));

        Assert.Equal(0.0, result.RawMeasuredValue);
    }

    [Fact]
    public void CalculateRank_WithSingleWord_ReturnsZero()
    {
        var ranker = CreateRanker();

        var result = ranker.CalculateRank(Analyze("hello"));

        Assert.Equal(0.0, result.RawMeasuredValue);
    }

    [Fact]
    public void CalculateRank_HandlesMultipleWhitespaceTypes()
    {
        var ranker = CreateRanker();

        var result = ranker.CalculateRank(Analyze("word\tword\ntest\rtest"));

        // 2 repetitions out of 4 words = 0.5
        Assert.Equal(0.5, result.RawMeasuredValue);
    }

    [Fact]
    public void CalculateRank_ReturnsCorrectRuleName()
    {
        var ranker = CreateRanker();

        var result = ranker.CalculateRank(Analyze("test test"));

        Assert.Equal("Repeated Adjacent Words", result.RuleName);
    }

    [Fact]
    public void CalculateRank_ReturnsTargetValue()
    {
        var ranker = CreateRanker();

        var result = ranker.CalculateRank(Analyze("test test"));

        Assert.Equal(new VoynichProfile().TargetRepeatedAdjacentWordsRatio, result.TargetValue);
    }

    [Fact]
    public void CalculateRank_CalculatesNormalizedError()
    {
        var ranker = CreateRanker();

        var result = ranker.CalculateRank(Analyze("word word word")); // 2/3 ≈ 0.667

        // Should have a non-zero normalized error since 0.667 != target
        Assert.True(result.NormalizedError > 0);
    }

    [Fact]
    public void Weight_ReturnsStandard()
    {
        var ranker = CreateRanker();

        Assert.Equal(RuleWeight.Standard, ranker.Weight);
    }
}

using Microsoft.Extensions.Options;
using VoynichBruteForce.Rankings;

namespace VoynichBruteForce.Tests.Rankings;

public class VocabularySizeRankerTests
{
    private static VocabularySizeRanker CreateRanker() => new(Options.Create(new VoynichProfile()));

    [Fact]
    public void CalculateRank_WithAllUniqueWords_ReturnsOne()
    {
        var ranker = CreateRanker();

        var result = ranker.CalculateRank("the quick brown fox");

        // 4 unique words / 4 total words = 1.0
        Assert.Equal(1.0, result.RawMeasuredValue);
    }

    [Fact]
    public void CalculateRank_WithAllSameWord_ReturnsLowRatio()
    {
        var ranker = CreateRanker();

        var result = ranker.CalculateRank("word word word word");

        // 1 unique word / 4 total words = 0.25
        Assert.Equal(0.25, result.RawMeasuredValue);
    }

    [Fact]
    public void CalculateRank_WithMixedRepetition_CalculatesCorrectly()
    {
        var ranker = CreateRanker();

        var result = ranker.CalculateRank("the cat and the dog and the bird");

        // Unique: the, cat, and, dog, bird = 5 unique
        // Total: 8 words
        // TTR = 5/8 = 0.625
        Assert.Equal(0.625, result.RawMeasuredValue, precision: 3);
    }

    [Fact]
    public void CalculateRank_IsCaseInsensitive()
    {
        var ranker = CreateRanker();

        var result = ranker.CalculateRank("Hello HELLO hello HeLLo");

        // All same word, different cases = 1 unique / 4 total = 0.25
        Assert.Equal(0.25, result.RawMeasuredValue);
    }

    [Fact]
    public void CalculateRank_WithEmptyString_ReturnsZero()
    {
        var ranker = CreateRanker();

        var result = ranker.CalculateRank("");

        Assert.Equal(0.0, result.RawMeasuredValue);
    }

    [Fact]
    public void CalculateRank_WithSingleWord_ReturnsOne()
    {
        var ranker = CreateRanker();

        var result = ranker.CalculateRank("hello");

        // 1 unique / 1 total = 1.0
        Assert.Equal(1.0, result.RawMeasuredValue);
    }

    [Fact]
    public void CalculateRank_HandlesMultipleWhitespaceTypes()
    {
        var ranker = CreateRanker();

        var result = ranker.CalculateRank("word\tcat\ndog\rword");

        // 3 unique words / 4 total = 0.75
        Assert.Equal(0.75, result.RawMeasuredValue);
    }

    [Fact]
    public void CalculateRank_ReturnsCorrectRuleName()
    {
        var ranker = CreateRanker();

        var result = ranker.CalculateRank("test words");

        Assert.Equal("Type-Token Ratio (Vocabulary Size)", result.RuleName);
    }

    [Fact]
    public void CalculateRank_ReturnsTargetValue()
    {
        var ranker = CreateRanker();

        var result = ranker.CalculateRank("test words");

        Assert.Equal(new VoynichProfile().TargetTypeTokenRatio, result.TargetValue);
    }

    [Fact]
    public void CalculateRank_CalculatesNormalizedError()
    {
        var ranker = CreateRanker();

        var result = ranker.CalculateRank("the quick brown fox");

        // TTR = 1.0, which is different from target
        // Should have non-zero normalized error
        Assert.True(result.NormalizedError > 0);
    }

    [Fact]
    public void Weight_ReturnsStandard()
    {
        var ranker = CreateRanker();

        Assert.Equal(RuleWeight.Standard, ranker.Weight);
    }

    [Fact]
    public void CalculateRank_LowTTRIndicatesRepetition()
    {
        var ranker = CreateRanker();

        // Voynich-like text with lots of repetition
        var result = ranker.CalculateRank("daiin daiin ol ol chol chol daiin");

        // 3 unique / 7 total ≈ 0.4286
        Assert.Equal(0.4286, result.RawMeasuredValue, precision: 3);
    }
}

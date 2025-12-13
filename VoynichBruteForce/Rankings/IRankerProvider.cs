using Microsoft.Extensions.Options;

namespace VoynichBruteForce.Rankings;

public interface IRankerProvider
{
    List<IRuleAdherenceRanker> GetRankers();
}

public class DefaultRankerProvider(IOptions<VoynichProfile> profile) : IRankerProvider
{
    public List<IRuleAdherenceRanker> GetRankers()
    {
        return
        [
            new ConditionalEntropyRanker(profile),
            new SingleCharEntropyRanker(profile),
            new ZipfLawRanker(profile),
            new RepeatedAdjacentWordsRanker(profile),
            new VocabularySizeRanker(profile),
            new WordLengthFrequencyRanker(profile),
            new NeighboringWordSimilarityRanker(profile)
        ];
    }
}
namespace VoynichBruteForce.Rankings;

public interface IRankerProvider
{
    List<IRuleAdherenceRanker> GetRankers();
}

public class DefaultRankerProvider : IRankerProvider
{
    public List<IRuleAdherenceRanker> GetRankers()
    {
        return [];
    }
}
namespace VoynichBruteForce.Rankings;

public interface IRankerProvider
{
    List<IRuleAdherenceRanker> GetRankers();
}
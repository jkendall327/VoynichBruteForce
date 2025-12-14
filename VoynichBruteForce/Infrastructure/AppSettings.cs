namespace VoynichBruteForce;

public class AppSettings
{
    public const string SectionName = "AppSettings";
    public int Seed { get; init; }
    public int DegreeOfParallelism { get; set; }
    public bool RunBenchmark { get; set; }
}
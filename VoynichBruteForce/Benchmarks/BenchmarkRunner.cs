using BenchmarkDotNet.Running;

namespace VoynichBruteForce.Benchmarks;

public static class EvolutionBenchmarkRunner
{
    public static void Run()
    {
        BenchmarkRunner.Run<EvolutionBenchmarks>();
    }
}

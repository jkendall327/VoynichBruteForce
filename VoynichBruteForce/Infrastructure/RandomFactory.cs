using Microsoft.Extensions.Options;

namespace VoynichBruteForce;

public class RandomFactory(IOptions<AppSettings> options)
{
    private readonly Random _random = new(options.Value.Seed);

    public Random GetRandom() => _random;
}
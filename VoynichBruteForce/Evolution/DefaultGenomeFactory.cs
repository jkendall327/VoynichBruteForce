using Microsoft.Extensions.Options;
using VoynichBruteForce.Modifications;

namespace VoynichBruteForce.Evolution;

public class DefaultGenomeFactory(IOptions<AppSettings> options) : IGenomeFactory
{
    public List<ITextModifier> CreateRandomGenome(int length)
    {
        throw new NotImplementedException();
    }

    public List<ITextModifier> Mutate(List<ITextModifier> original)
    {
        throw new NotImplementedException("Randomly switch on GenomeMutationStrategy here?");
    }

    public List<ITextModifier> Crossover(List<ITextModifier> parentA, List<ITextModifier> parentB)
    {
        // TODO: properly understand this code instead of cargo-culting it.

        var child = new List<ITextModifier>();
        var random = new Random(options.Value.Seed);

        // Pick a split point based on the shorter parent to avoid index errors
        var minLen = Math.Min(parentA.Count, parentB.Count);
        var splitPoint = random.Next(0, minLen);

        // Take head from A
        for (var i = 0; i < splitPoint; i++)
        {
            child.Add(parentA[i]);
        }

        // Take tail from B
        for (var i = splitPoint; i < parentB.Count; i++)
        {
            child.Add(parentB[i]);
        }

        return child;
    }
}
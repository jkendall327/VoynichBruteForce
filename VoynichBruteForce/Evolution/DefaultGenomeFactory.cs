using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VoynichBruteForce.Modifications;

namespace VoynichBruteForce.Evolution;

public partial class DefaultGenomeFactory(IOptions<AppSettings> options, ILogger<DefaultGenomeFactory> logger) : IGenomeFactory
{
    private static readonly Type[] ModifierTypes =
    [
        typeof(CaesarCipherModifier),
        typeof(AtbashCipherModifier),
        typeof(VowelRemovalModifier),
        typeof(PositionalExtractionModifier),
        typeof(NullInsertionModifier),
        typeof(LetterDoublingModifier),
        typeof(AnagramModifier),
        typeof(AffixModifier),
        typeof(ConsonantVowelSplitModifier),
        typeof(ColumnarTranspositionModifier),
        typeof(SkipCipherModifier),
        typeof(InterleaveModifier),
        typeof(WordReversalModifier)
    ];

    public List<ITextModifier> CreateRandomGenome(int length)
    {
        var random = new Random(options.Value.Seed);
        var genome = new List<ITextModifier>(length);

        for (var i = 0; i < length; i++)
        {
            genome.Add(CreateRandomModifier(random));
        }

        return genome;
    }

    public List<ITextModifier> Mutate(List<ITextModifier> original)
    {
        var random = new Random(options.Value.Seed);
        var mutated = new List<ITextModifier>(original);

        // Choose a random mutation strategy
        var strategy = random.Next(4);

        switch (strategy)
        {
            case 0: // Replace a random modifier
                if (mutated.Count > 0)
                {
                    var index = random.Next(mutated.Count);
                    mutated[index] = CreateRandomModifier(random);
                }
                break;

            case 1: // Swap two modifiers
                if (mutated.Count > 1)
                {
                    var i = random.Next(mutated.Count);
                    var j = random.Next(mutated.Count);
                    (mutated[i], mutated[j]) = (mutated[j], mutated[i]);
                }
                break;

            case 2: // Remove a random modifier
                if (mutated.Count > 1)
                {
                    var index = random.Next(mutated.Count);
                    mutated.RemoveAt(index);
                }
                break;

            case 3: // Add a random modifier
                mutated.Insert(random.Next(mutated.Count + 1), CreateRandomModifier(random));
                break;
        }

        LogMutation(logger, original.Count);

        return mutated;
    }

    private ITextModifier CreateRandomModifier(Random random)
    {
        var type = ModifierTypes[random.Next(ModifierTypes.Length)];

        return type.Name switch
        {
            nameof(CaesarCipherModifier) => new CaesarCipherModifier(random.Next(26)),
            nameof(AtbashCipherModifier) => new AtbashCipherModifier(),
            nameof(VowelRemovalModifier) => new VowelRemovalModifier(),
            nameof(PositionalExtractionModifier) => new PositionalExtractionModifier(random.Next(2, 5)),
            nameof(NullInsertionModifier) => new NullInsertionModifier((char)('a' + random.Next(26)), random.Next(3, 10)),
            nameof(LetterDoublingModifier) => new LetterDoublingModifier(),
            nameof(AnagramModifier) => new AnagramModifier(
                (AnagramMode)random.Next(Enum.GetValues<AnagramMode>().Length),
                random.Next(1000)),
            nameof(AffixModifier) => CreateRandomAffixModifier(random),
            nameof(ConsonantVowelSplitModifier) => new ConsonantVowelSplitModifier(),
            nameof(ColumnarTranspositionModifier) => new ColumnarTranspositionModifier(
                GenerateRandomColumnOrder(random, random.Next(3, 7))),
            nameof(SkipCipherModifier) => new SkipCipherModifier(random.Next(2, 5)),
            nameof(InterleaveModifier) => new InterleaveModifier(
                random.Next(2) == 0 ? InterleaveMode.HalvesAlternate : InterleaveMode.OddEvenSplit),
            nameof(WordReversalModifier) => new WordReversalModifier(),
            _ => throw new InvalidOperationException($"Unknown modifier type: {type.Name}")
        };
    }

    private static AffixModifier CreateRandomAffixModifier(Random random)
    {
        var mode = (AffixMode)random.Next(Enum.GetValues<AffixMode>().Length);
        return mode switch
        {
            AffixMode.AddPrefix => new AffixModifier(mode, GenerateRandomString(random, 2, 4)),
            AffixMode.AddSuffix => new AffixModifier(mode, GenerateRandomString(random, 2, 4)),
            _ => new AffixModifier(mode)
        };
    }

    private static int[] GenerateRandomColumnOrder(Random random, int length)
    {
        var order = Enumerable.Range(0, length).ToArray();
        // Fisher-Yates shuffle
        for (var i = order.Length - 1; i > 0; i--)
        {
            var j = random.Next(i + 1);
            (order[i], order[j]) = (order[j], order[i]);
        }
        return order;
    }

    private static string GenerateRandomString(Random random, int minLength, int maxLength)
    {
        var length = random.Next(minLength, maxLength + 1);
        var chars = new char[length];
        for (var i = 0; i < length; i++)
        {
            chars[i] = (char)('a' + random.Next(26));
        }
        return new string(chars);
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

        LogCrossover(logger, parentA.Count, parentB.Count, splitPoint, child.Count);

        return child;
    }

    [LoggerMessage(LogLevel.Debug, "Crossover: ParentA={parentACount}, ParentB={parentBCount}, SplitPoint={splitPoint} -> Child={childCount}")]
    static partial void LogCrossover(ILogger<DefaultGenomeFactory> logger, int parentACount, int parentBCount, int splitPoint, int childCount);

    [LoggerMessage(LogLevel.Debug, "Mutation applied: Original={originalCount} modifiers")]
    static partial void LogMutation(ILogger<DefaultGenomeFactory> logger, int originalCount);
}
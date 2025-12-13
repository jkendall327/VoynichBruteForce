using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VoynichBruteForce.Modifications;
using VoynichBruteForce.Sources;

namespace VoynichBruteForce.Evolution;

public partial class DefaultGenomeFactory(
    RandomFactory randomFactory,
    ISourceTextRegistry sourceTextRegistry,
    ILogger<DefaultGenomeFactory> logger) : IGenomeFactory
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

    public Genome CreateRandomGenome(int modifierCount)
    {
        var random = randomFactory.GetRandom();

        var sourceTextId = sourceTextRegistry.GetRandomId(random);
        var modifiers = CreateRandomModifiers(random, modifierCount);

        return new(sourceTextId, modifiers);
    }

    public Genome Mutate(Genome original)
    {
        var random = randomFactory.GetRandom();

        var mutationTarget = random.RandomEnumMember<MutationTarget>();

        var newSourceTextId = original.SourceTextId;
        var newModifiers = new List<ITextModifier>(original.Modifiers);

        if (mutationTarget is MutationTarget.Source or MutationTarget.Both)
        {
            // Mutate source text: pick a different source
            var oldSourceTextId = newSourceTextId;
            newSourceTextId = sourceTextRegistry.GetRandomId(random);
            LogSourceTextMutation(logger, oldSourceTextId, newSourceTextId);
        }

        if (mutationTarget is MutationTarget.Modifiers or MutationTarget.Both)
        {
            // Mutate modifiers using existing logic
            newModifiers = MutateModifiers(newModifiers, random);
        }

        return new(newSourceTextId, newModifiers);
    }

    public Genome Crossover(Genome parentA, Genome parentB)
    {
        var random = randomFactory.GetRandom();

        // Source text: 50/50 uniform crossover (standard for categorical genes)
        var childSourceTextId = random.Next(2) == 0 ? parentA.SourceTextId : parentB.SourceTextId;

        // Modifiers: existing single-point crossover logic
        var childModifiers = CrossoverModifiers(parentA.Modifiers, parentB.Modifiers, random);

        LogCrossover(logger,
            parentA.Modifiers.Count,
            parentB.Modifiers.Count,
            childModifiers.Count,
            parentA.SourceTextId,
            parentB.SourceTextId,
            childSourceTextId);

        return new(childSourceTextId, childModifiers);
    }

    private List<ITextModifier> CreateRandomModifiers(Random random, int count)
    {
        var modifiers = new List<ITextModifier>(count);

        for (var i = 0; i < count; i++)
        {
            modifiers.Add(CreateRandomModifier(random));
        }

        return modifiers;
    }

    private List<ITextModifier> MutateModifiers(List<ITextModifier> original, Random random)
    {
        var mutated = new List<ITextModifier>(original);

        switch (random.RandomEnumMember<MutationStrategy>())
        {
            case MutationStrategy.Replace:
                if (mutated.Count > 0)
                {
                    var index = random.Next(mutated.Count);
                    mutated[index] = CreateRandomModifier(random);
                }

                break;

            case MutationStrategy.Swap:
                if (mutated.Count > 1)
                {
                    var i = random.Next(mutated.Count);
                    var j = random.Next(mutated.Count);
                    (mutated[i], mutated[j]) = (mutated[j], mutated[i]);
                }

                break;

            case MutationStrategy.Remove:
                if (mutated.Count > 1)
                {
                    var index = random.Next(mutated.Count);
                    mutated.RemoveAt(index);
                }

                break;

            case MutationStrategy.Add:
                mutated.Insert(random.Next(mutated.Count + 1), CreateRandomModifier(random));

                break;

            case MutationStrategy.Perturb:
                var perturbableIndices = mutated
                    .Select((m, i) => (Modifier: m, Index: i))
                    .Where(x => x.Modifier is IPerturbable)
                    .Select(x => x.Index)
                    .ToList();

                if (perturbableIndices.Count > 0)
                {
                    var index = perturbableIndices[random.Next(perturbableIndices.Count)];
                    var perturbable = (IPerturbable)mutated[index];
                    mutated[index] = perturbable.Perturb(random);
                }

                // If no perturbable modifiers, mutation has no effect (preserves genome)
                break;
        }

        LogModifierMutation(logger, original.Count);

        return mutated;
    }

    private List<ITextModifier> CrossoverModifiers(List<ITextModifier> parentA,
        List<ITextModifier> parentB,
        Random random)
    {
        var child = new List<ITextModifier>();

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

    private ITextModifier CreateRandomModifier(Random random)
    {
        var type = ModifierTypes[random.Next(ModifierTypes.Length)];

        return type.Name switch
        {
            nameof(CaesarCipherModifier) => new CaesarCipherModifier(random.Next(26)),
            nameof(AtbashCipherModifier) => new AtbashCipherModifier(),
            nameof(VowelRemovalModifier) => new VowelRemovalModifier(),
            nameof(PositionalExtractionModifier) => new PositionalExtractionModifier(random.Next(2, 5)),
            nameof(NullInsertionModifier) => new NullInsertionModifier((char) ('a' + random.Next(26)),
                random.Next(3, 10)),
            nameof(LetterDoublingModifier) => new LetterDoublingModifier(),
            nameof(AnagramModifier) => new AnagramModifier((AnagramMode) random.Next(Enum.GetValues<AnagramMode>()
                    .Length),
                random.Next(1000)),
            nameof(AffixModifier) => CreateRandomAffixModifier(random),
            nameof(ConsonantVowelSplitModifier) => new ConsonantVowelSplitModifier(),
            nameof(ColumnarTranspositionModifier) => new ColumnarTranspositionModifier(
                GenerateRandomColumnOrder(random, random.Next(3, 7))),
            nameof(SkipCipherModifier) => new SkipCipherModifier(random.Next(2, 5)),
            nameof(InterleaveModifier) => new InterleaveModifier(random.Next(2) == 0
                ? InterleaveMode.HalvesAlternate
                : InterleaveMode.OddEvenSplit),
            nameof(WordReversalModifier) => new WordReversalModifier(),
            _ => throw new InvalidOperationException($"Unknown modifier type: {type.Name}")
        };
    }

    private static AffixModifier CreateRandomAffixModifier(Random random)
    {
        var mode = random.RandomEnumMember<AffixMode>();

        return mode switch
        {
            AffixMode.AddPrefix or AffixMode.AddSuffix => new(mode, GenerateRandomString(random, 2, 4)),
            _ => new(mode)
        };
    }

    private static int[] GenerateRandomColumnOrder(Random random, int length)
    {
        var order = Enumerable
            .Range(0, length)
            .ToArray();

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
            chars[i] = (char) ('a' + random.Next(26));
        }

        return new(chars);
    }

    [LoggerMessage(LogLevel.Debug, "Source text mutation: {OldSourceTextId} -> {NewSourceTextId}")]
    static partial void LogSourceTextMutation(ILogger<DefaultGenomeFactory> logger,
        SourceTextId oldSourceTextId,
        SourceTextId newSourceTextId);

    [LoggerMessage(LogLevel.Debug, "Modifier mutation applied: Original={OriginalCount} modifiers")]
    static partial void LogModifierMutation(ILogger<DefaultGenomeFactory> logger, int originalCount);

    [LoggerMessage(LogLevel.Debug,
        "Crossover: ParentA={ParentACount} ({ParentASource}), ParentB={ParentBCount} ({ParentBSource}) -> Child={ChildCount} ({ChildSource})")]
    static partial void LogCrossover(ILogger<DefaultGenomeFactory> logger,
        int parentACount,
        int parentBCount,
        int childCount,
        SourceTextId parentASource,
        SourceTextId parentBSource,
        SourceTextId childSource);
}
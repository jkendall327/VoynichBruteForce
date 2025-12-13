using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VoynichBruteForce.Modifications;
using VoynichBruteForce.Sources;

namespace VoynichBruteForce.Evolution;

public partial class DefaultGenomeFactory : IGenomeFactory
{
    private readonly RandomFactory _randomFactory;
    private readonly ISourceTextRegistry _sourceTextRegistry;
    private readonly ILogger<DefaultGenomeFactory> _logger;
    private readonly IModifierFactory[] _modifierFactories;

    public DefaultGenomeFactory(
        RandomFactory randomFactory,
        ISourceTextRegistry sourceTextRegistry,
        IEnumerable<IModifierFactory> modifierFactories,
        ILogger<DefaultGenomeFactory> logger)
    {
        _randomFactory = randomFactory;
        _sourceTextRegistry = sourceTextRegistry;
        _modifierFactories = modifierFactories.ToArray();
        _logger = logger;
    }

    public Genome CreateRandomGenome(int modifierCount)
    {
        var random = _randomFactory.GetRandom();

        var sourceTextId = _sourceTextRegistry.GetRandomId(random);
        var modifiers = CreateRandomModifiers(random, modifierCount);

        return new(sourceTextId, modifiers);
    }

    public Genome Mutate(Genome original)
    {
        var random = _randomFactory.GetRandom();

        var mutationTarget = random.RandomEnumMember<MutationTarget>();

        var newSourceTextId = original.SourceTextId;
        var newModifiers = new List<ITextModifier>(original.Modifiers);

        if (mutationTarget is MutationTarget.Source or MutationTarget.Both)
        {
            // Mutate source text: pick a different source
            var oldSourceTextId = newSourceTextId;
            newSourceTextId = _sourceTextRegistry.GetRandomId(random);
            LogSourceTextMutation(_logger, oldSourceTextId, newSourceTextId);
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
        var random = _randomFactory.GetRandom();

        // Source text: 50/50 uniform crossover (standard for categorical genes)
        var childSourceTextId = random.NextBool() ? parentA.SourceTextId : parentB.SourceTextId;

        // Modifiers: existing single-point crossover logic
        var childModifiers = CrossoverModifiers(parentA.Modifiers, parentB.Modifiers, random);

        LogCrossover(_logger,
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
                    var index = random.NextItem(perturbableIndices);
                    var perturbable = (IPerturbable)mutated[index];
                    mutated[index] = perturbable.Perturb(random);
                }

                // If no perturbable modifiers, mutation has no effect (preserves genome)
                break;
        }

        LogModifierMutation(_logger, original.Count);

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
        var factory = random.NextItem(_modifierFactories);
        return factory.CreateRandom(random);
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
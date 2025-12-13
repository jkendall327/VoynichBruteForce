using System.Reflection;
using VoynichBruteForce.Sources.Asemic;

namespace VoynichBruteForce.Sources;

/// <summary>
/// Default implementation of <see cref="ISourceTextRegistry"/> that provides access
/// to all available source texts, both asemic generators and file-based languages.
/// Text content is lazily loaded and cached for performance.
/// </summary>
public class DefaultSourceTextRegistry : ISourceTextRegistry
{
    private readonly Dictionary<SourceTextId, Lazy<string>> _textCache;

    public IReadOnlyList<SourceTextId> AvailableIds { get; }

    public DefaultSourceTextRegistry(RandomFactory randomFactory)
    {
        AvailableIds = Enum.GetValues<SourceTextId>().ToList().AsReadOnly();

        _textCache = new Dictionary<SourceTextId, Lazy<string>>
        {
            // Asemic generators (existing providers)
            [SourceTextId.Random] = new(() => new RandomTextProvider(randomFactory.GetRandom()).GetText()),
            [SourceTextId.LullianCombinator] = new(() => new LullianCombinatorTextProvider().GetText()),
            [SourceTextId.ArithmeticSequence] = new(() => new ArithmeticSequenceTextProvider().GetText()),
            [SourceTextId.SyllableTable] = new(() => new SyllableTableTextProvider().GetText()),
            [SourceTextId.LoremIpsum] = new(() => new LoremIpsumTextProvider().GetText()),

            // Medieval European languages (file-based, embedded resources)
            [SourceTextId.Latin] = new(() => LoadEmbeddedResource("Latin.txt")),
            [SourceTextId.MedievalGerman] = new(() => LoadEmbeddedResource("MedievalGerman.txt")),
            [SourceTextId.OldFrench] = new(() => LoadEmbeddedResource("OldFrench.txt")),
            [SourceTextId.Italian] = new(() => LoadEmbeddedResource("Italian.txt")),
        };
    }

    public string GetText(SourceTextId id) => _textCache[id].Value;

    public SourceTextId GetRandomId(Random random)
        => random.NextItem(AvailableIds);

    private static string LoadEmbeddedResource(string fileName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"VoynichBruteForce.Sources.Texts.{fileName}";

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded resource not found: {resourceName}");
        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }
}

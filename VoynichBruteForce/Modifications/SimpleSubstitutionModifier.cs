namespace VoynichBruteForce.Modifications;

/// <summary>
/// Applies a simple monoalphabetic substitution cipher using an arbitrary
/// alphabet mapping. Each letter maps to exactly one other letter.
///
/// Simple substitution ciphers were widely used in diplomatic correspondence
/// throughout medieval and Renaissance Europe. The technique predates the
/// 15th century by many centuries (used by the Romans, Arabs, and others).
/// </summary>
public class SimpleSubstitutionModifier : ISpanTextModifier
{
    private readonly Dictionary<char, char> _substitutionMap;
    private readonly string _keyDescription;

    public string Name => $"SimpleSubstitution({_keyDescription})";

    // Moderate cognitive cost - requires memorization or reference to a table
    public CognitiveComplexity CognitiveCost => new(5);

    /// <summary>
    /// Creates a substitution cipher from a 26-character key alphabet.
    /// The key represents what each letter A-Z maps to.
    /// </summary>
    /// <param name="keyAlphabet">A 26-character string representing the cipher alphabet.</param>
    public SimpleSubstitutionModifier(string keyAlphabet)
    {
        if (keyAlphabet.Length != 26)
        {
            throw new ArgumentException("Key alphabet must be exactly 26 characters", nameof(keyAlphabet));
        }

        _keyDescription = keyAlphabet.Length > 10
            ? keyAlphabet[..10] + "..."
            : keyAlphabet;

        _substitutionMap = new Dictionary<char, char>();

        for (var i = 0; i < 26; i++)
        {
            var plainChar = (char)('A' + i);
            var cipherChar = char.ToUpper(keyAlphabet[i]);
            _substitutionMap[plainChar] = cipherChar;
            _substitutionMap[char.ToLower(plainChar)] = char.ToLower(cipherChar);
        }
    }

    /// <summary>
    /// Creates a substitution cipher from an explicit character mapping.
    /// </summary>
    public SimpleSubstitutionModifier(Dictionary<char, char> mapping)
    {
        _substitutionMap = new Dictionary<char, char>(mapping);
        _keyDescription = $"{mapping.Count} mappings";
    }

    public string ModifyText(string text) => this.RunWithContext(text);

    public void Modify(ref ProcessingContext context)
    {
        var input = context.InputSpan;
        var output = context.OutputSpan;

        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];
            output[i] = _substitutionMap.TryGetValue(c, out var mapped) ? mapped : c;
        }

        context.Commit(input.Length);
    }

    /// <summary>
    /// Creates a random substitution cipher using the provided seed.
    /// </summary>
    public static SimpleSubstitutionModifier CreateRandom(int seed)
    {
        var random = new Random(seed);
        var alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

        // Fisher-Yates shuffle
        for (var i = alphabet.Length - 1; i > 0; i--)
        {
            var j = random.Next(i + 1);
            (alphabet[i], alphabet[j]) = (alphabet[j], alphabet[i]);
        }

        return new SimpleSubstitutionModifier(new string(alphabet));
    }
}

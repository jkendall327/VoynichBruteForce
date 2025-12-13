namespace VoynichBruteForce.Modifications;

/// <summary>
/// Applies a homophonic substitution cipher where common letters can be
/// represented by multiple symbols, cycling through alternatives.
///
/// Homophonic ciphers were used to defeat frequency analysis by giving
/// common letters (like E, T, A) multiple possible substitutes. This
/// technique was documented in Renaissance cryptography manuals and
/// was used in diplomatic correspondence. The concept requires only
/// a lookup table - no special tools.
/// </summary>
public class HomophonicSubstitutionModifier : ISpanTextModifier
{
    private readonly Dictionary<char, string[]> _substitutes;
    private readonly int _seed;
    private readonly int _maxSubstituteLength;

    public string Name => $"HomophonicSubstitution(seed:{_seed})";

    // High cognitive cost - requires tracking which substitute was last used
    public CognitiveComplexity CognitiveCost => new(7);

    /// <summary>
    /// Creates a homophonic substitution cipher with multiple substitutes
    /// for common letters.
    /// </summary>
    /// <param name="seed">Seed for generating substitute mappings.</param>
    /// <param name="maxSubstitutes">Maximum number of substitutes for the most common letters.</param>
    public HomophonicSubstitutionModifier(int seed, int maxSubstitutes = 4)
    {
        _seed = seed;
        _substitutes = GenerateSubstitutes(seed, maxSubstitutes);
        _maxSubstituteLength = _substitutes.Values
            .SelectMany(arr => arr)
            .DefaultIfEmpty("")
            .Max(s => s.Length);
    }

    /// <summary>
    /// Creates a homophonic substitution cipher with explicit substitute mappings.
    /// </summary>
    public HomophonicSubstitutionModifier(Dictionary<char, string[]> substitutes)
    {
        _seed = 0;
        _substitutes = new Dictionary<char, string[]>(substitutes);
        _maxSubstituteLength = _substitutes.Values
            .SelectMany(arr => arr)
            .DefaultIfEmpty("")
            .Max(s => s.Length);
    }

    public string ModifyText(string text) => this.RunWithContext(text);

    public void Modify(ref ProcessingContext context)
    {
        var input = context.InputSpan;

        if (input.Length == 0)
        {
            return;
        }

        // Max expansion: each char could expand to _maxSubstituteLength
        // Use at least 1 to handle edge case of empty substitutes dictionary
        var maxExpansion = Math.Max(1, _maxSubstituteLength);
        context.EnsureCapacity(input.Length * maxExpansion);

        var output = context.OutputSpan;

        // Use local counters for thread-safety
        var counters = new Dictionary<char, int>();
        var writeIndex = 0;

        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];
            var upper = char.ToUpperInvariant(c);

            if (_substitutes.TryGetValue(upper, out var subs) && subs.Length > 0)
            {
                if (!counters.TryGetValue(upper, out var counter))
                {
                    counter = 0;
                }

                var substitute = subs[counter % subs.Length];

                if (substitute.Length == 1)
                {
                    // Single-char substitute - preserve case
                    var subChar = char.IsLower(c)
                        ? char.ToLowerInvariant(substitute[0])
                        : substitute[0];
                    output[writeIndex++] = subChar;
                }
                else
                {
                    // Multi-char substitute - copy directly
                    foreach (var sc in substitute)
                    {
                        output[writeIndex++] = sc;
                    }
                }

                counters[upper] = counter + 1;
            }
            else
            {
                output[writeIndex++] = c;
            }
        }

        context.Commit(writeIndex);
    }

    private static Dictionary<char, string[]> GenerateSubstitutes(int seed, int maxSubstitutes)
    {
        var random = new Random(seed);

        // Letter frequencies (approximate) determine how many substitutes each gets
        var frequencies = new Dictionary<char, double>
        {
            ['E'] = 12.7,
            ['T'] = 9.1,
            ['A'] = 8.2,
            ['O'] = 7.5,
            ['I'] = 7.0,
            ['N'] = 6.7,
            ['S'] = 6.3,
            ['H'] = 6.1,
            ['R'] = 6.0,
            ['D'] = 4.3,
            ['L'] = 4.0,
            ['C'] = 2.8,
            ['U'] = 2.8,
            ['M'] = 2.4,
            ['W'] = 2.4,
            ['F'] = 2.2,
            ['G'] = 2.0,
            ['Y'] = 2.0,
            ['P'] = 1.9,
            ['B'] = 1.5,
            ['V'] = 1.0,
            ['K'] = 0.8,
            ['J'] = 0.15,
            ['X'] = 0.15,
            ['Q'] = 0.10,
            ['Z'] = 0.07
        };

        var maxFreq = frequencies.Values.Max();
        var result = new Dictionary<char, string[]>();

        // Generate a pool of symbols to use as substitutes
        var symbolPool = new List<char>();
        for (var c = 'A'; c <= 'Z'; c++) symbolPool.Add(c);
        for (var c = '0'; c <= '9'; c++) symbolPool.Add(c);
        // Add some additional symbols that might appear in a cipher
        symbolPool.AddRange(['#', '@', '%', '&', '*', '+', '=']);

        // Shuffle the symbol pool
        for (var i = symbolPool.Count - 1; i > 0; i--)
        {
            var j = random.Next(i + 1);
            (symbolPool[i], symbolPool[j]) = (symbolPool[j], symbolPool[i]);
        }

        var symbolIndex = 0;

        foreach (var (letter, freq) in frequencies.OrderByDescending(kv => kv.Value))
        {
            // More frequent letters get more substitutes
            var numSubs = Math.Max(1, (int)Math.Ceiling(freq / maxFreq * maxSubstitutes));
            var subs = new string[numSubs];

            for (var i = 0; i < numSubs; i++)
            {
                if (symbolIndex < symbolPool.Count)
                {
                    subs[i] = symbolPool[symbolIndex++].ToString();
                }
                else
                {
                    // If we run out, use two-character combinations
                    subs[i] = random.NextItem(symbolPool).ToString() +
                              random.NextItem(symbolPool).ToString();
                }
            }

            result[letter] = subs;
        }

        return result;
    }
}

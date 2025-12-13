namespace VoynichBruteForce.Modifications;

public sealed class CaesarCipherModifierFactory : IModifierFactory
{
    public ITextModifier CreateRandom(Random random) => new CaesarCipherModifier(random.Next(26));
}

public sealed class AtbashCipherModifierFactory : IModifierFactory
{
    public ITextModifier CreateRandom(Random random) => new AtbashCipherModifier();
}

public sealed class VowelRemovalModifierFactory : IModifierFactory
{
    public ITextModifier CreateRandom(Random random) => new VowelRemovalModifier();
}

public sealed class PositionalExtractionModifierFactory : IModifierFactory
{
    public ITextModifier CreateRandom(Random random) => new PositionalExtractionModifier(random.Next(2, 5));
}

public sealed class NullInsertionModifierFactory : IModifierFactory
{
    public ITextModifier CreateRandom(Random random) =>
        new NullInsertionModifier(random.NextChar('a', 26), random.Next(3, 10));
}

public sealed class LetterDoublingModifierFactory : IModifierFactory
{
    public ITextModifier CreateRandom(Random random) => new LetterDoublingModifier();
}

public sealed class AnagramModifierFactory : IModifierFactory
{
    public ITextModifier CreateRandom(Random random) =>
        new AnagramModifier(
            random.RandomEnumMember<AnagramMode>(),
            random.Next(1000));
}

public sealed class AffixModifierFactory : IModifierFactory
{
    public ITextModifier CreateRandom(Random random)
    {
        var mode = random.RandomEnumMember<AffixMode>();

        return mode switch
        {
            AffixMode.AddPrefix or AffixMode.AddSuffix => new AffixModifier(mode, GenerateRandomString(random, 2, 4)),
            _ => new AffixModifier(mode)
        };
    }

    private static string GenerateRandomString(Random random, int minLength, int maxLength)
    {
        var length = random.Next(minLength, maxLength + 1);
        var chars = new char[length];

        for (var i = 0; i < length; i++)
        {
            chars[i] = random.NextChar('a', 26);
        }

        return new string(chars);
    }
}

public sealed class ConsonantVowelSplitModifierFactory : IModifierFactory
{
    public ITextModifier CreateRandom(Random random) => new ConsonantVowelSplitModifier();
}

public sealed class ColumnarTranspositionModifierFactory : IModifierFactory
{
    public ITextModifier CreateRandom(Random random) =>
        new ColumnarTranspositionModifier(GenerateRandomColumnOrder(random, random.Next(3, 7)));

    private static int[] GenerateRandomColumnOrder(Random random, int length)
    {
        var order = Enumerable.Range(0, length).ToArray();

        for (var i = order.Length - 1; i > 0; i--)
        {
            var j = random.Next(i + 1);
            (order[i], order[j]) = (order[j], order[i]);
        }

        return order;
    }
}

public sealed class SkipCipherModifierFactory : IModifierFactory
{
    public ITextModifier CreateRandom(Random random) => new SkipCipherModifier(random.Next(2, 5));
}

public sealed class InterleaveModifierFactory : IModifierFactory
{
    public ITextModifier CreateRandom(Random random) =>
        new InterleaveModifier(random.NextBool()
            ? InterleaveMode.HalvesAlternate
            : InterleaveMode.OddEvenSplit);
}

public sealed class WordReversalModifierFactory : IModifierFactory
{
    public ITextModifier CreateRandom(Random random) => new WordReversalModifier();
}

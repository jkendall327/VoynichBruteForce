namespace VoynichBruteForce;

public static class RandomExtensions
{
    public static T RandomEnumMember<T>(this Random random) where T : struct, Enum
    {
        var values = Enum.GetValues<T>();

        random.Shuffle(values);

        return values.First();
    }
}
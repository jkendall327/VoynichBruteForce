namespace VoynichBruteForce;

public static class RandomExtensions
{
    public static T RandomEnumMember<T>(this Random random) where T : struct, Enum
    {
        var values = Enum.GetValues<T>();

        random.Shuffle(values);

        return values.First();
    }

    /// <summary>
    /// Returns a random element from an array.
    /// </summary>
    public static T NextItem<T>(this Random random, T[] array)
    {
        return array[random.Next(array.Length)];
    }

    /// <summary>
    /// Returns a random element from a collection.
    /// </summary>
    public static T NextItem<T>(this Random random, IReadOnlyList<T> list)
    {
        return list[random.Next(list.Count)];
    }

    /// <summary>
    /// Returns a random boolean value.
    /// </summary>
    public static bool NextBool(this Random random)
    {
        return random.Next(2) == 0;
    }

    /// <summary>
    /// Returns a random character in the specified range.
    /// </summary>
    /// <param name="random">The random generator</param>
    /// <param name="start">The starting character (e.g., 'a')</param>
    /// <param name="count">The number of characters in the range (e.g., 26 for a-z)</param>
    /// <returns>A random character in the range [start, start+count)</returns>
    public static char NextChar(this Random random, char start, int count)
    {
        return (char)(start + random.Next(count));
    }
}
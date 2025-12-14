namespace VoynichBruteForce;

public static class RandomExtensions
{
    extension(Random random)
    {
        public T RandomEnumMember<T>() where T : struct, Enum
        {
            var values = Enum.GetValues<T>();

            random.Shuffle(values);

            return values.First();
        }

        /// <summary>
        /// Returns a random element from an array.
        /// </summary>
        public T NextItem<T>(T[] array)
        {
            return array[random.Next(array.Length)];
        }

        /// <summary>
        /// Returns a random element from a collection.
        /// </summary>
        public T NextItem<T>(IReadOnlyList<T> list)
        {
            return list[random.Next(list.Count)];
        }

        public T NextItem<T>(ReadOnlySpan<T> list)
        {
            return list[random.Next(list.Length)];
        }

        /// <summary>
        /// Returns a random boolean value.
        /// </summary>
        public bool NextBool()
        {
            return random.Next(2) == 0;
        }

        /// <summary>
        /// Returns a random character in the specified range.
        /// </summary>
        /// <param name="start">The starting character (e.g., 'a')</param>
        /// <param name="count">The number of characters in the range (e.g., 26 for a-z)</param>
        /// <returns>A random character in the range [start, start+count)</returns>
        public char NextChar(char start, int count)
        {
            return (char)(start + random.Next(count));
        }
    }
}
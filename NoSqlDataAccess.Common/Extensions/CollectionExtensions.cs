

using System;
using System.Collections.Generic;
using System.Linq;

namespace NoSqlDataAccess.Common.Extensions
{
    /// <summary>
    /// Extennsion methods for collections.
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Check if the given instance of <see cref="IEnumerable{T}"/> is null or empty.
        /// </summary>
        /// <param name="collection">An instance of <see cref="IEnumerable{T}"/>.</param>
        /// <returns>True if collection is null or empty. False otherwise.</returns>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> collection)
        {
            return collection == null || !collection.Any();
        }

        /// <summary>
        /// Add or update the dictionary, based on existence of key.
        /// </summary>
        /// <param name="dictionary">The input dictionary.</param>
        /// <param name="key">Key to add or update.</param>
        /// <param name="value">Value for the key.</param>
        public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary != null)
            {
                if (dictionary.ContainsKey(key))
                {
                    dictionary[key] = value;
                }
                else
                {
                    dictionary.Add(key, value);
                }
            }
        }

        /// <summary>
        /// Returns a value of the given key.
        /// When the key does not exist, this method suppresses the <see cref="KeyNotFoundException"/> and returns a given default or null value. Otherwise, the value is returned.
        /// </summary>
        /// <typeparam name="TKey">Generic key type.</typeparam>
        /// <typeparam name="TValue">Generic value type.</typeparam>
        /// <param name="dictionary">The input dictionary.</param>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">Optional default value.</param>
        /// <returns>The value if exists, null or given default otherwise.</returns>
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default(TValue))
        {
            TValue value = defaultValue;

            if (!dictionary.IsNullOrEmpty() && dictionary.ContainsKey(key))
            {
                value = dictionary[key];
            }

            return value;
        }

        /// <summary>
        /// Moves given number of items from the given list to a new list.
        /// </summary>
        /// <typeparam name="T">Generic item type.</typeparam>
        /// <param name="source">The source list.</param>
        /// <param name="chunkSize">Number of items to move.</param>
        /// <returns>A new list of given chunk size."/></returns>
        public static List<T> MoveChunkToNewList<T>(this List<T> source, int chunkSize)
        {
            List<T> newList = new List<T>();
            if(!source.IsNullOrEmpty())
            {
                var selected = source.Take(chunkSize).ToList();
                selected.ForEach(item => source.Remove(item));
                newList.AddRange(selected);
            }

            return newList;
        }

        /// <summary>
        /// Splits the provided list into the equal number of lists based on provided size
        /// </summary>
        /// <param name="items">List of items.</param>
        /// <param name="chunkSize">Chunk size"/>.</param>
        /// <returns>An instance of <see cref="IEnumerable{List}"/>.</returns>
        public static IEnumerable<List<T>> SplitList<T>(this List<T> items, int chunkSize = 50)
        {
            for (int i = 0; i < items.Count; i += chunkSize)
            {
                yield return items.GetRange(i, Math.Min(chunkSize, items.Count - i));
            }
        }
    }
}

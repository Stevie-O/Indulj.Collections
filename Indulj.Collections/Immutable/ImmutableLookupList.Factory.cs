using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Indulj.Collections.Immutable
{
    /// <summary>
    /// Provides a set of initialization methods for the <see cref="ImmutableLookupList{TKey, TValue}"/> class.
    /// </summary>
    public static class ImmutableLookupList
    {
        /// <summary>
        /// Creates an empty immutable lookup with the default key comparer.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys stored in the lookup.</typeparam>
        /// <typeparam name="TValue">The type of the values stored in the lookup.</typeparam>
        /// <returns></returns>
        public static ImmutableLookupList<TKey, TValue> Create<TKey, TValue>() => ImmutableLookupList<TKey, TValue>.Empty;

        /// <summary>
        /// Creates an empty immutable lookup with the specified key comparer.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys stored in the lookup.</typeparam>
        /// <typeparam name="TValue">The type of the values stored in the lookup.</typeparam>
        /// <param name="keyComparer">The implementation used to determine key equality.</param>
        /// <returns>An empty immutable lookup.</returns>
        public static ImmutableLookupList<TKey, TValue> Create<TKey, TValue>(IEqualityComparer<TKey> keyComparer)
        {
            return new ImmutableLookupList<TKey, TValue>(
                        ImmutableDictionary.Create<TKey, ImmutableList<TValue>>(keyComparer)
                    );
        }

        /// <summary>
        /// Creates an empty immutable lookup with the specified key and value comparers.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys stored in the lookup.</typeparam>
        /// <typeparam name="TValue">The type of the values stored in the lookup.</typeparam>
        /// <param name="keyComparer">The implementation used to determine key equality.</param>
        /// <param name="valueComparer">The implementation used to determine value equality.</param>
        /// <returns>An empty immutable lookup.</returns>
        public static ImmutableLookupList<TKey, TValue> Create<TKey, TValue>(IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
        {
            if (valueComparer == null) throw new ArgumentNullException(nameof(valueComparer));

            return new ImmutableLookupList<TKey, TValue>(
                        ImmutableDictionary.Create<TKey, ImmutableList<TValue>>(keyComparer),
                        valueComparer);
        }
    }

}

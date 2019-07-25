using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Collections.Immutable;
using System.Collections;

namespace Indulj.Collections.Immutable
{
    /// <summary>
    /// Represents an immutable, unordered collection of keys, each of which maps to a (possibly-empty) ordered list of values.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the collection.</typeparam>
    /// <typeparam name="TValue">The type of the values in the collection.</typeparam>
    public sealed class ImmutableLookupList<TKey, TValue> : ILookup<TKey, TValue>
    {
        readonly ImmutableDictionary<TKey, ImmutableList<TValue>> _dict;
        readonly IEqualityComparer<TValue> _valueComparer;

        internal ImmutableLookupList(ImmutableDictionary<TKey, ImmutableList<TValue>> dict)
            : this(dict, EqualityComparer<TValue>.Default)
        {
        }

        internal ImmutableLookupList(ImmutableDictionary<TKey, ImmutableList<TValue>> dict,
            IEqualityComparer<TValue> valueComparer)
        {
            _dict = dict;
            _valueComparer = valueComparer;
        }

        ImmutableLookupList<TKey, TValue> WithDict(ImmutableDictionary<TKey, ImmutableList<TValue>> newTable)
        {
            if (_dict == newTable) return this;
            return new ImmutableLookupList<TKey, TValue>(newTable, _valueComparer);
        }

        /// <summary>
        /// Gets an empty lookup with the default key and comparers.
        /// </summary>
        public static readonly ImmutableLookupList<TKey, TValue> Empty = new ImmutableLookupList<TKey, TValue>(ImmutableDictionary<TKey, ImmutableList<TValue>>.Empty);

        /// <summary>
        /// Gets the sequence of values indexed by the specified key.
        /// </summary>
        /// <param name="key">The key of the desired sequence of values.</param>
        /// <returns>An <see cref="ImmutableList{T}"/> sequence of values indexed by the specified key.</returns>
        /// <remarks>
        /// If the lookup table does not contain the specified key, an empty collection is returned; in particular, no exception is thrown.
        /// </remarks>
        public ImmutableList<TValue> this[TKey key]
        {
            get
            {
                if (_dict.TryGetValue(key, out var value))
                    return value;
                return ImmutableList<TValue>.Empty;
            }
        }

        /// <summary>
        /// Gets the number of keys with at least one associated value.
        /// </summary>
        public int Count => _dict.Count;

        IEnumerable<TValue> ILookup<TKey, TValue>.this[TKey key] => this[key];

        /// <summary>
        /// Gets whether or not a specified key has at least one associated value.
        /// </summary>
        /// <param name="key">The key to search for in the collection.</param>
        /// <returns><c>true</c> if the key has at least one associated value; <c>false</c> otherwise.</returns>
        public bool Contains(TKey key)
        {
            return _dict.ContainsKey(key);
        }

        /// <summary>
        /// Represents a collection of values that have a common key.
        /// </summary>
        public struct Grouping : IGrouping<TKey, TValue>
        {
            /// <summary>
            /// Gets the key of the collection of objects.
            /// </summary>
            public TKey Key { get; }
            /// <summary>
            /// Gets the collection of values associated with the key.
            /// </summary>
            public ImmutableList<TValue> Values { get; }

            internal Grouping(TKey key, ImmutableList<TValue> values)
            {
                Key = key;
                Values = values;
            }

            /// <summary>
            /// Returns an enumerator that iterates through the values in the collection.
            /// </summary>
            /// <returns></returns>
            public ImmutableList<TValue>.Enumerator GetEnumerator()
            {
                return Values.GetEnumerator();
            }

            IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator() => GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        /// <summary>
        /// Enumerates through an <see cref="ImmutableLookupList{TKey, TValue}"/> while avoiding heap allocations.
        /// </summary>
        public struct Enumerator : IEnumerator<Grouping>, IEnumerator<IGrouping<TKey, TValue>>
        {
            ImmutableDictionary<TKey, ImmutableList<TValue>>.Enumerator _internalEnumerator;
            internal Enumerator(ImmutableLookupList<TKey, TValue> lookup)
            {
                _internalEnumerator = lookup._dict.GetEnumerator();
            }

            /// <summary>
            /// Gets the element at the current position of the enumerator.
            /// </summary>
            public Grouping Current => new Grouping(_internalEnumerator.Current.Key, _internalEnumerator.Current.Value);

            IGrouping<TKey, TValue> IEnumerator<IGrouping<TKey, TValue>>.Current => Current;

            object IEnumerator.Current => Current;

            /// <summary>
            /// Releases the resources used associated with the enumerator.
            /// </summary>
            public void Dispose()
            {
                _internalEnumerator.Dispose();
            }

            /// <summary>
            /// Advances the enumerator to the next key in the lookup table.
            /// </summary>
            /// <returns><c>true</c> if the enumerator was successfully advanced to the next element;
            /// <c>false</c> if the enumerator has passed the end of the lookup table.</returns>
            public bool MoveNext()
            {
                return _internalEnumerator.MoveNext();
            }

            /// <summary>
            /// Sets the enumerator to its initial position, which is immediately before the first key in the lookup table.
            /// </summary>
            public void Reset()
            {
                _internalEnumerator.Reset();
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the keys in the lookup table.
        /// </summary>
        /// <returns></returns>
        public Enumerator GetEnumerator() => new Enumerator(this);

        IEnumerator<IGrouping<TKey, TValue>> IEnumerable<IGrouping<TKey, TValue>>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Adds the specified key/value pair to the collection, if it does not exist already.
        /// </summary>
        /// <param name="key">The key to add to the lookup.</param>
        /// <param name="value">The value to associate with the key.</param>
        /// <returns>A new immutable lookup that contains the additional key/value pair.</returns>
        /// <remarks>
        /// If the specified key/value pair already exists in the lookup, the existing instance of the lookup is returned.
        /// </remarks>
        public ImmutableLookupList<TKey, TValue> Ensure(TKey key, TValue value)
        {
            var list = this[key];
            if (list.Contains(value, _valueComparer))
                return this;
            return Add(key, value);
        }

        /// <summary>
        /// Adds the specified key/value pair to the lookup.
        /// </summary>
        /// <param name="key">The key to add to the lookup.</param>
        /// <param name="value">The value to associate with the key.</param>
        /// <returns>A new immutable lookup that contains the additional key/value pair.</returns>
        /// <remarks>
        /// If the specified key/value pair already exists in the dictionary, a duplicate entry will be added.
        /// </remarks>
        public ImmutableLookupList<TKey, TValue> Add(TKey key, TValue value)
        {
            var list = this[key];
            var newList = list.Add(value);
            return WithDict(_dict.SetItem(key, newList));
        }

        /// <summary>
        /// Adds a collection of key/value pairs to the lookup.
        /// </summary>
        /// <param name="groupings">The key and collection of values to add to the lookup.</param>
        /// <returns>A new immutable lookup that includes the specified key/value pairs.</returns>
        public ImmutableLookupList<TKey, TValue> AddRange(IGrouping<TKey, TValue> groupings)
        {
            return AddRange(groupings.Key, groupings);
        }

        /// <summary>
        /// Adds a collection of key/value pairs to the lookup.
        /// </summary>
        /// <param name="key">The key to add to the collection.</param>
        /// <param name="values">The sequence of values to add under the specified key.</param>
        /// <returns>A new immutable lookup that includes the specified key/value pairs.</returns>
        public ImmutableLookupList<TKey, TValue> AddRange(TKey key, IEnumerable<TValue> values)
        {
            var list = this[key];
            var newList = list.AddRange(values);
            if (newList == list) return this;
            return WithDict(_dict.SetItem(key, newList));
        }

        /// <summary>
        /// Adds a collection of key/value pairs to the lookup.
        /// </summary>
        /// <param name="groupings">The collection of key/value pairs to add to the lookup, grouped by key.</param>
        /// <returns>A new immutable lookup that includes the specified key/value pairs.</returns>
        public ImmutableLookupList<TKey, TValue> AddRange(IEnumerable<IGrouping<TKey, TValue>> groupings)
        {
            var newItems = new Dictionary<TKey, ImmutableList<TValue>>();
            foreach (var grouping in groupings)
            {
                if (!newItems.TryGetValue(grouping.Key, out var list))
                {
                    list = this[grouping.Key];
                }
                list = list.AddRange(grouping);
                if (list.Count > 0)
                    newItems[grouping.Key] = list;
            }
            if (newItems.Count == 0) return this;
            return WithDict(_dict.SetItems(newItems));
        }

        /// <summary>
        /// Remove all values from the specified key.
        /// </summary>
        /// <param name="key">The key for which all values should be removed.</param>
        /// <returns>A new immutable lookup that does not contain any values for the specified key.</returns>
        /// <remarks>
        /// If there are no values associated with the specified key, this method returns the existing instance.
        /// </remarks>
        public ImmutableLookupList<TKey, TValue> RemoveAll(TKey key)
        {
            if (!_dict.TryGetValue(key, out var list))
            {
                return this;
            }
            return WithDict(_dict.Remove(key));
        }

        /// <summary>
        /// Remove all occurrrences of the specified value from the specified key.
        /// </summary>
        /// <param name="key">The key for which the specified value should be removed.</param>
        /// <param name="value">THe value that should be removed.</param>
        /// <returns>A new immutable lookup that does not contain the specified value for the specified key.</returns>
        /// <remarks>
        /// If there the specified value is not  associated with the specified key, this method returns the existing instance.
        /// </remarks>
        public ImmutableLookupList<TKey, TValue> RemoveAll(TKey key, TValue value)
        {
            if (!_dict.TryGetValue(key, out var list))
            {
                return this;
            }
            var newList = list.RemoveAll(v => _valueComparer.Equals(v, value));
            if (newList == list)
                return this;
            return WithDict(_dict.SetItem(key, newList));
        }

        /// <summary>
        /// Remove the first occurrence of the specified value from the specified key.
        /// </summary>
        /// <param name="key">The key for which the specified value should be removed.</param>
        /// <param name="value">The value that should be removed.</param>
        /// <returns>A new immutable lookup, without the first instance of the specified value for the specified key.</returns>
        /// <remarks>
        /// If there the specified value is not associated with the specified key, this method returns the existing instance.
        /// </remarks>
        public ImmutableLookupList<TKey, TValue> Remove(TKey key, TValue value)
        {
            if (!_dict.TryGetValue(key, out var list))
            {
                return this;
            }
            var newList = list.Remove(value, _valueComparer);
            if (newList == list)
                return this;
            return WithDict(_dict.SetItem(key, newList));
        }
    }

}

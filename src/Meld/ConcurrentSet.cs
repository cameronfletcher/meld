// <copyright file="ConcurrentSet.cs" company="Meld contributors">
//  Copyright (c) Meld contributors. All rights reserved.
// </copyright>

namespace Meld
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    // LINK (Cameron): http://stackoverflow.com/questions/4306936/how-to-implement-concurrenthashset-in-net
    internal sealed class ConcurrentSet<T> : ISet<T>, ICollection<T>, IEnumerable<T>, IEnumerable
    {
        private readonly ConcurrentDictionary<T, byte> dictionary;
        private readonly IEqualityComparer<T> equalityComparer;

        public ConcurrentSet()
            : this(EqualityComparer<T>.Default)
        {
        }

        public ConcurrentSet(IEqualityComparer<T> equalityComparer)
        {
            Guard.Against.Null(() => equalityComparer);

            this.dictionary = new ConcurrentDictionary<T, byte>(equalityComparer);
            this.equalityComparer = equalityComparer;
        }

        public int Count
        {
            get { return this.dictionary.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Add(T item)
        {
            return this.TryAdd(item);
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            Guard.Against.Null(() => other);

            foreach (var item in other)
            {
                this.TryRemove(item);
            }
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            Guard.Against.Null(() => other);

            var enumerable = other as IList<T> ?? other.ToArray();
            foreach (var item in this)
            {
                if (!enumerable.Contains(item, this.equalityComparer))
                {
                    this.TryRemove(item);
                }
            }
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            Guard.Against.Null(() => other);

            var enumerable = other as IList<T> ?? other.ToArray();
            return this.Count != enumerable.Count && this.IsSubsetOf(enumerable);
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            Guard.Against.Null(() => other);

            var enumerable = other as IList<T> ?? other.ToArray();
            return this.Count != enumerable.Count && this.IsSupersetOf(enumerable);
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            Guard.Against.Null(() => other);

            var enumerable = other as IList<T> ?? other.ToArray();
            return this.AsParallel().All(item => enumerable.Contains(item, this.equalityComparer));
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            Guard.Against.Null(() => other);

            return other.AsParallel().All(this.Contains);
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            Guard.Against.Null(() => other);

            return other.AsParallel().Any(this.Contains);
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            Guard.Against.Null(() => other);

            var enumerable = other as IList<T> ?? other.ToArray();
            return this.Count == enumerable.Count && enumerable.AsParallel().All(this.Contains);
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public void UnionWith(IEnumerable<T> other)
        {
            Guard.Against.Null(() => other);

            foreach (var item in other)
            {
                this.TryAdd(item);
            }
        }

        void ICollection<T>.Add(T item)
        {
            if (!this.Add(item))
            {
                throw new ArgumentException("Item already exists in set.", "item");
            }
        }

        public void Clear()
        {
            this.dictionary.Clear();
        }

        public bool Contains(T item)
        {
            return this.dictionary.ContainsKey(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            this.dictionary.Keys.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return this.TryRemove(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this.dictionary.Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public T[] ToArray()
        {
            return this.dictionary.Keys.ToArray();
        }

        public bool TryAdd(T item)
        {
            return this.dictionary.TryAdd(item, default(byte));
        }

        public bool TryRemove(T item)
        {
            byte @byte;
            return this.dictionary.TryRemove(item, out @byte);
        }
    }
}

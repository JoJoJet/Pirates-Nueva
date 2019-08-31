using System;
using System.Collections;
using System.Collections.Generic;

namespace Pirates_Nueva
{
    /// <summary>
    /// An unordered set of two elements.
    /// </summary>
    public readonly struct Pair<T> : IEquatable<Pair<T>>, IEnumerable<T>
    {
        private static readonly int typeHash = typeof(T).GetHashCode();

        private readonly T a, b;

        /// <summary>
        /// Creates a new unordered set containing two elements.
        /// </summary>
        public Pair(T a, T b)
            => (this.a, this.b) = (a, b);

        #region Operators
        /// <summary>
        /// Returns the hash code for this <see cref="Pair{T}"/>.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
            => typeHash + (this.a?.GetHashCode() ?? 0) + (this.b?.GetHashCode() ?? 0);

        /// <summary>
        /// Returns whether or not this <see cref="Pair{T}"/> is equal to another.
        /// </summary>
        public bool Equals(Pair<T> other)
            => Eq(this.a, other.a) && Eq(this.b, other.b) || Eq(this.a, other.b) && Eq(this.b, other.a);
        /// <summary>
        /// Returns whether or not this <see cref="Pair{T}"/> is equal to the specified object.
        /// </summary>
        public override bool Equals(object obj)
            => obj is Pair<T> other && other.Equals(this);

        public static bool operator ==(Pair<T> a, Pair<T> b) => a.Equals(b);
        public static bool operator !=(Pair<T> a, Pair<T> b) => !a.Equals(b);

        private static bool Eq(T a, T b)
            => a?.Equals(b) ?? b is null;
        #endregion

        #region IEnumerable Implementation
        public IEnumerator<T> GetEnumerator() {
            yield return this.a;
            yield return this.b;
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion
    }
}

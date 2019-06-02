using System;
using System.Collections.Generic;
using System.Text;
#nullable enable

namespace Pirates_Nueva
{
    /// <summary>
    /// Represents a direction in 2D space.
    /// </summary>
    public readonly struct Vector : IEquatable<Vector>
    {
        /// <summary> X: 1, Y: 0 </summary>
        public static Vector Right { get; } = new Vector(1, 0);

        /// <summary> The X component of this vector. [0, 1]. </summary>
        public float X { get; }
        /// <summary> The Y component of this vector. [0, 1]. </summary>
        public float Y { get; }

        /// <summary>
        /// Gets the <see cref="Pirates_Nueva.Angle"/> between this instance and <see cref="Right"/>.
        /// </summary>
        public Angle Angle => MeasureAngle(Right, this);

        /// <summary>
        /// Creates a normalized <see cref="Vector"/> pointing in the specified direction.
        /// </summary>
        public Vector(float x, float y) {
            float mag = (float)Math.Sqrt(x*x + y*y);
            //
            // If the vector is longer than 1,
            // normalize it.
            if(mag > 0 && mag != 1) {
                x /= mag;  y /= mag;
            }

            X = x;  Y = y;
        }
        /// <summary>
        /// Creates a normalized <see cref="Vector"/> pointing between the specified <see cref="PointF"/>s.
        /// </summary>
        public Vector(PointF from, PointF to) : this(to.X - from.X, to.Y - from.Y) {  }

        /// <summary>
        /// Gets the <see cref="Pirates_Nueva.Angle"/> between the specified <see cref="Vector"/>s.
        /// </summary>
        public static Angle MeasureAngle(Vector a, Vector b)
            => (Angle)((float)Math.Atan2(b.Y, b.X) - (float)Math.Atan2(a.Y, a.X));

        public override bool Equals(object obj) => obj switch {
            Vector v           => Equals(v),
            (float x, float y) => x == X && y == Y,
            _                  => false
        };
        public bool Equals(Vector other) => other.X == X && other.Y == Y;
        public override int GetHashCode() => X.GetHashCode() + Y.GetHashCode() * 17;

        public static explicit operator Vector(PointF p) => new Vector(p.X, p.Y);
        public static implicit operator PointF(Vector v) => new PointF(v.X, v.Y);

        public static implicit operator Vector((float, float) tup) => new Vector(tup.Item1, tup.Item2);

        public static bool operator ==(Vector a, Vector b) => a.X == b.X && a.Y == b.Y;
        public static bool operator !=(Vector a, Vector b) => a.X != b.X || a.Y != b.Y;

        /// <summary> Returns the <see cref="Vector"/> with its magnitude being the specified scalar value. </summary>
        public static PointF operator *(Vector v, float scalar) => new PointF(v.X * scalar, v.Y * scalar);
    }
}

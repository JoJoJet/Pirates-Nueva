using System;

namespace Pirates_Nueva
{
    /// <summary>
    /// Represents a direction in 2D space.
    /// </summary>
    public readonly struct Vector : IEquatable<Vector>
    {
        private static readonly Vector up    = new Vector(0, 1),
                                       right = new Vector(1, 0),
                                       down  = new Vector(0, -1),
                                       left  = new Vector(-1, 1);

        /// <summary> X: 0, Y: 1 </summary>
        public static ref readonly Vector Up => ref up;
        /// <summary> X: 1, Y: 0 </summary>
        public static ref readonly Vector Right => ref right;
        /// <summary> X: 0, Y: -1 </summary>
        public static ref readonly Vector Down => ref down;
        /// <summary> X: -1, Y: 0 </summary>
        public static ref readonly Vector Left => ref left;

        /// <summary> The X component of this vector. [0, 1]. </summary>
        public float X { get; }
        /// <summary> The Y component of this vector. [0, 1]. </summary>
        public float Y { get; }

        /// <summary>
        /// Gets the <see cref="Pirates_Nueva.Angle"/> between this instance and <see cref="Right"/>.
        /// </summary>
        public Angle Angle => MeasureAngle(in Right, in this);

        /// <summary>
        /// Creates a normalized <see cref="Vector"/> pointing in the specified direction.
        /// </summary>
        public Vector(float x, float y) {
            float mag = MathF.Sqrt(x*x + y*y);
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
        public static Angle MeasureAngle(in Vector a, in Vector b)
            => (Angle)(MathF.Atan2(b.Y, b.X) - MathF.Atan2(a.Y, a.X));

        public override bool Equals(object? obj) => obj switch {
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

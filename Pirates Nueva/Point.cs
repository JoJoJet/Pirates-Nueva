using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Pirates_Nueva
{
    /// <summary>
    /// A point in 2D space, composed of 2 <see cref="int"/>s. Implicitly converts to and from MonoGame.Point.
    /// </summary>
    public struct PointI
    {
        public static PointI Zero => new PointI(0);

        public int X { get; set; }
        public int Y { get; set; }

        /// <summary>
        /// Gets the squared magnitude of this <see cref="PointI"/>. Faster than <see cref="Magnitude"/>.
        /// </summary>
        public int SqrMagnitude => X*X + Y*Y;
        /// <summary>
        /// Gets the magnitude (distance from origin) of this <see cref="PointI"/>.
        /// </summary>
        public float Magnitude => (float)Math.Sqrt(SqrMagnitude);

        public PointI(int value) : this(value, value) {  }
        public PointI(int x, int y) {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Returns the squared distance between two points. Faster than <see cref="Distance(PointI, PointI)"/>.
        /// </summary>
        public static int SqrDistance(PointI a, PointI b) {
            return sqr(a.X - b.X) + sqr(a.Y - b.Y);
            int sqr(int val) => val*val;
        }
        /// <summary>
        /// Returns the euclidean distance between two points.
        /// </summary>
        public static float Distance(PointI a, PointI b) => (float)Math.Sqrt(SqrDistance(a, b));

        public void Deconstruct(out int x, out int y) {
            x = X;
            y = Y;
        }

        public override string ToString() => $"({X}, {Y})";

        public override bool Equals(object obj) {
            switch(obj) {
                case PointI p: return p == this;
                case PointF f: return f.X == X && f.Y == Y;
                case ValueTuple<int, int> ti: return ti.Item1 == X && ti.Item2 == Y;
                case ValueTuple<float, float> tf: return tf.Item1 == X && tf.Item2 == Y;
                case Point m: return m.X == X && m.Y == Y;
                default: return false;
            }
        }
        public override int GetHashCode() => X.GetHashCode() + 9 * Y.GetHashCode();

        public static explicit operator PointI(PointF p) => new PointI((int)p.X, (int)p.Y);

        public static implicit operator Microsoft.Xna.Framework.Point(PointI p) => new Point(p.X, p.Y);
        public static implicit operator PointI(Microsoft.Xna.Framework.Point p) => new PointI(p.X, p.Y);
        
        public static implicit operator PointI((int, int) tup) => new PointI(tup.Item1, tup.Item2);

        public static PointI operator +(PointI a, PointI b) => new PointI(a.X + b.X, a.Y + b.Y);
        public static PointI operator -(PointI a, PointI b) => new PointI(a.X - b.X, a.Y - b.Y);
        public static PointI operator -(PointI p) => new PointI(-p.X, -p.Y);

        /// <summary> The dot product of two <see cref="PointI"/>s. </summary>
        public static int operator *(PointI a, PointI b) => a.X * b.X + a.Y * b.Y;

        public static PointI operator *(PointI p, int scalar) => new PointI(p.X * scalar, p.Y * scalar);
        public static PointI operator /(PointI p, int scalar) => new PointI(p.X / scalar, p.Y / scalar);

        public static bool operator ==(PointI a, PointI b) => a.X == b.X && a.Y == b.Y;
        public static bool operator !=(PointI a, PointI b) => a.X != b.X || a.Y != b.Y;
    }

    /// <summary>
    /// A point in 2D space, composed of 2 <see cref="float"/>s. Implicitly converts to and from MonoGame.Vector2.
    /// </summary>
    public struct PointF
    {
        public static PointF Zero => new PointF(0);

        public float X { get; set; }
        public float Y { get; set; }

        /// <summary>
        /// The squared magnitude of this point. Faster than <see cref="Magnitude"/>.
        /// </summary>
        public float SqrMagnitude => X*X + Y*Y;
        /// <summary>
        /// Returns the magnitude (distance from the origin) of this <see cref="PointF"/>.
        /// </summary>
        public float Magnitude => (float)Math.Sqrt(X*X + Y*Y);

        /// <summary>
        /// Returns this <see cref="PointF"/>, with a <see cref="Magnitude"/> of zero. Does not modify this instance.
        /// </summary>
        public PointF Normalized => SqrMagnitude > 0 ? this / Magnitude : Zero;

        public PointF(float value) : this(value, value) {  }
        public PointF(float x, float y) {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Returns the squared distance between two points. Faster than <see cref="Distance(PointF, PointF)"/>/
        /// </summary>
        public static float SqrDistance(PointF a, PointF b) {
            return sqr(a.X - b.X) + sqr(a.Y - b.Y);
            float sqr(float val) => val*val;
        }
        /// <summary>
        /// Returns the euclidean distance between two points.
        /// </summary>
        public static float Distance(PointF a, PointF b) => (float)Math.Sqrt(SqrDistance(a, b));

        /// <summary>
        /// Linearly interpolate between the specified points, by the specified factor.
        /// </summary>
        public static PointF Lerp(PointF first, PointF second, float factor) {
            return (l(first.X, second.X, factor), l(first.Y, second.Y, factor));

            float l(float a, float b, float f) => a * (1 - f) + b * f;
        }

        /// <summary>
        /// Rotate a <see cref="PointF"/> /p/ around the origin (0, 0) by angle /theta/.
        /// </summary>
        public static PointF Rotate(PointF p, Angle theta) {
            float sine = (float)Math.Sin(theta);
            float cosine = (float)Math.Cos(theta);

            return new PointF(p.X * cosine - p.Y * sine, p.X * sine + p.Y * cosine);
        }

        /// <summary>
        /// Get the angle between the input <see cref="PointF"/>s, assuming they are vectors.
        /// </summary>
        public static Angle Angle(PointF a, PointF b) => (Angle)((float)Math.Atan2(b.Y, b.X) - (float)Math.Atan2(a.Y, a.X));

        public void Deconstruct(out float x, out float y) {
            x = X;
            y = Y;
        }

        public override string ToString() => $"({X:.00}, {Y:.00})";

        public override bool Equals(object obj) {
            switch(obj) {
                case PointF p: return p == this;
                case PointI i: return i.X == X && i.Y == Y;
                case ValueTuple<float, float> tf: return tf.Item1 == X && tf.Item2 == Y;
                case ValueTuple<int, int> ti: return ti.Item1 == X && ti.Item2 == Y;
                case Vector2 v: return v.X == X && v.Y == Y;
                default: return false;
            }
        }
        public override int GetHashCode() => X.GetHashCode() + 6 * Y.GetHashCode();

        public static implicit operator PointF(PointI p) => new PointF(p.X, p.Y);

        public static implicit operator Vector2(PointF p) => new Vector2(p.X, p.Y);
        public static implicit operator PointF(Vector2 v) => new PointF(v.X, v.Y);

        public static implicit operator PointF((float, float) tup) => new PointF(tup.Item1, tup.Item2);

        public static PointF operator +(PointF a, PointF b) => new PointF(a.X + b.X, a.Y + b.Y);
        public static PointF operator -(PointF a, PointF b) => new PointF(a.X - b.X, a.Y - b.Y);
        public static PointF operator -(PointF p) => new PointF(-p.X, -p.Y);

        public static PointF operator +(PointF a, PointI b) => new PointF(a.X + b.X, a.Y + b.Y);
        public static PointF operator +(PointI a, PointF b) => new PointF(a.X + b.X, a.Y + b.Y);

        /// <summary> The dot product of two <see cref="PointF"/>s. </summary>
        public static float operator *(PointF a, PointF b) => a.X * b.X + a.Y * b.Y;

        public static PointF operator *(PointF p, float scalar) => new PointF(p.X * scalar, p.Y * scalar);
        public static PointF operator /(PointF p, float scalar) => new PointF(p.X / scalar, p.Y / scalar);

        public static bool operator ==(PointF a, PointF b) => a.X == b.X && a.Y == b.Y;
        public static bool operator !=(PointF a, PointF b) => a.X != b.X || a.Y != b.Y;
    }

    public static class PointExt
    {
        public static PointI ReadPointI(this System.Xml.XmlReader reader) {
            var fields = GetFields(reader);

            return (int.Parse(fields[0].Trim()), int.Parse(fields[1].Trim()));
        }

        public static PointF ReadPointF(this System.Xml.XmlReader reader) {
            var fields = GetFields(reader);

            return (float.Parse(fields[0].Trim()), float.Parse(fields[1].Trim()));
        }

        static string[] GetFields(System.Xml.XmlReader reader) {
            return reader.ReadElementContentAsString().Split(new char[] { ',' }, 2, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}

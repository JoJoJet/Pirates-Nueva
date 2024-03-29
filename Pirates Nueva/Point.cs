﻿using System;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;

namespace Pirates_Nueva
{
    /// <summary>
    /// A point in 2D space, composed of 2 <see cref="int"/>s. Implicitly converts to and from <see cref="Point"/>
    /// </summary>
    public readonly struct PointI : IEquatable<PointI>
    {
        private static readonly PointI zero = new PointI(0);

        public static ref readonly PointI Zero => ref zero;

        public int X { get; }
        public int Y { get; }

        /// <summary>
        /// Gets the squared magnitude of this <see cref="PointI"/>. Faster than <see cref="Magnitude"/>.
        /// </summary>
        public int SqrMagnitude => X*X + Y*Y;
        /// <summary>
        /// Gets the magnitude (distance from origin) of this <see cref="PointI"/>.
        /// </summary>
        public float Magnitude => MathF.Sqrt(SqrMagnitude);

        public PointI(int value) : this(value, value) {  }
        public PointI(int x, int y) {
            X = x;   Y = y;
        }

        /// <summary>
        /// Returns the squared distance between two points. Faster than <see cref="Distance(in PointI, in PointI)"/>.
        /// </summary>
        public static int SqrDistance(in PointI a, in PointI b) {
            return sqr(a.X - b.X) + sqr(a.Y - b.Y);
            static int sqr(int val) => val*val;
        }
        /// <summary>
        /// Returns the euclidean distance between two points.
        /// </summary>
        public static float Distance(in PointI a, in PointI b) => MathF.Sqrt(SqrDistance(in a, in b));

        /// <summary>
        /// Returns a copy of the current <see cref="PointI"/> with the specified properties modified.
        /// </summary>
        public PointI With(int? X = null, int? Y = null)
            => new PointI(X ?? this.X, Y ?? this.Y);

        public void Deconstruct(out int x, out int y) {
            x = X;   y = Y;
        }

        public override string ToString() => $"({X}, {Y})";

        public override bool Equals(object? obj) => obj switch {
            PointI p       => Equals(p),
            (int x, int y) => x == X && y == Y,
            _              => false
        };
        public bool Equals(PointI other) => other.X == X && other.Y == Y;
        public override int GetHashCode() => X.GetHashCode() + 9 * Y.GetHashCode();

        public static explicit operator PointI(in PointF p) => new PointI((int)p.X, (int)p.Y);

        public static implicit operator Microsoft.Xna.Framework.Point(in PointI p) => new Point(p.X, p.Y);
        public static implicit operator PointI(in Microsoft.Xna.Framework.Point p) => new PointI(p.X, p.Y);
        
        public static implicit operator PointI(in (int, int) tup) => new PointI(tup.Item1, tup.Item2);

        public static PointI operator +(in PointI a, in PointI b) => new PointI(a.X + b.X, a.Y + b.Y);
        public static PointI operator -(in PointI a, in PointI b) => new PointI(a.X - b.X, a.Y - b.Y);
        public static PointI operator -(in PointI p) => new PointI(-p.X, -p.Y);

        /// <summary> The dot product of two <see cref="PointI"/>s. </summary>
        public static int operator *(in PointI a, in PointI b) => a.X * b.X + a.Y * b.Y;

        public static PointI operator *(in PointI p, in int scalar) => new PointI(p.X * scalar, p.Y * scalar);
        public static PointI operator /(in PointI p, in int scalar) => new PointI(p.X / scalar, p.Y / scalar);

        public static bool operator ==(in PointI a, in PointI b) => a.X == b.X && a.Y == b.Y;
        public static bool operator !=(in PointI a, in PointI b) => a.X != b.X || a.Y != b.Y;
    }

    /// <summary>
    /// A point in 2D space, composed of 2 <see cref="float"/>s. Implicitly converts to and from <see cref="Vector2"/>
    /// </summary>
    public readonly struct PointF : IEquatable<PointF>
    {
        private static readonly PointF zero = new PointF(0);

        public static ref readonly PointF Zero => ref zero;

        public float X { get; }
        public float Y { get; }

        /// <summary>
        /// The squared magnitude of this point. Faster than <see cref="Magnitude"/>.
        /// </summary>
        public float SqrMagnitude => X*X + Y*Y;
        /// <summary>
        /// Returns the magnitude (distance from the origin) of this <see cref="PointF"/>.
        /// </summary>
        public float Magnitude => MathF.Sqrt(X*X + Y*Y);

        /// <summary>
        /// Returns this <see cref="PointF"/>, with a <see cref="Magnitude"/> of zero. Does not modify this instance.
        /// </summary>
        public PointF Normalized => SqrMagnitude > 0 ? this / Magnitude : Zero;

        public PointF(float value) : this(value, value) {  }
        public PointF(float x, float y) {
            X = x;   Y = y;
        }

        /// <summary>
        /// Returns the squared distance between two points. Faster than <see cref="Distance(PointF, PointF)"/>.
        /// </summary>
        public static float SqrDistance(in PointF a, in PointF b) {
            return sqr(a.X - b.X) + sqr(a.Y - b.Y);
            static float sqr(float val) => val*val;
        }
        /// <summary>
        /// Returns the euclidean distance between two points.
        /// </summary>
        public static float Distance(in PointF a, in PointF b) => MathF.Sqrt(SqrDistance(in a, in b));

        /// <summary>
        /// Linearly interpolate between the specified points, by the specified factor.
        /// </summary>
        public static PointF Lerp(in PointF first, in PointF second, float factor) {
            return (l(first.X, second.X, factor), l(first.Y, second.Y, factor));

            static float l(float a, float b, float f) => a * (1 - f) + b * f;
        }

        /// <summary>
        /// Rotate a <see cref="PointF"/> /p/ around the origin (0, 0) by angle /theta/.
        /// </summary>
        public static PointF Rotate(in PointF p, in Angle theta) {
            float sine = MathF.Sin(theta);
            float cosine = MathF.Cos(theta);

            return new PointF(p.X * cosine - p.Y * sine, p.X * sine + p.Y * cosine);
        }

        /// <summary>
        /// Returns a copy of the current <see cref="PointF"/> with the specifed properties modified.
        /// </summary>
        public PointF With(float? X = null, float? Y = null)
            => new PointF(X ?? this.X, Y ?? this.Y);

        public void Deconstruct(out float x, out float y) {
            x = X;   y = Y;
        }

        public override string ToString() => $"({X:.00}, {Y:.00})";

        public override bool Equals(object? obj) => obj switch {
            PointF p           => Equals(p),
            (float x, float y) => x == X && y == Y,
            _                  => false
        };
        public bool Equals(PointF other) => other.X == X && other.Y == Y;
        public override int GetHashCode() => X.GetHashCode() + 6 * Y.GetHashCode();

        public static implicit operator PointF(in PointI p) => new PointF(p.X, p.Y);

        public static implicit operator Vector2(in PointF p) => new Vector2(p.X, p.Y);
        public static implicit operator PointF(in Vector2 v) => new PointF(v.X, v.Y);

        public static implicit operator PointF(in (float, float) tup) => new PointF(tup.Item1, tup.Item2);

        public static PointF operator +(in PointF a, in PointF b) => new PointF(a.X + b.X, a.Y + b.Y);
        public static PointF operator -(in PointF a, in PointF b) => new PointF(a.X - b.X, a.Y - b.Y);
        public static PointF operator -(in PointF p) => new PointF(-p.X, -p.Y);

        public static PointF operator +(in PointF a, in PointI b) => new PointF(a.X + b.X, a.Y + b.Y);
        public static PointF operator +(in PointI a, in PointF b) => new PointF(a.X + b.X, a.Y + b.Y);

        /// <summary> The dot product of two <see cref="PointF"/>s. </summary>
        public static float operator *(in PointF a, in PointF b) => a.X * b.X + a.Y * b.Y;

        public static PointF operator *(in PointF p, in float scalar) => new PointF(p.X * scalar, p.Y * scalar);
        public static PointF operator /(in PointF p, in float scalar) => new PointF(p.X / scalar, p.Y / scalar);

        public static bool operator ==(in PointF a, in PointF b) => a.X == b.X && a.Y == b.Y;
        public static bool operator !=(in PointF a, in PointF b) => a.X != b.X || a.Y != b.Y;
    }

    public static class PointExt
    {
        static readonly Regex intMatcher = new Regex(@"(\d+), *(\d+)", RegexOptions.Compiled);
        static readonly Regex floatMatcher = new Regex(@"(\d+\.?\d*), *(\d+\.?\d*)", RegexOptions.Compiled);

        public static PointI ReadPointI(this System.Xml.XmlReader reader) {
            var match = intMatcher.Match(reader.ReadElementString());
            return new PointI(parse(1), parse(2));

            int parse(int group) => int.Parse(match.Groups[group].Value);
        }

        public static PointF ReadPointF(this System.Xml.XmlReader reader) {
            var match = floatMatcher.Match(reader.ReadElementString());
            return new PointF(parse(1), parse(2));

            float parse(int group) => float.Parse(match.Groups[group].Value);
        }
    }
}

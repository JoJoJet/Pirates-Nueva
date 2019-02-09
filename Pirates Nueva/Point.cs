﻿using System;
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

        public PointI(int value) : this(value, value) {  }
        public PointI(int x, int y) {
            X = x;
            Y = y;
        }

        public void Deconstruct(out int x, out int y) {
            x = X;
            y = Y;
        }

        public override string ToString() => $"({X}, {Y})";

        public static explicit operator PointI(PointF p) => new PointI((int)Math.Round(p.X), (int)Math.Round(p.Y));

        public static implicit operator Microsoft.Xna.Framework.Point(PointI p) => new Point(p.X, p.Y);
        public static implicit operator PointI(Microsoft.Xna.Framework.Point p) => new PointI(p.X, p.Y);
        
        public static implicit operator PointI((int, int) tup) => new PointI(tup.Item1, tup.Item2);

        public static PointI operator +(PointI a, PointI b) => new PointI(a.X + b.X, a.Y + b.Y);
        public static PointI operator -(PointI a, PointI b) => new PointI(a.X - b.X, a.Y - b.Y);
        public static PointI operator -(PointI p) => new PointI(-p.X, -p.Y);
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
        /// Returns the magnitude (distance from the origin) of this <see cref="PointF"/>.
        /// </summary>
        public float Magnitude => (float)Math.Sqrt(X*X + Y*Y);

        /// <summary>
        /// Returns this <see cref="PointF"/>, with a <see cref="Magnitude"/> of zero. Does not modify this instance.
        /// </summary>
        public PointF Normalized => Magnitude > 0 ? this / Magnitude : Zero;

        public PointF(float value) : this(value, value) {  }
        public PointF(float x, float y) {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Rotate a <see cref="PointF"/> /p/ around the origin (0, 0) by angle /theta/ (in radians).
        /// </summary>
        public static PointF Rotate(PointF p, float theta) {
            float sine = (float)Math.Sin(theta);
            float cosine = (float)Math.Cos(theta);

            return new PointF(p.X * cosine - p.Y * sine, p.X * sine + p.Y * cosine);
        }

        public void Deconstruct(out float x, out float y) {
            x = X;
            y = Y;
        }

        public override string ToString() => $"({X:.00}, {Y:.00})";

        public static explicit operator PointF(PointI p) => new PointF(p.X, p.Y);

        public static implicit operator Vector2(PointF p) => new Vector2(p.X, p.Y);
        public static implicit operator PointF(Vector2 v) => new PointF(v.X, v.Y);

        public static implicit operator PointF((float, float) tup) => new PointF(tup.Item1, tup.Item2);

        public static PointF operator +(PointF a, PointF b) => new PointF(a.X + b.X, a.Y + b.Y);
        public static PointF operator -(PointF a, PointF b) => new PointF(a.X - b.X, a.Y - b.Y);
        public static PointF operator -(PointF p) => new PointF(-p.X, -p.Y);
        public static PointF operator *(PointF p, float scalar) => new PointF(p.X * scalar, p.Y * scalar);
        public static PointF operator /(PointF p, float scalar) => new PointF(p.X / scalar, p.Y / scalar);
    }
}

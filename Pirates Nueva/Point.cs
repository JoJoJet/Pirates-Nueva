using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Pirates_Nueva
{
    /// <summary>
    /// A point in 2D space, composed of 2 <see cref="int"/>s.
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

        public static implicit operator Microsoft.Xna.Framework.Point(PointI p) => new Point(p.X, p.Y);
        public static implicit operator PointI(Microsoft.Xna.Framework.Point p) => new PointI(p.X, p.Y);
    }

    /// <summary>
    /// A point in 2D space, composed of 2 <see cref="float"/>s.
    /// </summary>
    public struct PointF
    {
        public static PointF Zero => new PointF(0);

        public float X { get; set; }
        public float Y { get; set; }

        public PointF(float value) : this(value, value) {  }
        public PointF(float x, float y) {
            X = x;
            Y = y;
        }

        public void Deconstruct(out float x, out float y) {
            x = X;
            y = Y;
        }

        public override string ToString() => $"({X:.00}, {Y:.00})";

        public static implicit operator Vector2(PointF p) => new Vector2(p.X, p.Y);
        public static implicit operator PointF(Vector2 v) => new PointF(v.X, v.Y);
    }
}

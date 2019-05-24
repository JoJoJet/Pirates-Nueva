using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoColor = Microsoft.Xna.Framework.Color;

namespace Pirates_Nueva.UI
{
    /// <summary>
    /// An RGBA color.
    /// </summary>
    public readonly struct Color : IEquatable<Color>
    {
        /// <summary> R: 0, G: 0, B: 0, A: 0 </summary>
        public static Color Empty { get; } = new Color();
        /// <summary> R: 0, G: 0, B: 0 </summary>
        public static Color Black { get; } = new Color(0, 0, 0);
        /// <summary> R: 255, G: 255, B: 255 </summary>
        public static Color White { get; } = new Color(255, 255, 255);
        /// <summary> R: 0, G: 255, B: 0 </summary>
        public static Color Lime { get; } = new Color(0, 255, 0);
        /// <summary> R: 0, G: 155, B: 0 </summary>
        public static Color DarkLime { get; } = new Color(0, 155, 0);
        /// <summary> R: 255, G: 255, B: 153 </summary>
        public static Color PaleYellow { get; } = new Color(255, 255, 153);

        /// <summary> The red component of this <see cref="Color"/>. </summary>
        public byte R { get; }
        /// <summary> The green component of this <see cref="Color"/>. </summary>
        public byte G { get; }
        /// <summary> The blue component of this <see cref="Color"/>. </summary>
        public byte B { get; }
        /// <summary> The alpha component of this <see cref="Color"/>. </summary>
        public byte A { get; }

        /// <summary>
        /// Constructs an RGBA color with specified channels, defined with <see cref="byte"/>s.
        /// </summary>
        /// <param name="r">The red component, 0 to 255.</param>
        /// <param name="g">The green component, 0 to 255.</param>
        /// <param name="b">The blue component, 0 to 255.</param>
        /// <param name="a">The alpha component, 0 to 255.</param>
        public Color(byte r, byte g, byte b, byte a = 255) {
            R = r;    G = g;
            B = b;    A = a;
        }

        /// <summary>
        /// Constructs an RGBA color with specified channels, defined with <see cref="float"/>s.
        /// </summary>
        /// <param name="r">The red component, 0.0f to 1.0f.</param>
        /// <param name="g">The green component, 0.0f to 1.0f.</param>
        /// <param name="b">The blue component, 0.0f to 1.0f.</param>
        /// <param name="a">The alpha component, 0.0f to 1.0f.</param>
        public Color(float r, float g, float b, float a = 1f) {
            R = c(r);    G = c(g);
            B = c(b);    A = c(a);

            byte c(float f) => (byte)((f > 1f ? 1f : (f < 0 ? 0 : f)) * 255);
        }

        public override bool Equals(object obj) {
            switch(obj) {
                case Color c:     return Equals(c);
                case MonoColor m: return R == m.R && G == m.G && B == m.B && A == m.A;
                default:          return false;
            }
        }
        public bool Equals(Color other) => R == other.R && G == other.G && B == other.B && A == other.A;
        public override int GetHashCode() => A.GetHashCode() + G.GetHashCode() * 3 + B.GetHashCode() * 7 + A.GetHashCode() * 14;

        public static explicit operator Color(MonoColor mono) => new Color(mono.R, mono.G, mono.B, mono.A);
        public static implicit operator MonoColor(Color col) => new MonoColor(col.R, col.G, col.B, col.A);

        public static bool operator ==(Color a, Color b) => a.R == b.R && a.G == b.G && a.B == b.B && a.A == b.A;
        public static bool operator !=(Color a, Color b) => a.R != b.R || a.G != b.G || a.B != b.B || a.A != b.A;
    }
}

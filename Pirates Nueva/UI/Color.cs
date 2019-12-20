using System;
using MonoColor = Microsoft.Xna.Framework.Color;

namespace Pirates_Nueva.UI
{
    /// <summary>
    /// An RGBA color.
    /// </summary>
    public readonly struct Color : IEquatable<Color>
    {
        private static Color clear = new Color(),
                             black = new Color(0, 0, 0),
                             white = new Color(255, 255, 255),
                             lime = new Color(0, 255, 0),
                             darkLime = new Color(0, 155, 0),
                             paleYellow = new Color(255, 255, 153);

        /// <summary> R: 0, G: 0, B: 0, A: 0 </summary>
        public static ref readonly Color Clear => ref clear;
        /// <summary> R: 0, G: 0, B: 0 </summary>
        public static ref readonly Color Black => ref black;
        /// <summary> R: 255, G: 255, B: 255 </summary>
        public static ref readonly Color White => ref white;
        /// <summary> R: 0, G: 255, B: 0 </summary>
        public static ref readonly Color Lime => ref lime;
        /// <summary> R: 0, G: 155, B: 0 </summary>
        public static ref readonly Color DarkLime => ref darkLime;
        /// <summary> R: 255, G: 255, B: 153 </summary>
        public static ref readonly Color PaleYellow => ref paleYellow;

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

            static byte c(float f) => (byte)((f > 1f ? 1f : (f < 0 ? 0 : f)) * 255);
        }

        public override bool Equals(object? obj) => obj switch {
            Color c     => Equals(c),
            MonoColor m => m.R == R && m.G == G && m.B == B && m.A == A,
            _           => false
        };
        public bool Equals(Color other) => R == other.R && G == other.G && B == other.B && A == other.A;
        public override int GetHashCode() => A.GetHashCode() + G.GetHashCode() * 3 + B.GetHashCode() * 7 + A.GetHashCode() * 14;

        public static explicit operator Color(MonoColor mono) => new Color(mono.R, mono.G, mono.B, mono.A);
        public static implicit operator MonoColor(Color col) => new MonoColor(col.R, col.G, col.B, col.A);

        public static bool operator ==(Color a, Color b) => a.R == b.R && a.G == b.G && a.B == b.B && a.A == b.A;
        public static bool operator !=(Color a, Color b) => a.R != b.R || a.G != b.G || a.B != b.B || a.A != b.A;
    }
}

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
    public struct Color : IEquatable<Color>
    {
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
        public Color(byte r, byte g, byte b, byte a) {
            R = r;    G = g;
            B = b;    A = a;
        }
        /// <summary>
        /// Constructs an RGB color with specified channels, defined with <see cref="byte"/>s.
        /// </summary>
        /// <param name="r">The red component, 0 to 255.</param>
        /// <param name="g">The green component, 0 to 255.</param>
        /// <param name="b">The blue component, 0 to 255.</param>
        public Color(byte r, byte g, byte b) : this(r, g, b, 255) {  }

        /// <summary>
        /// Constructs an RGBA color with specified channels, defined with <see cref="float"/>s.
        /// </summary>
        /// <param name="r">The red component, 0.0f to 1.0f.</param>
        /// <param name="g">The green component, 0.0f to 1.0f.</param>
        /// <param name="b">The blue component, 0.0f to 1.0f.</param>
        /// <param name="a">The alpha component, 0.0f to 1.0f.</param>
        public Color(float r, float g, float b, float a) {
            R = c(r);
            G = c(g);
            B = c(b);
            A = c(a);

            byte c(float f) => (byte)((f > 1f ? 1f : (f < 0 ? 0 : f)) * 255);
        }
        /// <summary>
        /// Constructs an RGB color with specified channels, defined with <see cref="float"/>s.
        /// </summary>
        /// <param name="r">The red component, 0.0f to 1.0f.</param>
        /// <param name="g">The green component, 0.0f to 1.0f.</param>
        /// <param name="b">The blue component, 0.0f to 1.0f.</param>
        public Color(float r, float g, float b) : this(r, g, b, 1f) {  }

        bool IEquatable<Color>.Equals(Color other) => R == other.R && G == other.G && B == other.B && A == other.A;

        public static explicit operator Color(MonoColor mono) => new Color(mono.R, mono.G, mono.B, mono.A);
        public static implicit operator MonoColor(Color col) => new MonoColor(col.R, col.G, col.B, col.A);
    }
}

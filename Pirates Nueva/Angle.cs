using System;

namespace Pirates_Nueva
{
    /// <summary>
    /// Stores an angle, allowing access in both Radians and Degrees. Is always positive, and automatically wraps.
    /// </summary>
    public struct Angle
    {
        private const float Rad2Deg = 180f / (float)Math.PI;
        private const float Deg2Rad = (float)Math.PI / 180f;

        private float _radians;

        /// <summary> The value of this <see cref="Angle"/>, in radians. </summary>
        public float Radians {
            get => _radians;
            private set {
                this._radians = value;
                this._radians %= (float)Math.PI;
                if(this._radians < 0)
                    this._radians = 360 + this._radians;
            }
        }
        /// <summary> The value of this <see cref="Angle"/>, in degrees. </summary>
        public float Degrees => Radians * Rad2Deg;

        /// <summary> Create a new <see cref="Angle"/> struct, from a number in radians. </summary>
        public static Angle FromRadians(float rads) => new Angle() { Radians = rads };
        /// <summary> Create a new <see cref="Angle"/> struct, from a number in degrees. </summary>
        public static Angle FromDegrees(float degs) => new Angle() { Radians = degs * Deg2Rad };

        public override string ToString() => $"{Radians/Math.PI:.00}π";

        public static implicit operator float(Angle ang) => ang.Radians;
    }
}

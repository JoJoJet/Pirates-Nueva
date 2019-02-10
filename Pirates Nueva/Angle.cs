﻿using System;

namespace Pirates_Nueva
{
    /// <summary>
    /// Stores an angle, allowing access in both Radians and Degrees. Is always positive, and automatically wraps. Immutable.
    /// <para />
    /// Range: [0, 2π) radians, [0, 360°)
    /// </summary>
    public struct Angle
    {
        /// <summary> A half rotation around a circle. </summary>
        private const float HalfTurn = (float)Math.PI;
        /// <summary> A full rotation around a circle. </summary>
        private const float FullTurn = 2*HalfTurn;
        /// <summary> Converts radians to degrees. </summary>
        private const float Rad2Deg  = 180f / HalfTurn;
        /// <summary> Converts degrees to radians. </summary>
        private const float Deg2Rad  = HalfTurn / 180f;

        private float _radians;

        /// <summary> The value of this <see cref="Angle"/>, in radians. Range: [0, 2π) </summary>
        public float Radians {
            get => _radians;
            private set {
                this._radians = value;
                this._radians %= FullTurn;
                if(this._radians < 0)
                    this._radians += FullTurn;
            }
        }
        /// <summary> The value of this <see cref="Angle"/>, in degrees. Range: [0, 360°) </summary>
        public float Degrees => Radians * Rad2Deg;

        /// <summary> Create a new <see cref="Angle"/> struct, from a number in radians. </summary>
        public static Angle FromRadians(float rads) => new Angle() { Radians = rads };
        /// <summary> Create a new <see cref="Angle"/> struct, from a number in degrees. </summary>
        public static Angle FromDegrees(float degs) => new Angle() { Radians = degs * Deg2Rad };

        /// <summary>
        /// Return an angle, moving from /a/ towards /b/, with a maximum change of /step/.
        /// </summary>
        public static Angle MoveTowards(Angle a, Angle b, float step) {
            float difference = b - a; // Get the difference between the two angles.

            if(abs(difference) > HalfTurn) // If the difference is greater than a half turn, normalize it.
                difference = HalfTurn - difference;

            if(abs(difference) > step) // Make sure that the magnitude of /difference/ is no larger than /step/.
                difference = step * sign(difference);

            return FromRadians(a + difference); // Add /difference/ to the first angle.

            float abs(float f) => Math.Abs(f); float sign(float f) => Math.Sign(f);
        }

        public override string ToString() => Radians != 0 ? $"{Radians/Math.PI:0.##}π" : "0";

        public static explicit operator Angle(float rads) => FromRadians(rads);
        public static implicit operator float(Angle ang) => ang.Radians;

        public static Angle operator -(Angle ang) => FromRadians(-ang.Radians);
    }
}
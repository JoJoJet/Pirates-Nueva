using System;

namespace Pirates_Nueva
{
    /// <summary>
    /// Stores an angle, allowing access in both Radians and Degrees. Is always positive, and automatically wraps.
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

        /// <summary> The value of this <see cref="Angle"/>, in radians. </summary>
        public float Radians {
            get => _radians;
            private set {
                this._radians = value;
                this._radians %= HalfTurn;
                if(this._radians < 0)
                    this._radians += FullTurn;
            }
        }
        /// <summary> The value of this <see cref="Angle"/>, in degrees. </summary>
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

            if(difference > HalfTurn) // If the difference is greater than a half turn, normalize it.
                difference = HalfTurn - difference;

            if(Math.Abs(difference) > step) // Make sure that the magnitude of /difference/ is no larger than /step/.
                difference = step * Math.Sign(difference);

            return FromRadians(a + difference); // Add /difference/ to the first angle.
        }

        public override string ToString() => $"{Radians/Math.PI:.00}π";

        public static implicit operator float(Angle ang) => ang.Radians;
    }
}

using System;

namespace Pirates_Nueva
{
    /// <summary>
    /// Stores an angle, allowing access in both Radians and Degrees. Is always positive, and automatically wraps.
    /// <para />
    /// Range: [0, 2π) radians, [0, 360°)
    /// </summary>
    public readonly struct Angle
    {
        /// <summary> A half rotation around a circle, in radians. </summary>
        public const float HalfTurn = (float)Math.PI;
        /// <summary> A full rotation around a circle, in radians. </summary>
        private const float FullTurn = 2*HalfTurn;
        /// <summary> Converts radians to degrees. </summary>
        public const float Rad2Deg  = 180f / HalfTurn;
        /// <summary> Converts degrees to radians. </summary>
        public const float Deg2Rad  = HalfTurn / 180f;

        private Angle(float radians) {
            Radians = radians % FullTurn;
            if(Radians < 0)
                Radians += FullTurn;
        }

        /// <summary> The value of this <see cref="Angle"/>, in radians. Range: [0, 2π) </summary>
        public float Radians { get; }
        /// <summary> The value of this <see cref="Angle"/>, in degrees. Range: [0, 360°) </summary>
        public float Degrees => Radians * Rad2Deg;

        /// <summary>
        /// The <see cref="Pirates_Nueva.Vector"/> that represents this <see cref="Angle"/>.
        /// </summary>
        public Vector Vector => new Vector((float)Math.Cos(Radians), (float)Math.Sin(Radians));

        /// <summary> Create a new <see cref="Angle"/> struct, from a number in radians. </summary>
        public static Angle FromRadians(float rads) => new Angle(rads);
        /// <summary> Create a new <see cref="Angle"/> struct, from a number in degrees. </summary>
        public static Angle FromDegrees(float degs) => new Angle(degs * Deg2Rad);

        /// <summary>
        /// Return an angle, moving from /a/ towards /b/, with a maximum change of /step/.
        /// </summary>
        public static Angle MoveTowards(Angle a, Angle b, float step) {
            //
            // If the angles are equal, we can exit early.
            if(a == b)
                return b;
            //
            // Get the difference between the two angles.
            float difference = b - a;
            //
            // If the difference is greater than a half turn, normalize it.
            if(abs(difference) > HalfTurn)
                difference = HalfTurn - difference;
            //
            // If the difference is smaller than the step,
            // we can snap to the 2nd and return early.
            if(abs(difference) <= step)
                return b;
            //
            // Make sure that /difference/ is no larger than /step/.
            if(abs(difference) > step)
                difference = step * sign(difference);
            //
            // Add /difference/ to the first angle.
            return FromRadians(a + difference); // Add /difference/ to the first angle.

            float abs(float f) => Math.Abs(f);    float sign(float f) => Math.Sign(f);
        }

        public override string ToString() => Radians != 0 ? $"{Radians/Math.PI:0.##}π" : "0";

        public static explicit operator Angle(float rads) => FromRadians(rads);
        public static implicit operator float(Angle ang) => ang.Radians;

        public static Angle operator +(Angle a, Angle b) => FromRadians(a.Radians + b.Radians);
        public static Angle operator -(Angle a, Angle b) => FromRadians(a.Radians - b.Radians);
        public static Angle operator -(Angle ang) => FromRadians(-ang.Radians);
    }
}

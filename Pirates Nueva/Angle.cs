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
        public const float HalfTurn = MathF.PI;
        /// <summary> A full rotation around a circle, in radians. </summary>
        public const float FullTurn = 2*HalfTurn;
        /// <summary> Converts radians to degrees. </summary>
        public const float Rad2Deg  = 180f / HalfTurn;
        /// <summary> Converts degrees to radians. </summary>
        public const float Deg2Rad  = HalfTurn / 180f;

        /// <summary> 0π </summary>
        public static Angle Right { get; } = FromRadians(0);
        /// <summary> 1/2π </summary>
        public static Angle Up { get; } = FromRadians(HalfTurn * 1 / 2);
        /// <summary> π </summary>
        public static Angle Left { get; } = FromRadians(HalfTurn);
        /// <summary> 3/4π </summary>
        public static Angle Down { get; } = FromRadians(FullTurn * 3 / 4);

        /// <summary> The value of this <see cref="Angle"/>, in radians. Range: [0, 2π) </summary>
        public float Radians { get; }
        /// <summary> The value of this <see cref="Angle"/>, in degrees. Range: [0, 360°) </summary>
        public float Degrees => Radians * Rad2Deg;

        /// <summary>
        /// The <see cref="Pirates_Nueva.Vector"/> that represents this <see cref="Angle"/>.
        /// </summary>
        public Vector Vector => new Vector(MathF.Cos(Radians), MathF.Sin(Radians));

        /// <summary>
        /// Returns a new angle. Does not perform any input validation,
        /// so you gotta be sure that all inputs are valid.
        /// </summary>
        private Angle(float radians)
            => Radians = radians;

        /// <summary> Create a new <see cref="Angle"/> struct, from a number in radians. </summary>
        public static Angle FromRadians(float rads) {
            rads %= FullTurn;
            if(rads < 0)
                rads += FullTurn;
            return new Angle(rads);
        }
        /// <summary> Create a new <see cref="Angle"/> struct, from a number in degrees. </summary>
        public static Angle FromDegrees(float degs) => FromRadians(degs * Deg2Rad);

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

            static float abs(float f) => Math.Abs(f);    static float sign(float f) => Math.Sign(f);
        }

        public override string ToString() => Radians != 0 ? $"{Radians/MathF.PI:0.##}π" : "0";

        public static explicit operator Angle(float rads) => FromRadians(rads);
        public static implicit operator float(Angle ang) => ang.Radians;

        public static Angle operator +(Angle a, Angle b) {
            var sum = a.Radians + b.Radians;
            if(sum >= FullTurn)
                sum -= FullTurn;
            return new Angle(sum);
        }
        public static Angle operator -(Angle a, Angle b) {
            var dif = a.Radians - b.Radians;
            if(dif < 0)
                dif += FullTurn;
            return new Angle(dif);
        }
        public static Angle operator -(Angle ang) => ang.Radians == 0
                                                     ? ang
                                                     : new Angle(FullTurn - ang.Radians);
    }
}

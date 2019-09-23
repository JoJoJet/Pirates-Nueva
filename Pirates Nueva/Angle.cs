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

        /// <summary> A half rotation around a circle, in degrees. </summary>
        public const float HalfTurnDegs = FullTurnDegs / 2;
        /// <summary> A full rotation around a circle, in degrees. </summary>
        public const float FullTurnDegs = 360f;

        /// <summary> When a number is multiplied by this, converts from radians to degrees. </summary>
        public const float DegsPerRad = HalfTurnDegs / HalfTurn;
        /// <summary> When a number is multiplied by this, converts from degrees to radians. </summary>
        public const float RadsPerDeg = HalfTurn / HalfTurnDegs;

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
        public float Degrees => Radians * DegsPerRad;

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
            //
            // Perform a modulus to ensure that the value
            // is no greater in magnitude than a full turn.
            rads %= FullTurn;
            //
            // Ensure that the value is always positive.
            if(rads < 0)
                rads += FullTurn;
            return new Angle(rads);
        }
        /// <summary> Create a new <see cref="Angle"/> struct, from a number in degrees. </summary>
        public static Angle FromDegrees(float degs) {
            //
            // Perform a modulus to ensrue that the value
            // is no greater in magnitude than a full turn.
            degs %= FullTurnDegs;
            //
            // Ensure that the value is always positive.
            if(degs < 0)
                degs += FullTurnDegs;
            return new Angle(degs);
        }

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

        public override string ToString() => Radians != 0 ? $"{Radians/MathF.PI:0.###}π" : "0";

        public static explicit operator Angle(float rads) => FromRadians(rads);
        public static implicit operator float(Angle ang) => ang.Radians;

        public static Angle operator +(Angle a, Angle b) {
            //
            // The sum will always be less in magnitude than
            // two full turns, so we can perform subtraction
            // instead of a modulus.
            // It will also never be negative.
            var sum = a.Radians + b.Radians;
            if(sum >= FullTurn)
                sum -= FullTurn;
            return new Angle(sum);
        }
        public static Angle operator -(Angle a, Angle b) {
            //
            // The difference will always be less in magnitude
            // than a full turn, so we don't need to perform a modulus here.
            var dif = a.Radians - b.Radians;
            if(dif < 0)
                dif += FullTurn;
            return new Angle(dif);
        }
        public static Angle operator -(Angle ang) {
            //
            // As long as the input angle is not 0 radians,
            // the result of unary negation will always be
            // in a valid range, so we don't need to perform
            // any input validation.
            if(ang.Radians == 0)
                return ang;
            else
                return new Angle(FullTurn - ang.Radians);
        }

        public static Angle operator *(Angle a, float b) => FromRadians(a.Radians * b);
        public static Angle operator /(Angle a, float b) => FromRadians(a.Radians / b);
        public static Angle operator /(Angle a, int b) {
            //
            // We dont't need to perform a modulus here,
            // because since the denominator is > 0,
            // the quotient will always be smaller than a full turn.
            // We still need to check for negative though, as the denominator can be negative.
            var quo = a.Radians / b;
            if(quo < 0)
                quo += FullTurn;
            return new Angle(quo);
        }
    }
}

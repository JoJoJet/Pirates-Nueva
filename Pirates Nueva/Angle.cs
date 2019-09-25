using System;

namespace Pirates_Nueva
{
    /// <summary>
    /// Stores an angle, allowing access in both Radians and Degrees. Is always positive, and automatically wraps.
    /// <para />
    /// Range: [0, 2π) radians, [0°, 360°)
    /// </summary>
    public readonly struct Angle : IEquatable<Angle>
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

        #region Static Properties
        /// <summary> 0 </summary>
        public static Angle Right => new Angle(0);
        /// <summary> 1/2 π </summary>
        public static Angle Up => new Angle(HalfTurn * 1 / 2);
        /// <summary> π </summary>
        public static Angle Left => new Angle(HalfTurn);
        /// <summary> 3/4 π </summary>
        public static Angle Down => new Angle(FullTurn * 3 / 4);
        #endregion

        /// <summary> The value of this <see cref="Angle"/>, in radians. Range: [0, 2π) </summary>
        public float Radians { get; }
        /// <summary> The value of this <see cref="Angle"/>, in degrees. Range: [0, 360°) </summary>
        public float Degrees => Radians * DegsPerRad;

        /// <summary>
        /// The <see cref="Pirates_Nueva.Vector"/> that represents this <see cref="Angle"/>.
        /// </summary>
        public Vector Vector => new Vector(MathF.Cos(Radians), MathF.Sin(Radians));

        #region Constructors
        /// <summary>
        /// Returns a new angle. Does not perform any input validation,
        /// so you gotta be sure that all inputs are valid.
        /// </summary>
        private Angle(float radians)
            => Radians = radians;

        /// <summary> Creates a new <see cref="Angle"/> struct, from a number in radians. </summary>
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
        /// <summary> Creates a new <see cref="Angle"/> struct, from a number in degrees. </summary>
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
        #endregion

        #region Static Methods
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
            float difference = b.Radians - a.Radians;
            //
            // If the difference is greater in magnitude than a half turn, normalize it.
            if(difference > HalfTurn)
                difference = -FullTurn + difference;
            else if(difference < -HalfTurn)
                difference = FullTurn + difference;
            //
            // If the difference is smaller than the step,
            // we can snap to the 2nd and return early.
            var abs = MathF.Abs(difference);
            if(abs <= step)
                return b;
            //
            // Make sure that /difference/ is no larger than /step/.
            if(abs > step)
                difference = step * MathF.Sign(difference);
            //
            // Add /difference/ to the first angle.
            return FromRadians(a.Radians + difference);
        }

        /// <summary>
        /// Returns the unsigned distance between the two specified angles.
        /// </summary>
        public static float Distance(in Angle a, in Angle b) {
            //
            // Get the difference between the two angles.
            float difference = b.Radians - a.Radians;
            //
            // If it's greater in magnitude than a half turn, normalize it.
            if(difference > HalfTurn)
                difference = -FullTurn + difference;
            else if(difference < -HalfTurn)
                difference = FullTurn + difference;
            //
            // Return the absolute value of the diference.
            return MathF.Abs(difference);
        }
        #endregion

        #region Overriden Methods
        public override string ToString() => Radians != 0 ? $"{Radians/HalfTurn:0.###}π" : "0";

        public bool Equals(Angle other) => Radians == other.Radians;
        public override bool Equals(object obj) => obj switch
        {
            Angle a       => Radians == a.Radians,
            SignedAngle a => Radians == ((Angle)a).Radians,
            float f       => Radians == f,
            _             => false
        };
        public override int GetHashCode() => Radians.GetHashCode();
        #endregion

        #region Operators
        public static explicit operator Angle(float rads) => FromRadians(rads);
        public static implicit operator float(Angle ang) => ang.Radians;

        public static bool operator ==(Angle a, Angle b)
            => a.Radians == b.Radians;
        public static bool operator !=(Angle a, Angle b)
            => a.Radians != b.Radians;

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
            // because since the denominator is > 1,
            // the quotient will always be smaller than a full turn.
            // We still need to check for negative though, as the denominator can be negative.
            var quo = a.Radians / b;
            if(quo < 0)
                quo += FullTurn;
            return new Angle(quo);
        }
        #endregion
    }
}

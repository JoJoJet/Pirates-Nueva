﻿using System;

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

        /// <summary>
        /// Creates a new <see cref="Angle"/> instance, from a number in radians. <para />
        /// Does not perform any input validation, so you have to ensrue that the input is on [0, 2π)
        /// </summary>
        internal static Angle Unsafe(float rads) => new Angle(rads);
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
        public override bool Equals(object? obj) => obj switch
        {
            Angle a       => Radians == a.Radians,
            SignedAngle a => Radians == ((Angle)a).Radians,
            float f       => Radians == f,
            _             => false
        };
        public override int GetHashCode() => Radians.GetHashCode();
        #endregion

        #region Operators
        public static explicit operator Angle(in float rads) => FromRadians(rads);
        public static implicit operator float(in Angle ang) => ang.Radians;

        public static explicit operator Angle(in SignedAngle ang) {
            //
            // We don't need a modulus here,
            // as the signed angle will always be on [-2π, 2π].
            // The only input validation we have to worry about is
            // negatives, or an angle equalling 2π.
            var rads = ang.Radians;
            if(rads < 0)
                rads += FullTurn;
            else if(rads == FullTurn)
                rads = 0;
            return new Angle(rads);
        }

        public static bool operator ==(in Angle a, in Angle b)
            => a.Radians == b.Radians;
        public static bool operator !=(in Angle a, in Angle b)
            => a.Radians != b.Radians;

        public static Angle operator +(in Angle a, in Angle b) {
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
        public static Angle operator -(in Angle a, in Angle b) {
            //
            // The difference will always be less in magnitude
            // than a full turn, so we don't need to perform a modulus here.
            var dif = a.Radians - b.Radians;
            if(dif < 0)
                dif += FullTurn;
            return new Angle(dif);
        }
        public static Angle operator -(in Angle ang) {
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

        public static Angle operator *(in Angle a, in float b) => FromRadians(a.Radians * b);
        public static Angle operator /(in Angle a, in float b) => FromRadians(a.Radians / b);
        public static Angle operator /(in Angle a, in int b) {
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

    /// <summary>
    /// An angle for use in math, allowing access in both Radians and Degrees.
    /// Automatically wraps. Equality operators are not supported.
    /// <para />
    /// Range: (-2π, 2π) radians, (-360°, 360°).
    /// </summary>
    public readonly struct SignedAngle
    {
        const float HalfTurn = Angle.HalfTurn;
        const float FullTurn = Angle.FullTurn;

        const float DegsPerRad = Angle.DegsPerRad;
        const float RadsPerDeg = Angle.RadsPerDeg;

        #region Static Properties
        /// <summary> 0 </summary>
        public static SignedAngle Right => new SignedAngle(0);
        /// <summary> 1/2 π </summary>
        public static SignedAngle Up => new SignedAngle(HalfTurn * 1 / 2);
        /// <summary> π </summary>
        public static SignedAngle Left => new SignedAngle(HalfTurn);
        /// <summary> -π </summary>
        public static SignedAngle LeftN => new SignedAngle(-HalfTurn);
        /// <summary> -1/2 π </summary>
        public static SignedAngle Down => new SignedAngle(-HalfTurn * 1 / 2);
        #endregion

        /// <summary> The value of this angle, in radians. Range: (-2π, 2π). </summary>
        public float Radians { get; }
        /// <summary> The value of this angle, in degrees. Range: (-360, 360). </summary>
        public float Degrees => Radians * DegsPerRad;

        /// <summary>
        /// The <see cref="Pirates_Nueva.Vector"/> that represents this angle.
        /// </summary>
        public Vector Vector => new Vector(MathF.Cos(Radians), MathF.Sin(Radians));

        #region Constructors
        /// <summary>
        /// Returns a new angle. Does not perform any input validation,
        /// so you gotta make sure all inputs are valid.
        /// </summary>
        private SignedAngle(float radians)
            => Radians = radians;

        /// <summary>
        /// Creates a new <see cref="SignedAngle"/> instance, from a number in radians.
        /// </summary>
        public static SignedAngle FromRadians(float rads)
            => new SignedAngle(rads % FullTurn);
        /// <summary>
        /// Creates a new <see cref="SignedAngle"/> instance, from a number in degrees.
        /// </summary>
        public static SignedAngle FromDegrees(float degs)
            => new SignedAngle((degs * RadsPerDeg) % FullTurn);

        /// <summary>
        /// Creates a new <see cref="SignedAngle"/> instance, from a number in radians. <para />
        /// Does not perform any input validation, so you must be sure that the input is on (-2π, 2π).
        /// </summary>
        internal static SignedAngle Unsafe(float rads) => new SignedAngle(rads);
        #endregion

        #region Overriden Methods and Operators
        public override string ToString() => Radians != 0 ? $"{Radians/HalfTurn:0.###}π" : "0";

        public override bool Equals(object? obj) => throw new NotImplementedException();
        public override int GetHashCode() => throw new NotImplementedException();

        public static implicit operator float(in SignedAngle ang)
            => ang.Radians;
        public static explicit operator SignedAngle(in float f)
            => FromRadians(f);

        public static implicit operator SignedAngle(in Angle ang)
            => new SignedAngle(ang.Radians);

        private const string EqualityError = "Equality comparison is not supported on " +
                                              nameof(SignedAngle) + " struct.";
        [Obsolete(EqualityError, true)]
        public static bool operator ==(SignedAngle a, SignedAngle b)
            => throw new NotImplementedException();
        [Obsolete(EqualityError, true)]
        public static bool operator !=(SignedAngle a, SignedAngle b)
            => throw new NotImplementedException();

        public static bool operator >(in SignedAngle a, in SignedAngle b)
            => a.Radians > b.Radians;
        public static bool operator <(in SignedAngle a, in SignedAngle b)
            => a.Radians < b.Radians;
        public static bool operator >=(in SignedAngle a, in SignedAngle b)
            => a.Radians >= b.Radians;
        public static bool operator <=(in SignedAngle a, in SignedAngle b)
            => a.Radians <= b.Radians;

        public static SignedAngle operator +(in SignedAngle a, in SignedAngle b) {
            //
            // Since the sum will always be less in magnitude than two full turns,
            // we don't need ot use modulus.
            // We can use addition or subtraction instead.
            var sum = a.Radians + b.Radians;
            if(sum <= -FullTurn)
                sum += FullTurn;
            else if(sum >= FullTurn)
                sum -= FullTurn;
            return new SignedAngle(sum);
        }
        public static SignedAngle operator -(in SignedAngle a, in SignedAngle b) {
            //
            // Since the difference will always be less in magnitude than two full turns,
            // we don't need to use modulus.
            // WE can use addition or subtraction instead.
            var dif = a.Radians - b.Radians;
            if(dif <= -FullTurn)
                dif += FullTurn;
            else if(dif >= FullTurn)
                dif -= FullTurn;
            return new SignedAngle(dif);
        }
        public static SignedAngle operator -(in SignedAngle a)
            => new SignedAngle(-a.Radians);

        public static SignedAngle operator *(in SignedAngle a, in float b)
            => FromRadians(a.Radians * b);
        public static SignedAngle operator /(in SignedAngle a, in float b)
            => FromRadians(a.Radians * b);
        public static SignedAngle operator /(in SignedAngle a, in int b)
            //
            // We can elide a modulus here.
            // Since the denominator is always > 1, the quotient
            // is always smaller in magnitude than a full turn.
            => new SignedAngle(a.Radians / b);
        #endregion
    }
}

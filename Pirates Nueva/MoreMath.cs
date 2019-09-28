using System;

namespace Pirates_Nueva
{
    /// <summary>
    /// Includes additions to the <see cref="Math"/> and <see cref="MathF"/> classes.
    /// </summary>
    public static class MoreMath
    {
        /// <summary> Linearly interpolates between two values, by amount /f/. </summary>
        public static float Lerp(float a, float b, float f) => a * (1 - f) + b * f;
    }
}

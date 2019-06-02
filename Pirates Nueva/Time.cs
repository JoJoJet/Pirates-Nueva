using System;
#nullable enable

namespace Pirates_Nueva
{
    /// <summary>
    /// An value containing the time since the previous frame.
    /// </summary>
    public readonly struct Time
    {
        /// <summary>
        /// The number of seconds since last frame. <see cref="Time"/> can implicitly convert to a <see cref="float"/> with this value.
        /// </summary>
        public float Seconds { get; }
        /// <summary> The number of miliseconds since last frame. </summary>
        public float Miliseconds { get; }

        internal Time(Microsoft.Xna.Framework.GameTime time) {
            var span = time.ElapsedGameTime;
            Seconds     = (float)span.TotalSeconds;
            Miliseconds = (float)span.TotalMilliseconds;
        }

        public static implicit operator float(Time t) => t.Seconds;
    }
}

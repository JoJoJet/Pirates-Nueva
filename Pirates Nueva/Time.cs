using System;

namespace Pirates_Nueva
{
    /// <summary>
    /// An immutable value containing the time since the last frame.
    /// </summary>
    public readonly struct Time
    {
        private readonly TimeSpan time;

        /// <summary>
        /// The number of seconds since last frame. This instance can implicitly convert to a <see cref="float"/> with this value.
        /// </summary>
        public float Seconds => (float)time.TotalSeconds;
        /// <summary> The number of miliseconds since last frame. </summary>
        public float Miliseconds => (float)time.TotalMilliseconds;

        internal Time(Microsoft.Xna.Framework.GameTime time) {
            this.time = time.ElapsedGameTime;
        }

        public static implicit operator float(Time t) => t.Seconds;
    }
}

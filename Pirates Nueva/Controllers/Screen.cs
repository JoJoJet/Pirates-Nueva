using System;
using System.Collections.Generic;
using System.Text;

namespace Pirates_Nueva
{
    /// <summary>
    /// An object holding info about the screen.
    /// </summary>
    public sealed class Screen
    {
        /// <summary> The width of this <see cref="Screen"/> in pixels. </summary>
        public int Width => Master.GraphicsDevice.Viewport.Width;
        /// <summary> The height of this <see cref="Screen"/> in pixels. </summary>
        public int Height => Master.GraphicsDevice.Viewport.Height;

        private Master Master { get; }

        internal Screen(Master master) => Master = master;
    }
}

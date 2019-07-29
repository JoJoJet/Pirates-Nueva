using System;
using Pirates_Nueva.Ocean;

namespace Pirates_Nueva
{
    /// <summary>
    /// An object holding info about the screen.
    /// </summary>
    public sealed class Screen : ISpaceLocus<Screen>
    {
        /// <summary> The width of this <see cref="Screen"/> in pixels. </summary>
        public int Width => Master.GraphicsDevice.Viewport.Width;
        /// <summary> The height of this <see cref="Screen"/> in pixels. </summary>
        public int Height => Master.GraphicsDevice.Viewport.Height;

        private Master Master { get; }

        internal Screen(Master master) => Master = master;

        #region ISpaceLocus<> Implementation
        ISpaceLocus? ISpaceLocus.Parent => null;
        ISpace ISpaceLocus.Transformer => throw new NotImplementedException();
        ISpace<Screen> ISpaceLocus<Screen>.Transformer => throw new NotImplementedException();
        #endregion
    }
}

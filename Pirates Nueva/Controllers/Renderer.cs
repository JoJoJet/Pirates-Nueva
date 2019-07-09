using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pirates_Nueva
{
    /// <summary>
    /// An object that transforms and draws things to the screen.
    /// </summary>
    /// <typeparam name="T">The type of object around which things will be drawn.</typeparam>
    public interface ILocalDrawer<T>
    {
        /// <summary>
        /// Submits a <see cref="UI.Texture"/> to be drawn this frame, drawing from the top left corner.
        /// </summary>
        /// <param name="left">The local coordinate of the <see cref="UI.Texture"/>'s left edge.</param>
        /// <param name="top">The local coordinate of the <see cref="UI.Texture"/>'s top edge.</param>
        /// <param name="width">The width of the <see cref="UI.Texture"/>, in local units.</param>
        /// <param name="height">The height of the <see cref="UI.Texture"/>, in local units.</param>
        void DrawCorner(UI.Texture texture, float left, float top, float width, float height, in UI.Color tint);
        /// <summary>
        /// Submits a rotated <see cref="UI.Texture"/> to be drawn this frame.
        /// </summary>
        /// <param name="x">The local x coordinate at which to draw the <see cref="UI.Texture"/>.</param>
        /// <param name="y">The local y coordinate at which to draw the <see cref="UI.Texture"/>.</param>
        /// <param name="width">The width of the <see cref="UI.Texture"/>, in local units.</param>
        /// <param name="height">The height of the <see cref="UI.Texture"/>, in local units.</param>
        /// <param name="angle">The <see cref="Angle"/> at which to draw the <see cref="UI.Texture"/>.</param>
        /// <param name="origin">The point, relative to the <see cref="UI.Texture"/>,
        ///                      from which to draw it. Range: [0, 1].</param>
        void Draw(UI.Texture texture, float x, float y, float width, float height, in Angle angle, in PointF origin, in UI.Color tint);

        /// <summary>
        /// Draws a line with specified color this frame.
        /// </summary>
        /// <param name="start">The local position of the line's starting point.</param>
        /// <param name="end">The local position of the line's ending point.</param>
        void DrawLine(PointF start, PointF end, in UI.Color color);

        /// <summary>
        /// Submits a <see cref="string"/> to be drawn this frame.
        /// </summary>
        /// <param name="left">The local coordinate of the <see cref="string"/>'s left edge.</param>
        /// <param name="top">The local coordinate of the <see cref="string"/>'s top edge.</param>
        void DrawString(UI.Font font, string text, float left, float top, in UI.Color color);
    }
    public static class DrawerExt
    {
        /// <summary>
        /// Submits a <see cref="UI.Texture"/> to be drawn this frame, drawing from the center of the texture.
        /// </summary>
        /// <param name="centerX">The local x coordinate of the <see cref="UI.Texture"/>'s center.</param>
        /// <param name="centerY">The local y coordinate of the <see cref="UI.Texture"/>'s center.</param>
        /// <param name="width">The width of the <see cref="UI.Texture"/>, in local units.</param>
        /// <param name="height">The height of the <see cref="UI.Texture"/>, in local units.</param>
        public static void DrawCenter<T>(this ILocalDrawer<T> drawer, UI.Texture texture,
                                         float centerX, float centerY, float width, float height, in UI.Color tint)
            => drawer.DrawCorner(texture, centerX - width / 2, centerY + width / 2, width, height, in tint);


        /// <summary>
        /// Submits a <see cref="UI.Texture"/> to be drawn this frame, drawing from the center of the texture.
        /// </summary>
        /// <param name="centerX">The local x coordinate of the <see cref="UI.Texture"/>'s center.</param>
        /// <param name="centerY">The local y coordinate of the <see cref="UI.Texture"/>'s center.</param>
        /// <param name="width">The width of the <see cref="UI.Texture"/>, in local units.</param>
        /// <param name="height">The height of the <see cref="UI.Texture"/>, in local units.</param>
        public static void DrawCenter<T>(this ILocalDrawer<T> drawer, UI.Texture texture,
                                         float centerX, float centerY, float width, float height)
            => drawer.DrawCenter(texture, centerX, centerY, width, height, in UI.Color.White);
        /// <summary>
        /// Submits a <see cref="UI.Texture"/> to be drawn this frame, drawing from the top left corner.
        /// </summary>
        /// <param name="left">The local coordinate of the <see cref="UI.Texture"/>'s left edge.</param>
        /// <param name="top">The local coordinate of the <see cref="UI.Texture"/>'s top edge.</param>
        /// <param name="width">The width of the <see cref="UI.Texture"/>, in local units.</param>
        /// <param name="height">The height of the <see cref="UI.Texture"/>, in local units.</param>
        public static void DrawCorner<T>(this ILocalDrawer<T> drawer, UI.Texture texture,
                                         float left, float top, float width, float height)
            => drawer.DrawCorner(texture, left, top, width, height, in UI.Color.White);
        /// <summary>
        /// Submits a rotated <see cref="UI.Texture"/> to be drawn this frame.
        /// </summary>
        /// <param name="x">The local x coordinate at which to draw the <see cref="UI.Texture"/>.</param>
        /// <param name="y">The local y coordinate at which to draw the <see cref="UI.Texture"/>.</param>
        /// <param name="width">The width of the <see cref="UI.Texture"/>, in local units.</param>
        /// <param name="height">The height of the <see cref="UI.Texture"/>, in local units.</param>
        /// <param name="angle">The <see cref="Angle"/> at which to draw the <see cref="UI.Texture"/>.</param>
        /// <param name="origin">The point, relative to the <see cref="UI.Texture"/>,
        ///                      from which to draw it. Range: [0, 1].</param>
        public static void Draw<T>(this ILocalDrawer<T> drawer, UI.Texture texture,
                                   float x, float y, float width, float height,
                                   in Angle angle, in PointF origin)
            => drawer.Draw(texture, x, y, width, height, in angle, in origin, in UI.Color.White);
    }

    /// <summary>
    /// An instance that can be locally drawn around an object.
    /// </summary>
    /// <typeparam name="T">The type of object around which the instance will be drawn.</typeparam>
    internal interface IDrawable<T>
    {
        /// <summary>
        /// Draws this object around its parent.
        /// </summary>
        /// <param name="drawer">The object on which to draw.</param>
        void Draw(ILocalDrawer<T> drawer);
    }


    /// <summary>
    /// Controls rendering for <see cref="Pirates_Nueva"/>.
    /// </summary>
    internal class Renderer : ILocalDrawer<Master>
    {
        private Lazy<UI.Texture> pixelLazy;

        private Master Master { get; }
        private SpriteBatch SpriteBatch { get; }

        private UI.Texture Pixel => this.pixelLazy.Value;

        internal Renderer(Master master, SpriteBatch spriteBatch) {
            Master = master;
            SpriteBatch = spriteBatch;

            this.pixelLazy = new Lazy<UI.Texture>(() => Master.CreateTexture(1, 1, UI.Color.White));
        }

        public void DrawCorner(UI.Texture texture, float left, float top, float width, float height, in UI.Color tint)
            => SpriteBatch.Draw(texture, new Rectangle((int)left, (int)top, (int)width, (int)height), tint);
        public void Draw(UI.Texture texture, float x, float y, float width, float height,
                         in Angle angle, in PointF origin, in UI.Color tint)
            => SpriteBatch.Draw(texture, new Rectangle((int)x, (int)y, (int)width, (int)height),
                                null, tint, angle, origin, SpriteEffects.None, 0f);

        public void DrawLine(PointF start, PointF end, in UI.Color color) {
            var edge = end - start;
            var angle = (Angle)Math.Atan2(edge.Y, edge.X);

            Draw(Pixel, start.X, start.Y, edge.Magnitude, 1, angle, (0, 0), in color);
        }

        public void DrawString(UI.Font font, string text, float left, float top, in UI.Color color)
            => SpriteBatch.DrawString(font, text, new PointF(left, top), color);
    }
}

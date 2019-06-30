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
        /// Submits a <see cref="UI.Texture"/> to be drawn this frame.
        /// </summary>
        /// <param name="centerX">The local x coordinate of the <see cref="UI.Texture"/>'s center.</param>
        /// <param name="centerY">The local y coordinate of the <see cref="UI.Texture"/>'s center.</param>
        /// <param name="width">The width of the <see cref="UI.Texture"/>, in local units.</param>
        /// <param name="height">The height of the <see cref="UI.Texture"/>, in local units.</param>
        void Draw(UI.Texture texture, float centerX, float centerY, float width, float height, in UI.Color tint);
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
    }

    public static class DrawerExt
    {
        /// <summary>
        /// Submits a <see cref="UI.Texture"/> to be drawn this frame.
        /// </summary>
        /// <param name="centerX">The local x coordinate of the <see cref="UI.Texture"/>'s center.</param>
        /// <param name="centerY">The local y coordinate of the <see cref="UI.Texture"/>'s center.</param>
        /// <param name="width">The width of the <see cref="UI.Texture"/>, in local units.</param>
        /// <param name="height">The height of the <see cref="UI.Texture"/>, in local units.</param>
        public static void Draw<T>(this ILocalDrawer<T> drawer, UI.Texture texture,
                                   float centerX, float centerY, float width, float height)
            => drawer.Draw(texture, centerX, centerY, width, height, in UI.Color.White);
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
    /// Controls rendering for <see cref="Pirates_Nueva"/>.
    /// </summary>
    public class Renderer
    {
        private Lazy<UI.Texture> pixelLazy;

        public Master Master { get; }

        private SpriteBatch SpriteBatch { get; }

        private UI.Texture Pixel => this.pixelLazy.Value;

        internal Renderer(Master master, SpriteBatch spriteBatch) {
            Master = master;
            SpriteBatch = spriteBatch;

            this.pixelLazy = new Lazy<UI.Texture>(() => CreateTexture(1, 1, UI.Color.White));
        }

        /// <summary>
        /// Create a new <see cref="UI.Texture"/> with specified width, height, and pixel colors.
        /// </summary>
        public UI.Texture CreateTexture(int width, int height, params UI.Color[] pixels) {
            var tex = new Texture2D(Master.GraphicsDevice, width, height);
            tex.SetData(pixels);
            return new UI.Texture(tex);
        }

        /// <summary> Draws a texture this frame with the specified tint value. </summary>
        public void Draw(UI.Texture texture, int left, int top, int width, int height, in UI.Color tint)
            => SpriteBatch.Draw(texture, new Rectangle(left, top, width, height), tint);
        /// <summary> Draws the specified texture this frame. </summary>
        public void Draw(UI.Texture texture, int left, int top, int width, int height)
            => Draw(texture, left, top, width, height, in UI.Color.White);

        /// <summary> Draws a rotated texture this frame with the specified tint value. </summary>
        public void DrawRotated(UI.Texture texture, int x, int y, int width, int height, Angle angle, PointF origin, in UI.Color tint)
            => SpriteBatch.Draw(texture, new Rectangle(x, y, width, height), null, tint, angle, origin, SpriteEffects.None, 0f);
        /// <summary> Draws the specified texture this frame. </summary>
        public void DrawRotated(UI.Texture texture, int x, int y, int width, int height, Angle angle, PointF origin)
            => DrawRotated(texture, x, y, width, height, angle, origin, in UI.Color.White);

        /// <summary> Draws the specified text this frame. </summary>
        public void DrawString(UI.Font font, string text, int left, int top, in UI.Color color)
            => SpriteBatch.DrawString(font, text, new PointF(left, top), color);
        /// <summary> Draws the specified text this frame, in black. </summary>
        public void DrawString(UI.Font font, string text, int left, int top)
            => DrawString(font, text, left, top, in UI.Color.Black);

        /// <summary> Draws a line this frame with the specified color. </summary>
        public void DrawLine(PointI start, PointI end, in UI.Color color) {
            var edge = end - start;
            var angle = (Angle)Math.Atan2(edge.Y, edge.X);

            DrawRotated(Pixel, start.X, start.Y, (int)edge.Magnitude, 1, angle, (0, 0), in color);
        }
        /// <summary> Draws a white line this frame. </summary>
        public void DrawLine(PointI start, PointI end)
            => DrawLine(start, end, in UI.Color.White);
    }
}

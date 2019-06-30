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
        void Draw(UI.Texture texture, float left, float top, float width, float height, in UI.Color tint);
        void Draw(UI.Texture texture, float x, float y, float width, float height, in Angle angle, in PointF origin, in UI.Color tint);
    }

    public static class DrawerExt
    {
        public static void Draw<T>(this ILocalDrawer<T> drawer, UI.Texture texture,
                                   float x, float y, float width, float height)
            => drawer.Draw(texture, x, y, width, height, in UI.Color.White);
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

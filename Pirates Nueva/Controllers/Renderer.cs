using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pirates_Nueva
{
    /// <summary>
    /// Controls rendering for <see cref="Pirates_Nueva"/>.
    /// </summary>
    public class Renderer
    {
        private UI.Texture _pixel;

        public Master Master { get; }

        private SpriteBatch SpriteBatch { get; }

        private UI.Texture Pixel {
            get => _pixel ?? (_pixel = CreateTexture(1, 1, UI.Color.White));
        }

        internal Renderer(Master master, SpriteBatch spriteBatch) {
            Master = master;
            SpriteBatch = spriteBatch;
        }

        /// <summary>
        /// Create a new <see cref="UI.Texture"/> with specified width, height, and pixel colors.
        /// </summary>
        public UI.Texture CreateTexture(int width, int height, params UI.Color[] pixels) {
            var tex = new Texture2D(Master.GraphicsDevice, width, height);
            tex.SetData(pixels);
            return new UI.Texture(tex);
        }

        /// <summary> Submit a sprite to be drawn this frame. </summary>
        public void Draw(UI.Texture texture, int left, int top, int width, int height, UI.Color? tint = null) {
            var color = tint ?? Color.White;
            SpriteBatch.Draw(texture, new Rectangle(left, top, width, height), color);
        }
        /// <summary> Submit a rotated sprite to be drawn this frame. </summary>
        public void DrawRotated(UI.Texture texture, int x, int y, int width, int height, Angle angle, PointF origin, UI.Color? tint = null) {
            var color = tint ?? Color.White;
            SpriteBatch.Draw(texture, new Rectangle(x, y, width, height), null, color, angle, origin, SpriteEffects.None, 0f);
        }

        /// <summary> Submit a string to be drawn this frame. </summary>
        public void DrawString(UI.Font font, string text, int left, int top, UI.Color color) {
            SpriteBatch.DrawString(font, text, new PointF(left, top), color);
        }
        /// <summary> Submit a string to be drawn this frame. </summary>
        public void DrawString(UI.Font font, string text, PointI topLeft, UI.Color color) {
            SpriteBatch.DrawString(font, text, (PointF)topLeft, color);
        }

        /// <summary> Submit a line between two points to be drawn this frame. </summary>
        public void DrawLine(PointI start, PointI end, UI.Color? tint = null) {
            var edge = end - start;
            var angle = (Angle)Math.Atan2(edge.Y, edge.X);

            DrawRotated(Pixel, start.X, start.Y, (int)edge.Magnitude, 1, angle, (0, 0), tint);
        }
    }
}

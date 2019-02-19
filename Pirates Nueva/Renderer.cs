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
        public Master Master { get; }

        private SpriteBatch SpriteBatch { get; }

        internal Renderer(Master master, SpriteBatch spriteBatch) {
            Master = master;
            SpriteBatch = spriteBatch;
        }

        /// <summary> Submit a sprite to be drawn this frame. </summary>
        public void Draw(UI.Texture texture, int left, int top, int width, int height, Color? tint = null) {
            var color = tint ?? Color.White;
            SpriteBatch.Draw(texture, new Rectangle(left, top, width, height), color);
        }
        /// <summary> Submit a rotated sprite to be drawn this frame. </summary>
        public void DrawRotated(UI.Texture texture, int x, int y, int width, int height, Angle angle, PointF origin, Color? tint = null) {
            var color = tint ?? Color.White;
            SpriteBatch.Draw(texture, new Rectangle(x, y, width, height), null, color, angle, origin, SpriteEffects.None, 0f);
        }

        /// <summary> Submit a string to be drawn this frame. </summary>
        public void DrawString(UI.Font font, string text, int left, int top, Color color) {
            SpriteBatch.DrawString(font, text, new PointF(left, top), color);
        }
        /// <summary> Submit a string to be drawn this frame. </summary>
        public void DrawString(UI.Font font, string text, PointI topLeft, Color color) {
            SpriteBatch.DrawString(font, text, (PointF)topLeft, color);
        }
    }
}

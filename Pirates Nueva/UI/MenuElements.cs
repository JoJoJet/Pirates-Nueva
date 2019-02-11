using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Pirates_Nueva.UI
{
    /// <summary> A bit of text in a <see cref="GUI.Menu"/>. </summary>
    public class MenuText : GUI.MenuElement
    {
        /// <summary> The string of this <see cref="MenuText"/>. </summary>
        public string Text { get; }
        /// <summary> The <see cref="UI.Font"/> to render the text with. </summary>
        public Font Font { get; }

        /// <summary> The width of this element, in pixels. </summary>
        public override int WidthPixels => (int)Font.MeasureString(Text).X;
        /// <summary> The height of this element, in pixels. </summary>
        public override int HeightPixels => (int)Font.MeasureString(Text).Y;

        /// <summary>
        /// Create a <see cref="MenuText"/>, and allow its <see cref="GUI.Menu"/> to generate a position for it.
        /// </summary>
        public MenuText(string text, Font font) {
            Text = text;
            Font = font;
        }

        protected override void Draw(Master master, int left, int top) {
            master.SpriteBatch.DrawString(Font, Text, new PointF(left, top), Color.Black);
        }
        protected override bool IsMouseOver(PointI mouse, int left, int top) {
            return new Rectangle(left, top, WidthPixels, HeightPixels).Contains(mouse);
        }
    }
}

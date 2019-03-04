using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Pirates_Nueva.UI
{
    /// <summary>
    /// A bit of text in a <see cref="GUI.Menu"/>.
    /// </summary>
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

        /// <summary> Draw this <see cref="MenuText"/> onscreen, from the specified top left corner. </summary>
        protected override void Draw(Master master, int left, int top) {
            master.Renderer.DrawString(Font, Text, left, top, Color.Black);
        }
    }

    /// <summary>
    /// A button in a <see cref="GUI.Menu"/>.
    /// </summary>
    public class MenuButton : GUI.MenuElement, GUI.IButton
    {
        const int Padding = 4; // Padding on each edge of the button.

        /// <summary> The text on this <see cref="MenuButton"/>. </summary>
        public string Text { get; }
        /// <summary> The <see cref="UI.Font"/> to render the text with. </summary>
        public Font Font { get; }

        /// <summary> The width of this <see cref="MenuButton"/>, in pixels. </summary>
        public override int WidthPixels => (int)Font.MeasureString(Text).X + Padding*2;
        /// <summary> The height of this <see cref="MenuButton"/>, in pixels. </summary>
        public override int HeightPixels => (int)Font.MeasureString(Text).Y + Padding*2;

        private GUI.OnClick OnClick { get; }
        #region Hidden Properties
        GUI.OnClick GUI.IButton.OnClick => OnClick;
        #endregion

        /// <summary>
        /// Create a <see cref="MenuButton"/>, and allow its <see cref="GUI.Menu"/> to generate a position for it.
        /// </summary>
        public MenuButton(string text, Font font, GUI.OnClick onClick) {
            Text = text;
            Font = font;
            OnClick = onClick;
        }
        /// <summary>
        /// Create a <see cref="MenuButton"/> at the specified position.
        /// </summary>
        public MenuButton(int left, int top, string text, Font font, GUI.OnClick onClick) : base(left, top) {
            Text = text;
            Font = font;
            OnClick = onClick;
        }
        /// <summary> Draw this <see cref="MenuButton"/> onscreen, from the specified top left corner. </summary>
        protected override void Draw(Master master, int left, int top) {
            var panel = new NineSlice(Def.Get<SliceDef>("panel"), WidthPixels, HeightPixels, master); // Make a panel.
            master.Renderer.Draw(panel, left, top, WidthPixels, HeightPixels);                        // Draw a panel behind the text.

            master.Renderer.DrawString(Font, Text, left+Padding, top+Padding, Color.Black);           // Draw the text.
        }
    }
}

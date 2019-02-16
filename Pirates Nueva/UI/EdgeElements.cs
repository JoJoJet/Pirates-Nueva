using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Pirates_Nueva.UI
{
    /// <summary>
    /// A bit of text that hugs an edge of the screen, not tied to any menu.
    /// </summary>
    public class EdgeText : GUI.EdgeElement
    {
        private string _text;

        /// <summary> The string of this <see cref="EdgeText"/>. </summary>
        public string Text {
            get => this._text;
            set {
                string old = this._text; // Store the old value of Text.
                this._text = value;      // Set the new value of Text.

                if(old != value)        // If the value of Text has changed,
                    PropertyChanged();  // update the arrangement of floating elements in GUI.
            }
        }
        /// <summary> The <see cref="UI.Font"/> to render this <see cref="MenuText"/> with. </summary>
        public Font Font { get; }

        /// <summary> The width of this <see cref="EdgeText"/>, in pixels. </summary>
        public override int WidthPixels => (int)Font.MeasureString(Text).X;
        /// <summary> The height of this <see cref="EdgeText"/>, in pixels. </summary>
        public override int HeightPixels => (int)Font.MeasureString(Text).Y;

        public EdgeText(string text, Font font, GUI.Edge edge, GUI.Direction direction) : base(edge, direction) {
            this._text = text;
            Font = font;
        }

        /// <summary> Draw this <see cref="EdgeText"/> onscreen, from the specified top left corner. </summary>
        protected override void Draw(Master master, int left, int top) {
            master.Renderer.DrawString(Font, Text, left, top, Color.Black);
        }
    }
    
    /// <summary>
    /// A button that hugs an edge of the screen, not tied to any menu.
    /// </summary>
    public class EdgeButton : GUI.EdgeElement, GUI.IButtonContract
    {
        const int Padding = 3;

        /// <summary> Text to display on this <see cref="EdgeButton"/>. </summary>
        public string Text { get; }
        /// <summary> The <see cref="UI.Font"/> to render this button's text with. </summary>
        public Font Font { get; }

        /// <summary> The width of this <see cref="EdgeButton"/>, in pixels. </summary>
        public override int WidthPixels => (int)Font.MeasureString(Text).X + Padding*2;
        /// <summary> The width of this <see cref="EdgeButton"/>, in pixels. </summary>
        public override int HeightPixels => (int)Font.MeasureString(Text).Y + Padding*2;

        private GUI.OnClick OnClick { get; set; }
        #region Hidden Properties
        GUI.OnClick GUI.IButtonContract.OnClick => OnClick;
        #endregion

        public EdgeButton(string text, Font font, GUI.OnClick onClick, GUI.Edge edge, GUI.Direction direction) : base(edge, direction) {
            Text = text;
            Font = font;
            OnClick = onClick;
        }

        /// <summary> Draw this <see cref="EdgeButton"/> onscreen, from the specified top left corner. </summary>
        protected override void Draw(Master master, int left, int top) {
            var panel = new NineSlice(Def.Get<SliceDef>("panel"), WidthPixels, HeightPixels, master); // Make a panel.
            master.Renderer.Draw(panel, left, top, WidthPixels, HeightPixels);                        // Draw a panel behind the text.
            
            master.Renderer.DrawString(Font, Text, left+Padding, top+Padding, Color.Black);           // Draw the text.

        }
    }
}

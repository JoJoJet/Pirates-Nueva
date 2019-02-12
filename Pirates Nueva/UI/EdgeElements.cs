﻿using System;
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

                if(old != value && GUI != null) // If the value of Text has changed,
                    PropertyChanged();          // update the arrangement of floating elements in GUI.
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

        protected override void Draw(Master master, int left, int top) {
            PointF pos = (left, top);
            master.SpriteBatch.DrawString(Font, Text, pos, Color.Black);
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

        protected override void Draw(Master master, int left, int top) {
            var pos = new Vector2(left, top);
            pos += new Vector2(Padding, Padding);

            master.SpriteBatch.DrawString(Font, Text, pos, Color.Green);
        }
    }
}

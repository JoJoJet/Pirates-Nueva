using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Pirates_Nueva
{
    /// <summary>
    /// Controls the user interface for <see cref="Pirates_Nueva"/>.
    /// </summary>
    public class GUI : IUpdatable, IDrawable
    {
        private Dictionary<string, IFloating> _floatingElements = new Dictionary<string, IFloating>();

        public static Master Master { get; private set; }

        static SpriteFont Font => Master.Font;

        public int ScreenWidth => Master.GraphicsDevice.Viewport.Width;
        public int ScreenHeight => Master.GraphicsDevice.Viewport.Height;

        internal GUI(Master master) {
            Master = master;
        }
        
        #region Floating Accessors
        /// <summary>
        /// Add the indicated floating element to the GUI.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if there is already a floating element identified by /id/.</exception>
        public void AddFloating(string id, IFloating floating) {
            if(_floatingElements.ContainsKey(id) == false) {
                (floating as IFloatingContract).GUI = this;
                _floatingElements[id] = floating;
                ArrangeFloating();
            }
            else {
                throw new InvalidOperationException(
                    $"{nameof(GUI)}.{nameof(AddFloating)}(): There is already a floating element named \"{id}\"!"
                    );
            }
        }

        /// <summary>
        /// Tries to get the floating element identified by /id/, and returns whether or not it was successful.
        /// If it was successful, stuffs that value into /floating/.
        /// </summary>
        public bool TryGetFloating(string id, out IFloating floating) => this._floatingElements.TryGetValue(id, out floating);

        /// <summary>
        /// Tries to get the floating element identified by /id/, and returns whether or not it was successful.
        /// If it was successful, stuffs that value into /floating/.
        /// </summary>
        public bool TryGetFloating<T>(string id, out T floating) where T : IFloating {
            if(TryGetFloating(id, out IFloating fl)) {
                // If there is a floating element identified by /id/ and of type /T/.
                if(fl is T tf) {
                    floating = tf;
                    return true;
                }
                // If there is a floating element identifed by /id/, but it is not of type /T/.
                else {
                    floating = default;
                    return false;
                }
            }
            // If there is no floating element identified by /id/.
            else {
                floating = default;
                return false;
            }
        }
        #endregion

        /// <summary>
        /// Update the arrangement of Floating elements.
        /// </summary>
        void ArrangeFloating() {
            const int Padding = 3;

            int topRight    =                Padding; // Top left corner,     ->
            int topLeft     = ScreenWidth  - Padding; // Top right corner,    <-
            int bottomRight =                Padding; // Bottom left corner,  ->
            int bottomLeft  = ScreenWidth  - Padding; // Bottom right corner, <-
            int rightUp     = ScreenHeight - Padding; // Bottom right corner, ↑
            int rightDown   =                Padding; // Top right corner,    ↓
            int leftUp      = ScreenHeight - Padding; // Bottom left corner,  ↑
            int leftDown    =                Padding; // Top left corner,     ↓

            foreach(IFloating floating in this._floatingElements.Values) {
                if(!(floating is IFloatingContract con))
                    continue;

                switch(floating.Edge) {
                    case Edge.Top:
                        switch(floating.StackDirection) {
                            // Top left corner, stacking to the RIGHT.
                            case Direction.Right: {
                                con.Top = Padding;
                                con.Left = topRight;

                                topRight += floating.WidthPixels + Padding;
                            } break;

                            // Top right corner, stacking to the LEFT.
                            case Direction.Left: {
                                topLeft -= floating.WidthPixels + Padding;

                                con.Top = Padding;
                                con.Left = topLeft;
                            } break;
                        }
                    break;

                    case Edge.Bottom:
                        switch(floating.StackDirection) {
                            // Bottom left corner, stacking to the RIGHT.
                            case Direction.Right: {
                                con.Top = ScreenHeight - floating.HeightPixels - Padding;
                                con.Left = bottomRight;

                                bottomRight += floating.WidthPixels + Padding;
                            } break;

                            // Bottom right corner, stacking to the LEFT.
                            case Direction.Left: {
                                bottomLeft -= floating.WidthPixels + Padding;

                                con.Top = ScreenHeight - floating.HeightPixels - Padding;
                                con.Left = bottomLeft;
                            } break;
                        }
                    break;

                    case Edge.Right:
                        switch(floating.StackDirection) {
                            // Bottom right corner, stacking UPwards.
                            case Direction.Up: {
                                rightUp -= floating.HeightPixels + Padding;

                                con.Top = rightUp;
                                con.Left = ScreenWidth - floating.WidthPixels - Padding;
                            } break;

                            // Top right corner, stacking DOWNwards.
                            case Direction.Down: {
                                con.Top = rightDown;
                                con.Left = ScreenWidth - floating.WidthPixels - Padding;

                                rightDown += floating.HeightPixels + Padding;
                            } break;
                        }
                    break;

                    case Edge.Left:
                        switch(floating.StackDirection) {
                            // Bottom left corner, stacking UPwards.
                            case Direction.Up: {
                                leftUp -= floating.HeightPixels + Padding;

                                con.Top = leftUp;
                                con.Left = Padding;
                            } break;

                            // Top left corner, stacking DOWNwards.
                            case Direction.Down: {
                                con.Top = leftDown;
                                con.Left = Padding;

                                leftDown += floating.HeightPixels + Padding;
                            } break;
                        }
                    break;
                }
            }
        }

        void IUpdatable.Update(Master master) {

        }

        void IDrawable.Draw(Master master) {
            // Draw every drawable floating element.
            foreach(IFloating floating in this._floatingElements.Values) {
                if(floating is IFloatingContract drawable)
                    drawable.Draw(master);
            }
        }

        /// <summary>
        /// Allows us to make some properties or methods of public nested functions accessible only within <see cref="GUI"/>.
        /// </summary>
        private interface IFloatingContract
        {
            int Left { get; set; }
            int Top { get; set; }

            GUI GUI { set; }

            void Draw(Master master);
        }

        public enum Edge { Top, Right, Bottom, Left };
        public enum Direction { Up, Right, Down, Left };

        /// <summary>
        /// A GUI element floating against an edge of the screen, not part of any menu.
        /// </summary>
        public interface IFloating {
            Edge Edge { get; }
            Direction StackDirection { get; }

            int WidthPixels { get; }
            int HeightPixels { get; }
        }

        public delegate void OnClick();
        public class FloatingButton : IFloating, IFloatingContract
        {
            const int Padding = 4;

            public string Text { get; }

            public OnClick OnClick { get; }

            public Edge Edge { get; }
            public Direction StackDirection { get; }

            public int WidthPixels => (int)Font.MeasureString(Text).X + Padding*2;
            public int HeightPixels => (int)Font.MeasureString(Text).Y + Padding*2;
            
            public GUI GUI { get; private set; }
            #region Hidden Properties
            GUI IFloatingContract.GUI { set => this.GUI = value; }

            int IFloatingContract.Left { get; set; }
            int IFloatingContract.Top { get; set; }
            #endregion

            public FloatingButton(string text, OnClick onClick, Edge edge, Direction stackDirection) {
                Text = text;
                OnClick = onClick;
                Edge = edge;
                StackDirection = stackDirection;
            }

            void IFloatingContract.Draw(Master master) {

            }
        }
        public class FloatingText : IFloating, IFloatingContract
        {
            private string _text;

            /// <summary>
            /// The string of this <see cref="FloatingText"/>.
            /// </summary>
            public string Text {
                get => this._text;
                set {
                    string old = this._text;
                    this._text = value;

                    // Update the arrangement of floating elements after this property is changed.
                    if(old != value && GUI != null)
                        GUI.ArrangeFloating();
                }
            }

            public Edge Edge { get; }
            public Direction StackDirection { get; }
            
            public int WidthPixels => (int)Font.MeasureString(Text).X;
            public int HeightPixels => (int)Font.MeasureString(Text).Y;

            public GUI GUI { get; private set; }
            #region Hidden properties
            GUI IFloatingContract.GUI { set => this.GUI = value; }

            int IFloatingContract.Left { get; set; }
            int IFloatingContract.Top { get; set; }
            #endregion

            public FloatingText(string text, Edge edge, Direction stackDirection) {
                this._text = text;
                Edge = edge;
                StackDirection = stackDirection;
            }

            void IFloatingContract.Draw(Master master) {
                var pos = new Vector2((this as IFloatingContract).Left, (this as IFloatingContract).Top);
                master.SpriteBatch.DrawString(Font, Text, pos, Color.Black);
            }
        }
    }
}

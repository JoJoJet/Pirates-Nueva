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
                (floating as IFloatingContract).GUI = this; // Set the /GUI/ property of /floating/ to be this GUI object.
                _floatingElements[id] = floating;           // Add /floating/ to the dictionary of floating elements.
                ArrangeFloating(); // Update the arrangement of floating elements after it has been added.
            }
            // If there is already a floating element identified by /id/, throw an InvalidOperationException.
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
            // If there is a floating element identified by /id/, and it is of type /T/,
            // set out parameter /floating/ to be that element, and return true.
            if(TryGetFloating(id, out IFloating med) && med is T last) {
                floating = last;
                return true;
            }
            // If there is no floating element of type /T/ and identified by /id/, return false;
            else {
                floating = default;
                return false;
            }
        }

        /// <summary>
        /// Remove the floating element identifed by /id/, and then return it.
        /// </summary>
        /// <exception cref="KeyNotFoundException">Thrown when there is no <see cref="IFloating"/> to remove.</exception>
        public IFloating RemoveFloating(string id) {
            // If there is a floating element identified by /id/, remove it and then return it.
            if(this._floatingElements.ContainsKey(id)) {
                var floating = this._floatingElements[id]; // Store the current element identifed by /id/.
                this._floatingElements.Remove(id);         // Remove that element from the dictionary.
                return floating;                           // Return the stored element.
            }
            // If there is no floating element identified by /id/, throw a KeyNotFoundException.
            else {
                throw new KeyNotFoundException(
                    $"{nameof(GUI)}.{nameof(RemoveFloating)}(): There is no {nameof(IFloating)} named \"{id}\" to remove!"
                    );
            }
        }
        #endregion

        /// <summary>
        /// Update the arrangement of Floating elements.
        /// </summary>
        void ArrangeFloating() {
            const int Padding = 3;

            Dictionary<(Edge, Direction), int> stackLengths = new Dictionary<(Edge, Direction), int>();

            foreach(IFloating floating in this._floatingElements.Values) {
                if(!(floating is IFloatingContract con))
                    continue;

                // Copy over some commonly used properties of /floating/.
                var (e, d, width, height) = (floating.Edge, floating.StackDirection, floating.WidthPixels, floating.HeightPixels);

                if(stackLengths.ContainsKey((e, d)) == false) // If the stack for /floating/ is unassigned,
                    stackLengths[(e, d)] = Padding;           // make it default to the constant /Padding/.

                
                if(d == Direction.Left || d == Direction.Up) { // If the stack direction is leftwards or upwards,
                    incr();                                    // increment the stack,
                    arrange();                                 // THEN arrange /floating/.
                }
                else {         // If the stack direction if rightwards or downwards,
                    arrange(); // increment the stack,
                    incr();    // THEN arrange /floating/.
                }

                // Arrange /floating/ into its stack.
                void arrange() {
                    // Position it based on the edge it's hugging.
                    if(e == Edge.Top)         // Hugging the top
                        con.Top = Padding;
                    else if(e == Edge.Bottom) // Hugging the bottom
                        con.Top = ScreenHeight - height - Padding;
                    else if(e == Edge.Right)  // Hugging the right
                        con.Left = ScreenWidth - width - Padding;
                    else if(e == Edge.Left)   // Hugging the Left
                        con.Left = Padding;

                    // Position it based on its stack direction
                    if(d == Direction.Up)         // Stacking upwards
                        con.Top = ScreenHeight - stackLengths[(e, d)];
                    else if(d == Direction.Down)  // Stacking downwards
                        con.Top = stackLengths[(e, d)];
                    else if(d == Direction.Right) // Stacking rightwards
                        con.Left = stackLengths[(e, d)];
                    else if(d == Direction.Left)  // Stacking leftwards
                        con.Left = ScreenWidth - stackLengths[(e, d)];
                }

                // Increment the stack length that /floating/ is aligned in.
                void incr() => stackLengths[(e, d)] += (d == Direction.Down || d == Direction.Up ? height : width) + Padding;
            }
        }

        void IUpdatable.Update(Master master) {

        }

        void IDrawable.Draw(Master master) {
            // Draw every drawable floating element.
            foreach(IFloating floating in this._floatingElements.Values) {
                if(floating is IFloatingContract drawable) // If /floating/ implements IFloatingContract,
                    drawable.Draw(master);                 // Call its Draw() method.
            }
        }

        /// <summary>
        /// Allows us to make some properties or methods of public nested functions accessible only within <see cref="GUI"/>.
        /// </summary>
        private interface IFloatingContract
        {
            /// <summary> The position of this element's left edge. </summary>
            int Left { get; set; }
            /// <summary> The position of this element's top edge. </summary>
            int Top { get; set; }

            /// <summary> Sets the floating element's reference to the GUI object. </summary>
            GUI GUI { set; }

            /// <summary> Draws this floating element onscreen. </summary>
            void Draw(Master master);
        }

        public enum Edge { Top, Right, Bottom, Left };
        public enum Direction { Up, Right, Down, Left };

        /// <summary>
        /// A GUI element floating against an edge of the screen, not part of any menu.
        /// </summary>
        public abstract class IFloating {
            /// <summary>  The edge of the screen that this floating element will hug. </summary>
            public virtual Edge Edge { get; }
            /// <summary> The direction that this floating element will stack towards. </summary>
            public virtual Direction StackDirection { get; }

            /// <summary> The width of this floating element, in pixels. </summary>
            public abstract int WidthPixels { get; }
            /// <summary> The height of this floating element, in pixels. </summary>
            public abstract int HeightPixels { get; }

            public IFloating(Edge edge, Direction stackDirection) {
                Edge = edge;
                StackDirection = stackDirection;
            }
        }

        /// <summary>
        /// Action to invoke when a Button is clicked.
        /// </summary>
        public delegate void OnClick();
        /// <summary>
        /// A button that floats along the edge of a screen, not tied to any menu.
        /// </summary>
        public class FloatingButton : IFloating, IFloatingContract
        {
            const int Padding = 4;

            /// <summary> Text to display on this <see cref="FloatingButton"/>. </summary>
            public string Text { get; }

            /// <summary> Action to invoke when this <see cref="FloatingButton"/> is clicked. </summary>
            public OnClick OnClick { get; }

            /// <summary> The width of this <see cref="FloatingButton"/>, in pixels. </summary>
            public override int WidthPixels => (int)Font.MeasureString(Text).X + Padding*2;
            /// <summary> The width of this <see cref="FloatingButton"/>, in pixels. </summary>
            public override int HeightPixels => (int)Font.MeasureString(Text).Y + Padding*2;
            
            /// <summary> The <see cref="Pirates_Nueva.GUI"/> that contains this <see cref="FloatingButton"/>. </summary>
            public GUI GUI { get; private set; }
            #region Hidden Properties
            GUI IFloatingContract.GUI { set => this.GUI = value; }

            int IFloatingContract.Left { get; set; }
            int IFloatingContract.Top { get; set; }
            #endregion

            public FloatingButton(string text, OnClick onClick, Edge edge, Direction direction) : base(edge, direction) {
                Text = text;
                OnClick = onClick;
            }

            void IFloatingContract.Draw(Master master) {

            }
        }
        /// <summary>
        /// A bit of text that floats along the edge of a screen, not tied to any menu.
        /// </summary>
        public class FloatingText : IFloating, IFloatingContract
        {
            private string _text;

            /// <summary> The string of this <see cref="FloatingText"/>. </summary>
            public string Text {
                get => this._text;
                set {
                    string old = this._text; // Store the old value of Text.
                    this._text = value;      // Set the new value of Text.
                    
                    if(old != value && GUI != null) // If the value of Text has changed,
                        GUI.ArrangeFloating();      // update the arrangement of floating elements in GUI.
                }
            }
            
            /// <summary> The width of this <see cref="FloatingText"/>, in pixels. </summary>
            public override int WidthPixels => (int)Font.MeasureString(Text).X;
            /// <summary> The height of this <see cref="FloatingText"/>, in pixels. </summary>
            public override int HeightPixels => (int)Font.MeasureString(Text).Y;

            /// <summary> The <see cref="Pirates_Nueva.GUI"/> object that contains this <see cref="FloatingText"/>. </summary>
            public GUI GUI { get; private set; }
            #region Hidden properties
            GUI IFloatingContract.GUI { set => this.GUI = value; }

            int IFloatingContract.Left { get; set; }
            int IFloatingContract.Top { get; set; }
            #endregion

            public FloatingText(string text, Edge edge, Direction direction) : base(edge, direction) {
                this._text = text;
            }

            void IFloatingContract.Draw(Master master) {
                var pos = new Vector2((this as IFloatingContract).Left, (this as IFloatingContract).Top);
                master.SpriteBatch.DrawString(Font, Text, pos, Color.Black);
            }
        }
    }
}

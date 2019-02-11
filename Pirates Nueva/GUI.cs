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
        private Dictionary<string, EdgeElement> _edgeElements = new Dictionary<string, EdgeElement>();
        private Dictionary<string, Menu> _menus = new Dictionary<string, Menu>();

        public static Master Master { get; private set; }

        static SpriteFont Font => Master.Font;

        public int ScreenWidth => Master.GraphicsDevice.Viewport.Width;
        public int ScreenHeight => Master.GraphicsDevice.Viewport.Height;

        internal GUI(Master master) {
            Master = master;
        }
        
        #region Floating Accessors
        /// <summary>
        /// Add the indicated edge element to the GUI.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if there is already a floating element identified by /id/.</exception>
        public void AddEdge(string id, EdgeElement floating) {
            if(_edgeElements.ContainsKey(id) == false) {
                (floating as IEdgeContract).GUI = this; // Set the /GUI/ property of /floating/ to be this GUI object.
                _edgeElements[id] = floating;           // Add /floating/ to the dictionary of floating elements.
                ArangeEdges(); // Update the arrangement of floating elements after it has been added.
            }
            // If there is already a floating element identified by /id/, throw an InvalidOperationException.
            else {
                throw new InvalidOperationException(
                    $"{nameof(GUI)}.{nameof(AddEdge)}(): There is already a floating element named \"{id}\"!"
                    );
            }
        }

        /// <summary>
        /// Tries to get the edge element identified by /id/, and returns whether or not it was successful.
        /// If it was successful, stuffs that value into /edge/.
        /// </summary>
        public bool TryGetEdge(string id, out EdgeElement edge) => this._edgeElements.TryGetValue(id, out edge);

        /// <summary>
        /// Tries to get the edge element identified by /id/, and returns whether or not it was successful.
        /// If it was successful, stuffs that value into /edge/.
        /// </summary>
        public bool TryGetEdge<T>(string id, out T edge) where T : EdgeElement {
            // If there is a floating element identified by /id/, and it is of type /T/,
            // set out parameter /floating/ to be that element, and return true.
            if(TryGetEdge(id, out EdgeElement med) && med is T last) {
                edge = last;
                return true;
            }
            // If there is no floating element of type /T/ and identified by /id/, return false;
            else {
                edge = default;
                return false;
            }
        }

        /// <summary>
        /// Whether or not there is a edge element identified by /id/.
        /// </summary>
        public bool HasEdge(string id) => this._edgeElements.ContainsKey(id);

        /// <summary>
        /// Remove the edge element identifed by /id/, and then return it.
        /// </summary>
        /// <exception cref="KeyNotFoundException">Thrown when there is no <see cref="EdgeElement"/> to remove.</exception>
        public void RemoveEdge(string id) {
            if(this._edgeElements.ContainsKey(id)) { // If there is an edge element identifed by /id/,
                this._edgeElements.Remove(id);     // Remove that element from the dictionary.
                ArangeEdges();                     // Update the arrangement of edge elements.
            }
            // If there is no edge element identified by /id/, throw a KeyNotFoundException.
            else {
                throw new KeyNotFoundException(
                    $"{nameof(GUI)}.{nameof(RemoveEdge)}(): There is no {nameof(EdgeElement)} named \"{id}\" to remove!"
                    );
            }
        }
        #endregion

        #region Menu Accessors
        /// <summary>
        /// Add the menu to the screen.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if there is already a menu identified by /id/.</exception>
        public void AddMenu(string id, Menu menu) {
            if(this._menus.ContainsKey(id) == false)
                this._menus[id] = menu;
            else
                throw new InvalidOperationException(
                    $"{nameof(GUI)}.{nameof(AddMenu)}(): There is already a {nameof(Menu)} identified by string \"{id}\"!""
                    );
        }

        /// <summary>
        /// Whether or not there is a <see cref="Menu"/> identified by /id/.
        /// </summary>
        public bool HasMenu(string id) => this._menus.ContainsKey(id);

        /// <summary>
        /// Get the <see cref="Menu"/> identified by /id/.
        /// </summary>
        public bool TryGetMenu(string id, out Menu menu) => this._menus.TryGetValue(id, out menu);

        /// <summary>
        /// Remove the <see cref="Menu"/> identified by /id/.
        /// </summary>
        /// <exception cref="KeyNotFoundException">Thrown if there is no <see cref="Menu"/> identified by /id/.</exception>
        public void RemoveMenu(string id) {
            if(this._menus.ContainsKey(id)) // If there is a menu keyed with /id/,
                this._menus.Remove(id);     // Remove it from the dictionary.
            else
                // If there is no menu identifed by /id/, throw a KeyNotFoundException.
                throw new KeyNotFoundException(
                    $"{nameof(GUI)}.{nameof(RemoveMenu)}(): There is no {nameof(Menu)} identified by \"{id}\"!"
                    );
        }
        #endregion

        /// <summary>
        /// Update the arrangement of edge elements.
        /// </summary>
        void ArangeEdges() {
            const int Padding = 5;

            Dictionary<(Edge, Direction), int> stackLengths = new Dictionary<(Edge, Direction), int>();

            foreach(EdgeElement floating in this._edgeElements.Values) {
                if(!(floating is IEdgeContract con))
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
            // Exit the method if the mouse wasn't clicked this frame.
            if(!master.Input.MouseLeft.IsDown)
                return;

            var (mouseX, mouseY) = master.Input.MousePosition;
            foreach(EdgeElement edge in this._edgeElements.Values) {            // For every edge element:
                if(edge is IButtonContract b && b.IsMouseOver(mouseX, mouseY))  // If the element is a button and the mouse is over it,
                    b.OnClick();                                                // invoke its action.
            }
        }

        void IDrawable.Draw(Master master) {
            // Draw every drawable floating element.
            foreach(EdgeElement edge in this._edgeElements.Values) {
                if(edge is IEdgeContract drawable) // If /edge/ implements IEdgeContract,
                    drawable.Draw(master);         // Call its Draw() method.
            }
        }

        /// <summary>
        /// Allows us to make some properties or methods of public nested functions accessible only within <see cref="GUI"/>.
        /// </summary>
        private interface IEdgeContract
        {
            /// <summary> The position of this element's left edge. </summary>
            int Left { get; set; }
            /// <summary> The position of this element's top edge. </summary>
            int Top { get; set; }

            /// <summary> Sets the edge element's reference to the GUI object. </summary>
            GUI GUI { set; }

            /// <summary> Draws this edge element onscreen. </summary>
            void Draw(Master master);
        }

        public enum Edge { Top, Right, Bottom, Left };
        public enum Direction { Up, Right, Down, Left };

        /// <summary>
        /// A GUI element hugging an edge of the screen, not part of any menu.
        /// </summary>
        public abstract class EdgeElement {
            /// <summary>  The edge of the screen that this element will hug. </summary>
            public virtual Edge Edge { get; }
            /// <summary> The direction that this edge element will stack towards. </summary>
            public virtual Direction StackDirection { get; }

            /// <summary> The width of this edge element, in pixels. </summary>
            public abstract int WidthPixels { get; }
            /// <summary> The height of this edge element, in pixels. </summary>
            public abstract int HeightPixels { get; }

            public EdgeElement(Edge edge, Direction stackDirection) {
                Edge = edge;
                StackDirection = stackDirection;
            }
        }

        /// <summary>
        /// A bit of text that hugs an edge of the screen, not tied to any menu.
        /// </summary>
        public class EdgeText : EdgeElement, IEdgeContract
        {
            private string _text;

            /// <summary> The string of this <see cref="EdgeText"/>. </summary>
            public string Text {
                get => this._text;
                set {
                    string old = this._text; // Store the old value of Text.
                    this._text = value;      // Set the new value of Text.
                    
                    if(old != value && GUI != null) // If the value of Text has changed,
                        GUI.ArangeEdges();          // update the arrangement of floating elements in GUI.
                }
            }
            
            /// <summary> The width of this <see cref="EdgeText"/>, in pixels. </summary>
            public override int WidthPixels => (int)Font.MeasureString(Text).X;
            /// <summary> The height of this <see cref="EdgeText"/>, in pixels. </summary>
            public override int HeightPixels => (int)Font.MeasureString(Text).Y;

            /// <summary> The <see cref="Pirates_Nueva.GUI"/> object that contains this <see cref="EdgeText"/>. </summary>
            public GUI GUI { get; private set; }
            #region Hidden properties
            GUI IEdgeContract.GUI { set => this.GUI = value; }

            int IEdgeContract.Left { get; set; }
            int IEdgeContract.Top { get; set; }
            #endregion

            public EdgeText(string text, Edge edge, Direction direction) : base(edge, direction) {
                this._text = text;
            }

            void IEdgeContract.Draw(Master master) {
                var pos = new Vector2((this as IEdgeContract).Left, (this as IEdgeContract).Top);
                master.SpriteBatch.DrawString(Font, Text, pos, Color.Black);
            }
        }
        /// <summary>
        /// Action to invoke when a Button is clicked.
        /// </summary>
        public delegate void OnClick();

        /// <summary>
        /// Allows us to make some properties or methods of public nested functions accessible only within <see cref="GUI"/>.
        /// </summary>
        private interface IButtonContract
        {
            /// <summary> Action to invoke when this button is clicked. </summary>
            OnClick OnClick { get; }

            /// <summary> Whether or not the input coordinates are hovering over this button. </summary>
            bool IsMouseOver(int mouseX, int mouseY);
        }

        /// <summary>
        /// A button that hugs an edge of the screen, not tied to any menu.
        /// </summary>
        public class EdgeButton : EdgeElement, IEdgeContract, IButtonContract
        {
            const int Padding = 3;

            /// <summary> Text to display on this <see cref="EdgeButton"/>. </summary>
            public string Text { get; }

            /// <summary> The width of this <see cref="EdgeButton"/>, in pixels. </summary>
            public override int WidthPixels => (int)Font.MeasureString(Text).X + Padding*2;
            /// <summary> The width of this <see cref="EdgeButton"/>, in pixels. </summary>
            public override int HeightPixels => (int)Font.MeasureString(Text).Y + Padding*2;

            /// <summary> The <see cref="Pirates_Nueva.GUI"/> that contains this <see cref="EdgeButton"/>. </summary>
            public GUI GUI { get; private set; }
            #region Hidden Properties
            GUI IEdgeContract.GUI { set => this.GUI = value; }

            int IEdgeContract.Left { get; set; }
            int IEdgeContract.Top { get; set; }

            private OnClick _onClick;
            OnClick IButtonContract.OnClick => this._onClick;
            #endregion

            public EdgeButton(string text, OnClick onClick, Edge edge, Direction direction) : base(edge, direction) {
                Text = text;
                this._onClick = onClick;
            }

            void IEdgeContract.Draw(Master master) {
                var pos = new Vector2((this as IEdgeContract).Left, (this as IEdgeContract).Top);
                pos += new Vector2(Padding, Padding);

                master.SpriteBatch.DrawString(Font, Text, pos, Color.Green);
            }

            bool IButtonContract.IsMouseOver(int mouseX, int mouseY) {
                IEdgeContract f = this as IEdgeContract;
                return new Rectangle(f.Left, f.Top, WidthPixels, HeightPixels).Contains(mouseX, mouseY);
            }
        }

        /// <summary>
        /// A base class for different types of menus.
        /// </summary>
        public abstract class Menu
        {
            protected MenuElement[] Elements { get; set; }

            public Menu(MenuElement[] elements) {
                const int Padding = 3;
                
                int elementsLeft = Padding; // The length of the row of MenuElements.
                foreach(MenuElement el in elements) {
                    var con = el as IMenuToElementContract;

                    con.Menu = this; // Set the element's parent menu to be /this/.

                    if(con.NullablePosition == null) {                  // If the element's position is uninitialied,
                        con.NullablePosition = (elementsLeft, Padding); // Put it the the furthest right position in the row.

                        elementsLeft += el.WidthPixels + Padding;       // Increment the length of the row by the width of the element.
                    }
                }
            }
        }

        private interface IMenuToElementContract
        {
            (int Left, int Top)? NullablePosition { get; set; }
            Menu Menu { get; set; }
        }
        /// <summary>
        /// An element (text, button, slider, etc.) in a menu.
        /// </summary>
        public abstract class MenuElement : IMenuToElementContract
        {
            /// <summary> The position of the left edge of this element local to its menu. </summary>
            public int Left => Pos.Left;
            /// <summary> The position of the top edge of this element local to its menu. </summary>
            public int Top => Pos.Top;

            /// <summary> The width of this element, in pixels. </summary>
            public abstract int WidthPixels { get; }
            /// <summary> The height of this element, in pixels. </summary>
            public abstract int HeightPixels { get; }

            protected Menu Menu { get; private set; }
            #region Hidden Properties
            private (int Left, int Top) Pos {
                get => (this as IMenuToElementContract).NullablePosition.Value;
                set => (this as IMenuToElementContract).NullablePosition = value;
            }
            (int Left, int Top)? IMenuToElementContract.NullablePosition { get; set; }

            Menu IMenuToElementContract.Menu { get => Menu; set => Menu = value; }
            #endregion

            public MenuElement() {  }
            public MenuElement(int x, int y) {
                Pos = (x, y);
            }
        }

        public class MenuText : MenuElement
        {
            public string Text { get; }

            public override int WidthPixels => (int)Font.MeasureString(Text).X;
            public override int HeightPixels => (int)Font.MeasureString(Text).Y;

            public MenuText(string text) {
                Text = text;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Pirates_Nueva.UI;

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
        
        #region Edge Accessors
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
                    $"{nameof(GUI)}.{nameof(AddMenu)}(): There is already a {nameof(Menu)} identified by string \"{id}\"!"
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

            var mouse = master.Input.MousePosition;
            foreach(EdgeElement edge in this._edgeElements.Values) {       // For every edge element:
                var con = edge as IEdgeContract;
                if(edge is IButtonContract b && edge is IElementDrawable d // If the element is a button,
                    && d.IsMouseOver(mouse)) {                             // and the mouse is over it,
                    b.OnClick();                                           // invoke its action.

                    return; // Exit this method.
                }
            }

            foreach(Menu menu in this._menus.Values) {
                (menu as IMenuContract).QueryClicks(mouse);
            }
        }

        void IDrawable.Draw(Master master) {
            // Draw every drawable floating element.
            foreach(EdgeElement edge in this._edgeElements.Values) {
                var con = edge as IEdgeContract;
                if(edge is IElementDrawable drawable)         // If /edge/ implements IEdgeDrawable,
                    drawable.Draw(master, con.Left, con.Top); // Call its Draw() method.
            }
            
            // Draw every menu.
            foreach(Menu menu in this._menus.Values) {
                (menu as IMenuContract).Draw(master);
            }
        }


        /// <summary>
        /// Action to invoke when a Button is clicked.
        /// </summary>
        public delegate void OnClick();
        /// <summary>
        /// Makes the OnClick property of a Button accessible only through this assembly.
        /// </summary>
        internal interface IButtonContract
        {
            /// <summary> Action to invoke when this button is clicked. </summary>
            OnClick OnClick { get; }
        }

        /// <summary>
        /// Makes the Draw() and IsMouseOver() method elements accessible only within <see cref="GUI"/>.
        /// </summary>
        private interface IElementDrawable
        {
            /// <summary> Draws this element onscreen, from the specified top left corner. </summary>
            void Draw(Master master, int left, int top);

            /// <summary> Whether or not the mouse is hovering over this element, measuring from the specified top left corner. </summary>
            bool IsMouseOver(PointI mouse);
        }
        /// <summary>
        /// A GUI element onscreen.
        /// </summary>
        public abstract class Element : IElementDrawable
        {
            /// <summary> The width of this element, in pixels. </summary>
            public abstract int WidthPixels { get; }
            /// <summary> The height of this element, in pixels. </summary>
            public abstract int HeightPixels { get; }
            
            private Rectangle? Bounds { get; set; } // The extents of this element. Used in IsMouseOver().

            internal Element() {  } // Ensures that /Element/ can only be descended from within this Assembly.

            /// <summary>
            /// Draw this <see cref="Element"/> onscreen, from the specified top left corner.
            /// </summary>
            protected abstract void Draw(Master master, int left, int top);
            
            void IElementDrawable.Draw(Master master, int left, int top) {
                Draw(master, left, top);                                       // Have the subclass draw the button onscreen.
                Bounds = new Rectangle(left, top, WidthPixels, HeightPixels);  // Store the current bounds of this element
                                                                               //     for use in IsMouseOver() at a later time.
            }
            bool IElementDrawable.IsMouseOver(PointI mouse) => Bounds?.Contains(mouse) == true;
        }


        public enum Edge { Top, Right, Bottom, Left };
        public enum Direction { Up, Right, Down, Left };
        /// <summary>
        /// Makes some properties of an EdgeElement accessible only within <see cref="Pirates_Nueva.GUI"/>.
        /// </summary>
        private interface IEdgeContract
        {
            /// <summary> The position of this element's left edge. </summary>
            int Left { get; set; }
            /// <summary> The position of this element's top edge. </summary>
            int Top { get; set; }

            /// <summary> Sets the edge element's reference to the GUI object. </summary>
            GUI GUI { set; }
        }
        /// <summary>
        /// A GUI element hugging an edge of the screen, not part of any menu.
        /// </summary>
        public abstract class EdgeElement : Element, IEdgeContract
        {
            /// <summary>  The edge of the screen that this element will hug. </summary>
            public virtual Edge Edge { get; }
            /// <summary> The direction that this edge element will stack towards. </summary>
            public virtual Direction StackDirection { get; }

            #region Hidden Properties
            private GUI GUI { get; set; }
            GUI IEdgeContract.GUI { set => GUI = value; }

            int IEdgeContract.Left { get; set; }
            int IEdgeContract.Top { get; set; }
            #endregion

            public EdgeElement(Edge edge, Direction stackDirection) {
                Edge = edge;
                StackDirection = stackDirection;
            }

            /// <summary> Call this when a property is changed after initialization. </summary>
            protected void PropertyChanged() => GUI?.ArangeEdges();
        }

        
        /// <summary>
        /// Makes the Draw() method of a <see cref="Menu"/> accessible only within <see cref="GUI"/>.
        /// </summary>
        private interface IMenuContract
        {
            void Draw(Master master);

            /// <summary> If /mouse/ is hovering over any buttons, invoke its action and return true. </summary>
            bool QueryClicks(PointI mouse);
        }
        /// <summary>
        /// A base class for different types of menus.
        /// </summary>
        public abstract class Menu : IMenuContract
        {
            protected const int Padding = 3;

            protected MenuElement[] Elements { get; set; }

            public Menu(MenuElement[] elements) {
                
                int elementsLeft = Padding; // The length of the row of MenuElements.
                foreach(MenuElement el in elements) {
                    var con = el as IMenuToElementContract;

                    con.Menu = this; // Set the element's parent menu to be /this/.

                    if(con.NullablePosition == null) {                  // If the element's position is uninitialied,
                        con.NullablePosition = (elementsLeft, Padding); // Put it the the furthest right position in the row.

                        elementsLeft += el.WidthPixels + Padding;       // Increment the length of the row by the width of the element.
                    }
                }

                Elements = elements;
            }

            void IMenuContract.Draw(Master master) => Draw(master);
            protected abstract void Draw(Master master);

            protected void DrawElement(Master master, MenuElement element, int left, int top) {
                (element as IElementDrawable).Draw(master, left, top);
            }

            bool IMenuContract.QueryClicks(PointI mouse) {
                foreach(MenuElement el in Elements) {                      // For every element in this menu:
                    if(el is IButtonContract b && el is IElementDrawable d // If the element is a button,
                        && d.IsMouseOver(mouse)) {                         // and the mouse is hovering over it,
                        b.OnClick.Invoke();                                // invoke its action,
                        return true;                                       // and return true.
                    }
                }
                return false; // If we got this far without exiting the method, return false.
            }
        }

        /// <summary>
        /// Makes some properties of a <see cref="MenuElement"/> accessible only with <see cref="GUI"/>.
        /// </summary>
        private interface IMenuToElementContract
        {
            (int Left, int Top)? NullablePosition { get; set; }
            Menu Menu { set; }
        }
        /// <summary>
        /// An element (text, button, slider, etc.) in a menu.
        /// </summary>
        public abstract class MenuElement : Element, IMenuToElementContract
        {
            /// <summary> The position of the left edge of this element local to its menu. </summary>
            public int Left => Pos.Value.Left;
            /// <summary> The position of the top edge of this element local to its menu. </summary>
            public int Top => Pos.Value.Top;

            protected Menu Menu { get; private set; }
            #region Hidden Properties
            private (int Left, int Top)? Pos { get; set; }
            (int Left, int Top)? IMenuToElementContract.NullablePosition { get => Pos; set => Pos = value; }

            Menu IMenuToElementContract.Menu { set => Menu = value; }
            #endregion

            /// <summary> Create a <see cref="MenuElement"/>, allowing its <see cref="GUI.Menu"/> to position it. </summary>
            public MenuElement() {  }
            /// <summary> Create a <see cref="MenuElement"/>, with the specified position. </summary>
            public MenuElement(int left, int top) {
                Pos = (left, top);
            }
        }
    }
}

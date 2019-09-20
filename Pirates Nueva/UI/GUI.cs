using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;

namespace Pirates_Nueva.UI
{
    public enum Edge { Top, Right, Bottom, Left };
    public enum Direction { Up, Right, Down, Left };

    /// <summary>
    /// Controls the user interface for <see cref="Pirates_Nueva"/>.
    /// </summary>
    public class GUI : IUpdatable
    {
        private struct EdgeInfo {
            public Edge Edge { get; set; }
            public Direction Dir { get; set; }
            public Element<Edge> Element { get; set; }
        }

        private readonly Dictionary<string, EdgeInfo> _edgeElements = new Dictionary<string, EdgeInfo>();
        private readonly Dictionary<string, Menu> _menus = new Dictionary<string, Menu>();

        public Master Master { get; private set; }

        public string Tooltip { get; set; }

        private Font Font => Master.Font;

        internal GUI(Master master) {
            Master = master;
            Tooltip = "";
        }

        /// <summary> Whether or not the mouse is currently hovering over any GUI elements. </summary>
        public bool IsMouseOverGUI => IsPointOverGUI(Master.Input.MousePosition);
        
        #region Edge Accessors
        /// <summary>
        /// Adds the specified edge element to the GUI.
        /// </summary>
        /// <param name="edge">The edge of the screen that the element will hug.</param>
        /// <param name="dir">The direction that the element will stack towards.</param>
        /// <exception cref="InvalidOperationException">Thrown if there is already an edge element identified by /id/.</exception>
        public void AddEdge(string id, Edge edge, Direction dir, Element<Edge> element) {
            if(this._edgeElements.ContainsKey(id) == false) {
                (element as IElement<Edge>).SubscribeOnPropertyChanged(() => ArrangeEdges());
                this._edgeElements[id] = new EdgeInfo() { Edge = edge, Dir = dir, Element = element };
                ArrangeEdges();
            }
            //
            // If there is already a floating element identified by /id/, throw an InvalidOperationException.
            else {
                throw new InvalidOperationException(
                    $"{nameof(GUI)}.{nameof(AddEdge)}(): There is already a floating element named \"{id}\"!"
                    );
            }
        }

        /// <summary> Gets the edge Element identifed by /id/. </summary>
        public bool TryGetEdge(string id, [NotNullWhen(true)] out Element<Edge>? element) {
            if(this._edgeElements.TryGetValue(id, out var info)) {
                element = info.Element;
                return true;
            }
            else {
                element = null;
                return false;
            }
        }
        /// <summary> Gets the edge element identified by /id/ and that is of type /T/. </summary>
        public bool TryGetEdge<T>(string id, [NotNullWhen(true)] out T? element) where T : Element<Edge> {
            if(TryGetEdge(id, out var med) && med is T last) // If there's an edge of type /T/ identified by /id/,
                element = last;                              //     return it.
            else                                             // If there is no edge element of type /T/ and identified by /id/,
                element = null;                              //     return false;
                                                             //
            return element != default;                       // Return whether or not we found /edge/.
        }

        /// <summary> Whether or not there is a edge element identified by /id/. </summary>
        public bool HasEdge(string id) => this._edgeElements.ContainsKey(id);

        /// <summary>
        /// Remove the edge element identifed by /id/.
        /// </summary>
        /// <exception cref="KeyNotFoundException">Thrown when there is no edge Element to remove.</exception>
        public void RemoveEdge(string id) {
            if(this._edgeElements.ContainsKey(id)) { // If there is an edge element identifed by /id/,
                this._edgeElements.Remove(id);     // Remove that element from the dictionary.
                ArrangeEdges();                     // Update the arrangement of edge elements.
            }
            // If there is no edge element identified by /id/, throw a KeyNotFoundException.
            else {
                throw new KeyNotFoundException(
                    $"{nameof(GUI)}.{nameof(RemoveEdge)}(): There is no {nameof(Element<Edge>)} named \"{id}\" to remove!"
                    );
            }
        }
        /// <summary>
        /// Removes every edge element identified by the specified strings.
        /// </summary>
        /// <exception cref="KeyNotFoundException">Thrown when one of the strings does not identify an existing edge Element.</exception>
        public void RemoveEdges(params string[] ids) {
            foreach(var id in ids) {
                RemoveEdge(id);
            }
        }
        #endregion

        #region Menu Accessors
        /// <summary>
        /// Add the menu to the screen.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if there is already a menu identified by /id/.</exception>
        public void AddMenu(string id, Menu menu) {
            if(this._menus.ContainsKey(id) == false) {
                this._menus[id] = menu;
                (menu as IMenuContract).Screen = Master.Screen;
            }
            else
                throw new InvalidOperationException(
                    $"{nameof(GUI)}.{nameof(AddMenu)}(): There is already a {nameof(Menu)} identified by string \"{id}\"!"
                    );
        }

        /// <summary> Whether or not there is a <see cref="Menu"/> identified by /id/. </summary>
        public bool HasMenu(string id) => this._menus.ContainsKey(id);

        /// <summary> Get the <see cref="Menu"/> identified by /id/. </summary>
        public bool TryGetMenu(string id, [NotNullWhen(true)] out Menu? menu) => this._menus.TryGetValue(id, out menu);

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

        /// <summary> Returns whether or not the specified point is over any GUI elements. </summary>
        public bool IsPointOverGUI(PointI point) {
            foreach(var info in this._edgeElements.Values) {            // For every edge element:
                if((info.Element as IElement<Edge>).IsMouseOver(point)) // If the point is hovering over it,
                    return true;                                        //     return true.
            }                                                           //
                                                                        //
            foreach(var menu in this._menus.Values) {                   // For every menu:
                if((menu as IMenuContract).IsMouseOver(point))          // If the point is hovering over it,
                    return true;                                        //     return true.
            }                                                           //
                                                                        //
            return false;                                               // If we got this far without returning, return false.
        }

        /// <summary> Update the arrangement of edge elements. </summary>
        void ArrangeEdges() {
            const int Padding = 5;

            Dictionary<(Edge, Direction), int> stackLengths = new Dictionary<(Edge, Direction), int>();

            foreach(var info in this._edgeElements.Values) {
                var el = info.Element;
                var con = el as IElement<Edge>;

                // Copy over some commonly used properties of /edge/.
                var (e, d, width, height) = (info.Edge, info.Dir, el.Width, el.Height);

                if(stackLengths.ContainsKey((e, d)) == false) // If the stack for /edge/ is unassigned,
                    stackLengths[(e, d)] = Padding;           // make it default to the constant /Padding/.
                
                if(d == Direction.Left || d == Direction.Up) // If the direction is 'left' or 'up',
                    incr();                                  // increment the stack.

                /* Arrange /edge/ into its stack. */
                    if(e == Edge.Top || e == Edge.Bottom) // Position it based on the edge it's hugging.
                        con.Top = e == Edge.Top ? Padding : Master.Screen.Height - height - Padding;
                    else
                        con.Left = e == Edge.Right ? Master.Screen.Width - width - Padding : Padding;
                
                    if(d == Direction.Up || d == Direction.Down) // Position it based on its stack direction
                        con.Top = d == Direction.Up ? Master.Screen.Height - stackLengths[(e, d)] : stackLengths[(e, d)];
                    else
                        con.Left = d == Direction.Right ? stackLengths[(e, d)] : Master.Screen.Width - stackLengths[(e, d)];

                if(d == Direction.Right || d == Direction.Down) // If the direction is 'right' or 'down',
                    incr();                                     // increment the stack.

                // Increment the stack length that /edge/ is aligned in.
                void incr() => stackLengths[(e, d)] += (d == Direction.Down || d == Direction.Up ? height : width) + Padding;
            }
        }

        void IUpdatable.Update(in UpdateParams @params) {
            var input = Master.Input;
            if(!input.MouseLeft.IsDown) // If the mouse was NOT clicked this frame,
                return;                        //     exit the method.

            var mouse = input.MousePosition;
            foreach(var info in this._edgeElements.Values) {    // For every edge element:
                var edge = info.Element as IElement<Edge>;
                if(edge is IButton b && edge.IsMouseOver(mouse)) {   // If the element is a button and the mouse is over it,
                    b.OnClick();                                     //     invoke its action,
                    return;                                          //     and exit this method.
                }
            }

            foreach(IMenuContract menu in this._menus.Values) { // For every menu:
                if(menu.GetButton(mouse, out var button)) {     //     Query it. If there's a button under the mouse,
                    button.OnClick();                           //         invoke its action,
                    break;                                      //         and stop querying.
                }
            }
        }

        internal void Draw(Master master) {
            //
            // Draw every edge element.
            foreach(var info in this._edgeElements.Values) {
                var edge = info.Element as IElement<Edge>;
                edge.Draw(master.Renderer, master);
            }
            
            // Draw every menu.
            foreach(IMenuContract menu in this._menus.Values) {
                menu.Draw(master.Renderer, master);
            }

            // Draw the tooltip.
            if(!string.IsNullOrEmpty(Tooltip)) {                                 // If there is a tooltip:
                var (x, y) = master.Input.MousePosition;                         //
                master.Renderer.DrawString(Font, Tooltip, x, y, in Color.Black); //     Draw it next to the mouse cursor.
            }
        }


        /// <summary>
        /// Action to invoke when a Button is clicked.
        /// </summary>
        public delegate void OnClick();
        /// <summary>
        /// An Element that can be pressed.
        /// </summary>
        public interface IButton
        {
            /// <summary> Action to invoke when this button is clicked. </summary>
            OnClick OnClick { get; }
        }

        /// <summary>
        /// Makes the Draw() and IsMouseOver() method elements accessible only within <see cref="GUI"/>.
        /// </summary>
        private interface IElement<T>
        {
            int Left { set; }
            int Top { set; }

            bool IsHidden { set; }

            /// <summary> Signs up the specified <see cref="Action"/> to be called when a property changes. </summary>
            void SubscribeOnPropertyChanged(Action action);

            /// <summary> Draws this element onscreen, using the offset. </summary>
            void Draw<TDrawer>(in TDrawer drawer, Master master)
                where TDrawer : ILocalDrawer<T>;

            /// <summary> Whether or not the mouse is hovering over this element, local to the element's containing menu. </summary>
            bool IsMouseOver(PointF localMouse);
        }
        /// <summary>
        /// A GUI element onscreen.
        /// </summary>
        /// <typeparam name="T">The type of object that will be drawing this element.</typeparam>
        public abstract class Element<T> : IElement<T>
        {
            protected Action? onPropertyChanged;

            /// <summary> The local position of this element's left edge. </summary>
            public int Left { get; private set; }
            /// <summary> The local position of this element's top edge. </summary>
            public int Top { get; private set; }

            int IElement<T>.Left { set => Left = value; }
            int IElement<T>.Top { set => Top = value; }

            /// <summary> The width of this element, in units local to its container. </summary>
            public abstract int Width { get; }
            /// <summary> The height of this element, in units local to its container. </summary>
            public abstract int Height { get; }
            
            private Rectangle? Bounds { get; set; } // The extents of this element. Used in IsMouseOver().

            void IElement<T>.SubscribeOnPropertyChanged(Action action) => this.onPropertyChanged += action;

            /// <summary> Draw this <see cref="Element"/> onscreen, from the specified top left corner. </summary>
            protected abstract void Draw<TDrawer>(in TDrawer drawer, Master master)
                where TDrawer : ILocalDrawer<T>;
            
            private bool IsHidden { get; set; }
            bool IElement<T>.IsHidden { set => IsHidden = value; }

            void IElement<T>.Draw<TDrawer>(in TDrawer drawer, Master master) {
                if(!IsHidden) {
                    Draw(in drawer, master);                          // Draw the button onscreen.
                    Bounds = new Rectangle(Left, Top, Width, Height); // Store the bounds of this element
                                                                      //     for later use in IsMouseOver().
                }
                else {             // If the element is hidden,
                    Bounds = null; //     don't draw it, and set the bounds to null.
                }
            }
            bool IElement<T>.IsMouseOver(PointF localMouse) => Bounds?.Contains(localMouse) ?? false;

            /// <summary> Call this when a property is changed after initialization. </summary>
            protected void PropertyChanged() => this.onPropertyChanged?.Invoke();
        }

        
        /// <summary>
        /// Makes some members of a <see cref="Menu"/> accessible only within the <see cref="UI.GUI"/> class.
        /// </summary>
        private interface IMenuContract
        {
            Screen Screen { set; }

            void Draw<TDrawer>(in TDrawer drawer, Master master)
                where TDrawer : ILocalDrawer<Screen>;

            bool IsMouseOver(PointI mouse);

            /// <summary> Returns the <see cref="IButton"/> under /mouse/, if it exists. </summary>
            bool GetButton(PointI mouse, out IButton button);
        }
        /// <summary>
        /// A base class for different types of menus.
        /// </summary>
        public abstract class Menu : IMenuContract
        {
            /// <summary> The default spacing between <see cref="Element"/>s. </summary>
            protected const int Padding = 3;

            private Screen? screen;

            /// <summary>
            /// The <see cref="Pirates_Nueva.Screen"/> on which the <see cref="GUI"/> elements are located.
            /// </summary>
            protected Screen Screen => this.screen ?? NullableUtil.ThrowNotInitialized<Screen>();
            Screen IMenuContract.Screen { set => this.screen = value; }

            /// <summary> Every <see cref="MenuElement"/> in this <see cref="Menu"/>. </summary>
            protected Element<Menu>[] Elements { get; set; }

            public Menu(Element<Menu>[] elements) {
                
                foreach(IElement<Menu> el in elements)
                    el.SubscribeOnPropertyChanged(arrange); // Rearrange the elements when a property changes.

                Elements = elements;

                arrange();

                void arrange() {
                    int elementsLeft = Padding;                     // The length of the row of elements.
                    foreach(var el in Elements) {                   // For every element:
                        (el as IElement<Menu>).Left = elementsLeft; // Put it in the furthest-right position in the menu,
                        (el as IElement<Menu>).Top = Padding;       // put padding above it,
                        elementsLeft += el.Width + Padding;         // and increment the length of the row by the element's width.
                    }
                }
            }

            /// <summary> Hides this <see cref="Menu"/> next frame. </summary>
            public void Hide() => SetIsHidden(true);
            /// <summary> Unhides this <see cref="Menu"/> next frame. </summary>
            public void Unhide() => SetIsHidden(false);

            private void SetIsHidden(bool which) {
                foreach(IElement<Menu> el in Elements) // For every element:
                    el.IsHidden = which;               //     set whether or not it is hidden.
            }

            void IMenuContract.Draw<TDrawer>(in TDrawer drawer, Master master) => Draw(in drawer, master);
            protected abstract void Draw<TDrawer>(in TDrawer drawer, Master master)
                where TDrawer : ILocalDrawer<Screen>;
            
            bool IMenuContract.IsMouseOver(PointI mouse) => IsMouseOver(mouse);
            protected virtual bool IsMouseOver(PointI mouse) {
                var localMouse = ScreenPointToMenu(mouse.X, mouse.Y);
                foreach(IElement<Menu> el in Elements) { // For every element in this menu:
                    if(el.IsMouseOver(localMouse))       // If the mouse is hovering over it,
                        return true;                     //     return true.
                }
                return false; // If we got this far without returning already, return false.
            }

            /// <summary> Draws the specified element onscreen. </summary>
            protected void DrawElement<TDrawer>(Element<Menu> element, in TDrawer drawer, Master master)
                where TDrawer : ILocalDrawer<Menu>
                => (element as IElement<Menu>).Draw(in drawer, master);

            bool IMenuContract.GetButton(PointI mouse, out IButton button) {
                var localMouse = ScreenPointToMenu(mouse.X, mouse.Y);
                foreach(IElement<Menu> el in Elements) {                // For every element in this menu:
                    if(el is IButton b && el.IsMouseOver(localMouse)) { // If the element is a button and the mouse is hovering over it,
                        button = b;                                     //     output the button,
                        return true;                                    //     and return true.
                    }
                }
                button = null!;
                return false; // If we got this far without exiting the method, return false.
            }

            /// <summary>
            /// Transforms a point from the screen to a local point within this <see cref="Menu"/>.
            /// </summary>
            protected abstract PointF ScreenPointToMenu(int screenX, int screenY);
        }
    }
}

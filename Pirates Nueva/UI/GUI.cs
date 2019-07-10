﻿using System;
using System.Collections.Generic;
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
        private Dictionary<string, (Edge edge, Direction dir, Element<Edge> element)> _edgeElements = new Dictionary<string, (Edge, Direction, Element<Edge>)>();
        private Dictionary<string, Menu> _menus = new Dictionary<string, Menu>();

        public Master Master { get; private set; }

        public int ScreenWidth => Master.GraphicsDevice.Viewport.Width;
        public int ScreenHeight => Master.GraphicsDevice.Viewport.Height;

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
            if(_edgeElements.ContainsKey(id) == false) {
                (element as IElement).SubscribeOnPropertyChanged(() => ArrangeEdges());
                _edgeElements[id] = (edge, dir, element); // Add the edge element to the dictionary of edge elements.
                ArrangeEdges();                           // Update the arrangement of edge elements after it has been added.
            }
            //
            // If there is already a floating element identified by /id/, throw an InvalidOperationException.
            else {
                throw new InvalidOperationException(
                    $"{nameof(GUI)}.{nameof(AddEdge)}(): There is already a floating element named \"{id}\"!"
                    );
            }
        }

        /// <summary> Get the <see cref="EdgeElement"/> identifed by /id/. </summary>
        public bool TryGetEdge(string id, out Element<Edge> element) {
            if(this._edgeElements.TryGetValue(id, out var info)) {
                element = info.element;
                return true;
            }
            else {
                element = null!;
                return false;
            }
        }

        /// <summary> Get the edge element identified by /id/ and that is of type /T/. </summary>
        public bool TryGetEdge<T>(string id, out T element) where T : Element<Edge> {
            if(TryGetEdge(id, out var med) && med is T last) // If there's an edge of type /T/ identified by /id/,
                element = last;                              //     return it.
            else                                             // If there is no edge element of type /T/ and identified by /id/,
                element = null!;                             //     return false;
                                                             //
            return element != default;                       // Return whether or not we found /edge/.
        }

        /// <summary> Whether or not there is a edge element identified by /id/. </summary>
        public bool HasEdge(string id) => this._edgeElements.ContainsKey(id);

        /// <summary>
        /// Remove the edge element identifed by /id/.
        /// </summary>
        /// <exception cref="KeyNotFoundException">Thrown when there is no <see cref="EdgeElement"/> to remove.</exception>
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
        /// <exception cref="KeyNotFoundException">Thrown when one of the strings does not identify an existing <see cref="EdgeElement"/>.</exception>
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
            if(this._menus.ContainsKey(id) == false)
                this._menus[id] = menu;
            else
                throw new InvalidOperationException(
                    $"{nameof(GUI)}.{nameof(AddMenu)}(): There is already a {nameof(Menu)} identified by string \"{id}\"!"
                    );
        }

        /// <summary> Whether or not there is a <see cref="Menu"/> identified by /id/. </summary>
        public bool HasMenu(string id) => this._menus.ContainsKey(id);

        /// <summary> Get the <see cref="Menu"/> identified by /id/. </summary>
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

        /// <summary> Returns whether or not the specified point is over any GUI elements. </summary>
        public bool IsPointOverGUI(PointI point) {
            foreach(var info in this._edgeElements.Values) {      // For every edge element:
                if((info.element as IElement).IsMouseOver(point)) // If the point is hovering over it,
                    return true;                                  //     return true.
            }                                                     //
                                                                  //
            foreach(var menu in this._menus.Values) {             // For every menu:
                if((menu as IMenuContract).IsMouseOver(point))    // If the point is hovering over it,
                    return true;                                  //     return true.
            }                                                     //
                                                                  //
            return false;                                         // If we got this far without returning, return false.
        }

        /// <summary> Update the arrangement of edge elements. </summary>
        void ArrangeEdges() {
            const int Padding = 5;

            Dictionary<(Edge, Direction), int> stackLengths = new Dictionary<(Edge, Direction), int>();

            foreach(var info in this._edgeElements.Values) {
                var edge = info.element;
                var con = edge as IElement;

                // Copy over some commonly used properties of /edge/.
                var (e, d, width, height) = (info.edge, info.dir, edge.WidthPixels, edge.HeightPixels);

                if(stackLengths.ContainsKey((e, d)) == false) // If the stack for /edge/ is unassigned,
                    stackLengths[(e, d)] = Padding;           // make it default to the constant /Padding/.
                
                if(d == Direction.Left || d == Direction.Up) // If the direction is 'left' or 'up',
                    incr();                                  // increment the stack.

                /* Arrange /edge/ into its stack. */
                    if(e == Edge.Top || e == Edge.Bottom) // Position it based on the edge it's hugging.
                        con.Top = e == Edge.Top ? Padding : ScreenHeight - height - Padding;
                    else
                        con.Left = e == Edge.Right ? ScreenWidth - width - Padding : Padding;
                
                    if(d == Direction.Up || d == Direction.Down) // Position it based on its stack direction
                        con.Top = d == Direction.Up ? ScreenHeight - stackLengths[(e, d)] : stackLengths[(e, d)];
                    else
                        con.Left = d == Direction.Right ? stackLengths[(e, d)] : ScreenWidth - stackLengths[(e, d)];

                if(d == Direction.Right || d == Direction.Down) // If the direction is 'right' or 'down',
                    incr();                                     // increment the stack.

                // Increment the stack length that /edge/ is aligned in.
                void incr() => stackLengths[(e, d)] += (d == Direction.Down || d == Direction.Up ? height : width) + Padding;
            }
        }

        void IUpdatable.Update(Master master, Time delta) {
            if(!master.Input.MouseLeft.IsDown) // If the mouse was NOT clicked this frame,
                return;                        //     exit the method.

            var mouse = master.Input.MousePosition;
            foreach(var info in this._edgeElements.Values) {    // For every edge element:
                var edge = info.element as IElement;
                if(edge is IButton b && edge.IsMouseOver(mouse)) {   // If the element is a button and the mouse is over it,
                    b.OnClick();                                     //     invoke its action,
                    return;                                          //     and exit this method.
                }
            }

            foreach(var menu in this._menus.Values) {        // For every menu:
                (menu as IMenuContract).QueryClicks(mouse);  //     query a click at the current mouse position.
            }
        }

        internal void Draw(Master master) {
            //
            // Draw every edge element.
            foreach(var info in this._edgeElements.Values) {
                var edge = info.element as IElement;
                edge.Draw(master, 0, 0);
            }
            
            // Draw every menu.
            foreach(IMenuContract menu in this._menus.Values) {
                menu.Draw(master);
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
        private interface IElement
        {
            int Left { get; set; }
            int Top { get; set; }

            bool IsHidden { get; set; }

            /// <summary> Signs up the specified <see cref="Action"/> to be called when a property changes. </summary>
            void SubscribeOnPropertyChanged(Action action);

            /// <summary> Draws this element onscreen, using the offset. </summary>
            void Draw(Master master, int offsetX, int offsetY);

            /// <summary> Whether or not the mouse is hovering over this element, measuring from the specified top left corner. </summary>
            bool IsMouseOver(PointI mouse);
        }
        /// <summary>
        /// A GUI element onscreen.
        /// </summary>
        public abstract class Element<TDrawer> : IElement
        {
            protected Action? onPropertyChanged;

            /// <summary> The local position of this element's left edge. </summary>
            public int Left { get; private set; }
            /// <summary> The local position of this element's top edge. </summary>
            public int Top { get; private set; }

            int IElement.Left { get => Left; set => Left = value; }
            int IElement.Top { get => Top; set => Top = value; }

            /// <summary> The width of this element, in pixels. </summary>
            public abstract int WidthPixels { get; }
            /// <summary> The height of this element, in pixels. </summary>
            public abstract int HeightPixels { get; }
            
            private Rectangle? Bounds { get; set; } // The extents of this element. Used in IsMouseOver().

            void IElement.SubscribeOnPropertyChanged(Action action) => this.onPropertyChanged += action;

            /// <summary> Draw this <see cref="Element"/> onscreen, from the specified top left corner. </summary>
            protected abstract void Draw(Master master, int offsetX, int offsetY);
            
            private bool IsHidden { get; set; }
            bool IElement.IsHidden { get => IsHidden; set => IsHidden = value; }

            void IElement.Draw(Master master, int offsetX, int offsetY) {
                if(!IsHidden) {
                    Draw(master, offsetX, offsetY);                                                   // Draw the button onscreen.
                    Bounds = new Rectangle(Left + offsetX, Top + offsetY, WidthPixels, HeightPixels); // Store the bounds of this element
                                                                                                      //     for later use in IsMouseOver().
                }
                else {             // If the element is hidden,
                    Bounds = null; //     don't draw it, and set the bounds to null.
                }
            }
            bool IElement.IsMouseOver(PointI mouse) => Bounds?.Contains(mouse) ?? false;

            /// <summary> Call this when a property is changed after initialization. </summary>
            protected void PropertyChanged() => this.onPropertyChanged?.Invoke();
        }

        
        /// <summary>
        /// Makes the Draw() method of a <see cref="Menu"/> accessible only within <see cref="GUI"/>.
        /// </summary>
        private interface IMenuContract
        {
            void Draw(Master master);

            bool IsMouseOver(PointI mouse);

            /// <summary> If /mouse/ is hovering over any buttons, invoke its action and return true. </summary>
            bool QueryClicks(PointI mouse);
        }
        /// <summary>
        /// A base class for different types of menus.
        /// </summary>
        public abstract class Menu : IMenuContract
        {
            /// <summary> The default spacing between <see cref="Element"/>s. </summary>
            protected const int Padding = 3;

            /// <summary> Every <see cref="MenuElement"/> in this <see cref="Menu"/>. </summary>
            protected Element<Menu>[] Elements { get; set; }

            public Menu(Element<Menu>[] elements) {
                
                foreach(var el in elements) {
                    (el as IElement).SubscribeOnPropertyChanged(arrange); // Rearrange the elements when a property changes.
                }

                Elements = elements;

                arrange();

                void arrange() {
                    int elementsLeft = Padding;                   // The length of the row of elements.
                    foreach(var el in Elements) {                 // For every element:
                        (el as IElement).Left = elementsLeft;     // Put it in the furthest-right position in the menu,
                        (el as IElement).Top = Padding;           // put padding above it,
                                                                  //
                        elementsLeft += el.WidthPixels + Padding; // and increment the length of the row by the element's width.
                    }
                }
            }

            /// <summary> Hide this <see cref="Menu"/> next frame. </summary>
            public void Hide() => SetIsHidden(true);
            /// <summary> Unhide this <see cref="Menu"/> next frame. </summary>
            public void Unhide() => SetIsHidden(false);

            private void SetIsHidden(bool which) {
                foreach(IElement el in Elements) // For every element:
                    el.IsHidden = which;         //     set whether or not it is hidden.
            }

            void IMenuContract.Draw(Master master) => Draw(master);
            protected abstract void Draw(Master master);
            
            bool IMenuContract.IsMouseOver(PointI mouse) => IsMouseOver(mouse);
            protected virtual bool IsMouseOver(PointI mouse) {
                foreach(IElement el in Elements) { // For every element in this menu:
                    if(el.IsMouseOver(mouse))      // If the mouse is hovering over it,
                        return true;               //     return true.
                }
                return false; // If we got this far without returning already, return false.
            }

            /// <summary> Draw the input element onscreen. </summary>
            protected void DrawElement(Master master, Element<Menu> element, int offsetX, int offsetY) {
                (element as IElement).Draw(master, offsetX, offsetY);
            }

            bool IMenuContract.QueryClicks(PointI mouse) {
                foreach(var el in Elements) {              // For every element in this menu:
                    if(el is IButton b && el is IElement d // If the element is a button,
                        && d.IsMouseOver(mouse)) {         // and the mouse is hovering over it,
                        b.OnClick.Invoke();                //     invoke its action,
                        return true;                       //     and return true.
                    }
                }
                return false; // If we got this far without exiting the method, return false.
            }
        }
    }
}

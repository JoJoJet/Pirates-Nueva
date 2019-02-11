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
        
        static SpriteFont Font { get; set; }

        public Master Master { get; }

        internal GUI(Master master) {
            Master = master;

            Font = master.Font;
        }
        
        /// <summary>
        /// Add the indicated floating element to the GUI.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if there is already a floating element identified by /id/.</exception>
        public void AddFloating(string id, IFloating floating) {
            if(_floatingElements.ContainsKey(id) == false)
                _floatingElements[id] = floating;
            else
                throw new InvalidOperationException(
                    $"{nameof(GUI)}.{nameof(AddFloating)}(): There is already a floating element named \"{id}\"!"
                    );
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

        void IUpdatable.Update(Master master) {

        }

        void IDrawable.Draw(Master master) {

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
        public class FloatingButton : IFloating
        {
            const int Padding = 4;

            public string Text { get; }

            public OnClick OnClick { get; }

            public Edge Edge { get; }
            public Direction StackDirection { get; }

            public int WidthPixels => (int)Font.MeasureString(Text).X + Padding*2;
            public int HeightPixels => (int)Font.MeasureString(Text).Y + Padding*2;

            public FloatingButton(string text, OnClick onClick, Edge edge, Direction stackDirection) {
                Text = text;
                OnClick = onClick;
                Edge = edge;
                StackDirection = stackDirection;
            }
        }
        public class FloatingText : IFloating
        {
            public string Text { get; }

            public Edge Edge { get; }
            public Direction StackDirection { get; }
            
            public int WidthPixels => (int)Font.MeasureString(Text).X;
            public int HeightPixels => (int)Font.MeasureString(Text).Y;

            public FloatingText(string text, Edge edge, Direction stackDirection) {
                Text = text;
                Edge = edge;
                StackDirection = stackDirection;
            }
        }
    }
}

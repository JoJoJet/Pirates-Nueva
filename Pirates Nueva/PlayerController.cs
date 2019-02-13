using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pirates_Nueva
{
    /// <summary>
    /// A game object that can be <see cref="Focus(Master)"/>'ed on by <see cref="PlayerController"/>.
    /// </summary>
    public interface IFocusable
    {
        /// <summary> Called when <see cref="PlayerController"/> starts focusing on this object. </summary>
        void StartFocus(Master master);
        /// <summary> Called when <see cref="PlayerController"/> is focusing on this object. </summary>
        void Focus(Master master);
        /// <summary> Called when <see cref="PlayerController"/> stops focusing on this object. </summary>
        void Unfocus(Master master);
    }
    public class PlayerController : IUpdatable
    {
        private IFocusable _focused;

        public Master Master { get; }

        private Sea Sea { get; }

        private IFocusable Focused {
            get => this._focused;
            set {
                if(value != this._focused) {        // If the focused object changed,
                    this._focused?.Unfocus(Master); // call the old one's Unfocus() method,
                    value?.StartFocus(Master);      // and the new one's StartFocus() method.
                }

                this._focused = value;
            }
        }

        internal PlayerController(Master master, Sea sea) {
            Master = master;
            Sea = sea;
        }

        void IUpdatable.Update(Master master) {
            if(master.Input.MouseLeft.IsDown && master.GUI.IsMouseOverGUI == false) {
                var (seaX, seaY) = Sea.ScreenPointToSea(master.Input.MousePosition);
                var focusable = Sea.GetFocusable((seaX, seaY));  // Get any focusable objects under the mouse.
                Focused = focusable.FirstOrDefault();            // Set the focus to be the first element of /focusable/
            }

            if(Focused != null)
                Focused.Focus(master);
        }
    }
}

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
        /// <summary> Whether or not the focus should be locked onto this object. </summary>
        bool IsLocked { get; }

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

        private IFocusable[] Focusable { get; set; } = new IFocusable[0];
        private int FocusIndex { get; set; }
        private IFocusable Focused => Focusable.Length > 0 ? Focusable[FocusIndex] : null;

        internal PlayerController(Master master, Sea sea) {
            Master = master;
            Sea = sea;
        }

        void IUpdatable.Update(Master master) {
            if(master.Input.MouseLeft.IsDown && !master.GUI.IsMouseOverGUI) { // If the user clicked, but not on GUI,
                if(!(Focused?.IsLocked == true)) {                            // and if the current focus isn't locked.

                    var (seaX, seaY) = Sea.ScreenPointToSea(master.Input.MousePosition);
                    var focusable = Sea.GetFocusable((seaX, seaY));  // Get any focusable objects under the mouse.

                    IFocusable oldFocus = Focused;
                    if(Focused != null && Focusable.SequenceEqual(focusable)) { // If the user clicked the same spot as last,
                        FocusIndex = (FocusIndex + 1) % Focusable.Length;       // Cycle through all of the focusable things right here.
                    }
                    else {                               // If this is a new spot,
                        Focusable = focusable.ToArray(); // set the focus to the first element of /focusable/.
                        FocusIndex = 0;
                    }

                    if(Focused != oldFocus) {        // If the focus has changed,
                        oldFocus?.Unfocus(master);   // call Unfocus() on the old one,
                        Focused?.StartFocus(master); // and StartFocus() on the new.
                    }
                }
            }

            if(Focused != null)
                Focused.Focus(master);
        }
    }
}

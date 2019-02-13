using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pirates_Nueva
{
    /// <summary>
    /// A object that the <see cref="PlayerController"/> can focus on.
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
    /// <summary>
    /// An object that contains <see cref="IFocusable"/>s.
    /// </summary>
    public interface IFocusableParent
    {
        List<IFocusable> GetFocusable(PointF seaPoint);
    }
    public class PlayerController : IUpdatable
    {
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
            if(master.Input.MouseLeft.IsDown && !master.GUI.IsMouseOverGUI // If the user clicked, but it not on GUI,
                && !(Focused?.IsLocked == true)) {                         // and if the current focus isn't locked:

                var (seaX, seaY) = Sea.ScreenPointToSea(master.Input.MousePosition);
                var focusable = (Sea as IFocusableParent).GetFocusable((seaX, seaY));  // Get any focusable objects under the mouse.

                var oldFocused = Focused;        // Save a copy of the old focused object.
                var oldFocusable = Focusable;    // Save a copy of the old focusable objects.
                Focusable = focusable.ToArray(); // Assign the new focusable objects.

                bool qualify(int i) => i < Focusable.Length && i < oldFocusable.Length;
                for(int i = 0; qualify(i); i++) {           // For each focusable element:
                    if(i == FocusIndex) {                   // if its index is the same as the old focused element:
                        if(Focusable[i] == oldFocusable[i]) //     If the old and new sets have been equal up to here,
                            FocusIndex = i+1;               //         set the index of focus to be the current index + 1.
                        break;                              //     Also: break from the loop.
                    }                                       // Otherwise:
                    if(Focusable[i] != oldFocusable[i]) {   // If the current element is the previous focusable element at this position,
                        FocusIndex = i;                     //     set the index of focus to be the current index,
                        break;                              //     and break from the loop.
                    }
                }

                if(Focusable.Length > 0)            // If the set of focusable elements is NOT empty,
                    FocusIndex %= Focusable.Length; //     make sure the index of focus isn't greater than the length of the set.
                else                                // If the set of focusable elements IS empty,
                    FocusIndex = 0;                 //     set the index of focus to be zero.

                if(Focused != oldFocused) {      // If the focused object has changed,
                    oldFocused?.Unfocus(master); //    call Unfocus() on the old one,
                    Focused?.StartFocus(master); //    and StartFocus() on the new one.
                }
            }

            if(Focused != null)        // If we're focusing on an object,
                Focused.Focus(master); //     call its Focus() method
        }
    }
}

using System;
using System.Collections.Generic;
using Pirates_Nueva.Ocean;
#nullable enable

namespace Pirates_Nueva
{
    /// <summary>
    /// An object that the <see cref="PlayerController"/> can focus on.
    /// </summary>
    public interface IFocusable
    {
        /// <summary>
        /// Whether or not the current instance is being focused on.
        /// </summary>
        bool IsFocused { set; }
        /// <summary>
        /// Gets an object that provides a menu for focusing on this object.
        /// </summary>
        IFocusMenuProvider GetProvider(Master master);
    }
    /// <summary>
    /// An object that contains <see cref="IFocusable"/>s.
    /// </summary>
    public interface IFocusableParent
    {
        List<IFocusable> GetFocusable(PointF seaPoint);
    }

    /// <summary>
    /// An object that displays a menu for an <see cref="IFocusable"/> object.
    /// </summary>
    public interface IFocusMenuProvider
    {
        /// <summary>
        /// Whether or not the focus should be locked onto this menu.
        /// </summary>
        bool IsLocked { get; }
        /// <summary>
        /// Is called every frame while this menu is displayed.
        /// </summary>
        void Update(Master master);
        /// <summary>
        /// Is called when the menu is unfocused.
        /// </summary>
        void Close(Master master);
    }

    public class PlayerController : IUpdatable
    {
        private PointI mouseFirst;
        private PointF cameraFirst;

        public Master Master { get; }

        private Sea Sea { get; }

        private IFocusable[] Focusable { get; set; } = new IFocusable[0];
        private int FocusIndex { get; set; }
        private IFocusable? Focused => Focusable.Length > 0 ? Focusable[FocusIndex] : null;

        IFocusMenuProvider? FocusProvider { get; set; }

        const string MouseDebugID = "debug_mouse";
        const string CameraDebugID = "debug_cameraPosition";
        internal PlayerController(Master master, Sea sea) {
            Master = master;
            Sea = sea;

            master.GUI.AddEdge(MouseDebugID, new UI.EdgeText("mouse position", master.Font, UI.Edge.Top, UI.Direction.Right));
            master.GUI.AddEdge(CameraDebugID, new UI.EdgeText("camera", master.Font, UI.Edge.Top, UI.Direction.Left));
        }

        void IUpdatable.Update(Master master, Time delta) {
            if(master.Input.MouseLeft.IsDown && !master.GUI.IsMouseOverGUI // If the user clicked, but it not on GUI,
                && (!FocusProvider?.IsLocked ?? true)) {                   // and if the current focus isn't locked:

                var (seaX, seaY) = Sea.MousePosition;
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

                if(Focused != oldFocused) {                       // If the focused object has changed,
                    if(oldFocused != null)                        //
                        oldFocused.IsFocused = false;             //
                    FocusProvider?.Close(master);                 //     close the old menu,
                    if(Focused != null)                           //
                        Focused.IsFocused = true;                 //
                    FocusProvider = Focused?.GetProvider(master); //     and create a new one.
                }
            }

            FocusProvider?.Update(master); // If there's a focus provider, update it.


            if(master.Input.MouseWheel.IsDown) {                    // When the user clicks the scrollwheel,
                mouseFirst = master.Input.MousePosition;            //     store the position of the mouse
                cameraFirst = (Sea.Camera.Left, Sea.Camera.Bottom); //     and of the camera.
            }
            if(master.Input.MouseWheel.IsPressed) {                                            // When the scrollwheel is held down:
                var mDelta = master.Input.MousePosition - mouseFirst;                          //     Find how much the mouse has moved,
                mDelta.Y *= -1;                                                                //     and invert the y component of that value.
                (Sea.Camera.Left, Sea.Camera.Bottom) = cameraFirst - (PointF)mDelta / Sea.PPU; //     Move the camera according to that value.
            }

            Sea.Camera.Zoom += master.Input.MouseWheel.Scroll / 120 * (15f + Sea.Camera.Zoom)/16;

            if(master.GUI.TryGetEdge<UI.EdgeText>(MouseDebugID, out var tex)) { // If there's a mouse debug GUI element,
                tex.Text = $"Mouse: {Sea.MousePosition}";                       //     update its text to display the mouse position.
            }
            if(master.GUI.TryGetEdge<UI.EdgeText>(CameraDebugID, out var edge)) { // If there's a camera debug GUI element,
                edge.Text = "Camera Position: " + Sea.Camera.Position;            //     update its text to display the camera position.
            }
        }
    }
}

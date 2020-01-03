using System;
using System.Collections.Generic;
using Pirates_Nueva.Ocean;
using Pirates_Nueva.UI;

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
        private enum Mode { Focus = 0, PlaceEnemy };

        private PointI mouseFirst;
        private PointF cameraFirst;

        private Mode mode;

        public Master Master { get; }

        public Faction Faction { get; }

        internal Faction EnemyFaction { get; }

        private Sea Sea { get; }

        private IFocusable[] Focusable { get; set; } = new IFocusable[0];
        private int FocusIndex { get; set; }
        private IFocusable? Focused => Focusable.Length > 0 ? Focusable[FocusIndex] : null;

        IFocusMenuProvider? FocusProvider { get; set; }

        const string MouseDebugID = "debug_mouse";
        const string CameraDebugID = "debug_cameraPosition";
        const string PlaceEnemyDebugID = "debug_placeEnemy";
        internal PlayerController(Master master, Sea sea) {
            Master = master;
            Sea = sea;

            //
            // Create a Faction for this player.
            Faction = new Faction(isPlayer: true);
            EnemyFaction = new Faction(isPlayer: false);

            //
            // Add the player's ship.
            Sea.AddEntity(new Ship(Sea, ShipDef.Get("dinghy"), Faction));

            master.GUI.AddEdge(MouseDebugID, Edge.Top, Direction.Right, new MutableText<Edge>("mouse position", master.Font));
            master.GUI.AddEdge(CameraDebugID, Edge.Top, Direction.Left, new MutableText<Edge>("camera", master.Font));

            master.GUI.AddEdge(PlaceEnemyDebugID, Edge.Bottom, Direction.Right,
                               new Button<Edge>("Place enemy", master.Font, () => this.mode = Mode.PlaceEnemy)
                               );
        }

        void IUpdatable.Update(in UpdateParams @params)
        {
            var master = @params.Master;
            switch(this.mode) {
                case Mode.PlaceEnemy:
                    //
                    // If the user clicks, place an enemy there and exit enemy placing mode.
                    if(master.Input.MouseLeft.IsDown && !master.GUI.IsMouseOverGUI) {
                        Sea.AddEntity(new Ship(Sea, ShipDef.Get("dinghy"), EnemyFaction, Sea.MousePosition));
                        this.mode = Mode.Focus;
                    }
                    break;

                case Mode.Focus:
                    //
                    // If the user clicked on anything other than GUI, and if the current focus isn't locked,
                    // change what the user is focusing on.
                    if(master.Input.MouseLeft.IsDown && !master.GUI.IsMouseOverGUI
                       && (!FocusProvider?.IsLocked ?? true)) {
                        //
                        // Get a list of focusable objects under the mouse.
                        var (seaX, seaY) = Sea.MousePosition;
                        var focusable = (Sea as IFocusableParent).GetFocusable((seaX, seaY));
                        //
                        // Make a local copy of the last object that was being focused on,
                        // as well as all of the objects that were under the mouse last time.
                        var oldFocused = Focused;
                        var oldFocusable = Focusable;
                        Focusable = focusable.ToArray();
                        //
                        // Iterate over the old and new lists of focusable objects at the same time.
                        for(int i = 0; i < Focusable.Length && i < oldFocusable.Length; i++) {
                            //
                            // If the current index in the lists is the same
                            // index as the element we were previously focusing on,
                            // that means all of the focusable objects in the list up to this point
                            // are the same as the focusable objects from last time.
                            if(i == FocusIndex) {
                                // If the objects at this position are the same,
                                // increase FocusIndex in order to set the focus on the subsequent object.
                                if(Focusable[i] == oldFocusable[i])
                                    FocusIndex = i+1;
                                //
                                // If the objects at this position are different,
                                // that means they have changed since last time. Leave FocusIndex as-is.
                                break;
                            }
                            //
                            // If we are looking at a position in the loops that
                            // comes before the element on which we were previously focusing on,
                            // Check if the list of focusable elmenents has changed since the last time this method was run.
                            // If it has, set FocusIndex to be the current index in the list, and break from the loop.
                            if(Focusable[i] != oldFocusable[i]) {
                                FocusIndex = i;
                                break;
                            }
                        }
                        //
                        // Make FocusIndex wrap around the list of focusable elements.
                        if(Focusable.Length > 0)
                            FocusIndex %= Focusable.Length;
                        else
                            FocusIndex = 0;

                        //
                        // If the focus has changed,
                        // close the old focus provider,
                        // and get a focus provider for the new object.
                        if(Focused != oldFocused) {
                            if(oldFocused != null)
                                oldFocused.IsFocused = false;
                            FocusProvider?.Close(master);
                            if(Focused != null)
                                Focused.IsFocused = true;
                            FocusProvider = Focused?.GetProvider(master);
                        }
                    }
                    //
                    // Update the current focus provider, if it exists.
                    FocusProvider?.Update(master);
                    break;
            }

            //
            // We always allow the user to move the camera, no matter what mode they're in.
            if(master.Input.MouseWheel.IsDown) {                    // When the user clicks the scrollwheel,
                mouseFirst = master.Input.MousePosition;            //     store the position of the mouse
                cameraFirst = (Sea.Camera.Left, Sea.Camera.Bottom); //     and of the camera.
            }
            if(master.Input.MouseWheel.IsPressed) {                                            // When the scrollwheel is held down:
                var mDelta = master.Input.MousePosition - mouseFirst;                          //     Find how much the mouse has moved,
                mDelta = mDelta.With(Y: -mDelta.Y);                                            //     and invert the y componenet of that value.
                (Sea.Camera.Left, Sea.Camera.Bottom) = cameraFirst - (PointF)mDelta / Sea.PPU; //     Move the camera according to that value.
            }

            Sea.Camera.Zoom += master.Input.MouseWheel.Scroll / 120 * (15f + Sea.Camera.Zoom)/16;

            if(master.GUI.TryGetEdge<MutableText<Edge>>(MouseDebugID, out var tex)) { // If there's a mouse debug GUI element,
                tex.Value = $"Mouse: {Sea.MousePosition}";                       //     update its text to display the mouse position.
            }
            if(master.GUI.TryGetEdge<MutableText<Edge>>(CameraDebugID, out var edge)) { // If there's a camera debug GUI element,
                edge.Value = "Camera Position: " + Sea.Camera.Position;            //     update its text to display the camera position.
            }
        }
    }
}

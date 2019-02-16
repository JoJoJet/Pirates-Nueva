using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pirates_Nueva.Path;

namespace Pirates_Nueva
{
    public class Agent : IDrawable, IFocusable, UI.IScreenSpaceTarget
    {
        private Stack<Block> _path;

        /// <summary> The <see cref="Pirates_Nueva.Ship"/> that contains this <see cref="Agent"/>. </summary>
        public Ship Ship { get; }

        /// <summary> The <see cref="Block"/> that this <see cref="Agent"/> is standing on or moving from. </summary>
        public Block CurrentBlock { get; protected set; }
        /// <summary> The <see cref="Block"/> that this <see cref="Agent"/> is moving to. </summary>
        public Block NextBlock { get; protected set; }
        /// <summary>
        /// This <see cref="Agent"/>'s progress in moving between <see cref="CurrentBlock"/> and <see cref="NextBlock"/>.
        /// </summary>
        public float MoveProgress { get; protected set; }
        
        /// <summary> The X coordinate of this <see cref="Agent"/>, local to its <see cref="Pirates_Nueva.Ship"/>. </summary>
        public float X => Lerp(CurrentBlock.X, (NextBlock??CurrentBlock).X, MoveProgress);
        /// <summary> The Y coordinate of this <see cref="Agent"/>, local to its <see cref="Pirates_Nueva.Ship"/>. </summary>
        public float Y => Lerp(CurrentBlock.Y, (NextBlock??CurrentBlock).Y, MoveProgress);

        /// <summary> Linearly interpolate between two values, by amount /f/. </summary>
        private float Lerp(float a, float b, float f) => a * (1 - f) + b * f;

        protected Stack<Block> Path {
            get => this._path;
            set => this._path = (value.Count > 0 ? value : null);
        }

        public Agent(Ship ship, Block floor) {
            Ship = ship;
            CurrentBlock = floor;
        }

        #region IDrawable Implementation
        void IDrawable.Draw(Master master) {
            var tex = master.Resources.LoadTexture("agent");

            (float seaX, float seaY) = Ship.ShipPointToSea(X, Y+1);
            (int screenX, int screenY) = Ship.Sea.SeaPointToScreen(seaX, seaY);
            master.Renderer.DrawRotated(tex, screenX, screenY, Ship.Part.Pixels, Ship.Part.Pixels, -Ship.Angle, (0, 0));
        }
        #endregion

        #region IScreenSpaceTarget Implementation
        private PointI ScreenTarget => Ship.Sea.SeaPointToScreen(Ship.ShipPointToSea(X + 0.5f, Y + 0.5f));
        int UI.IScreenSpaceTarget.X => ScreenTarget.X;
        int UI.IScreenSpaceTarget.Y => ScreenTarget.Y;
        #endregion

        #region IFocusable Implementation
        enum FocusOption { None, ChoosePath };
        private FocusOption focusMode;

        private bool IsFocusLocked { get; set; }
        bool IFocusable.IsLocked => IsFocusLocked;

        const string FocusMenuID = "agentfocusfloating";
        void IFocusable.StartFocus(Master master) {
            if(master.GUI.HasMenu(FocusMenuID) == false) // If there is no GUI menu for this Agent,
                master.GUI.AddMenu(                      //     create one.
                    FocusMenuID,
                    new UI.FloatingMenu(
                        this, (0, -0.05f), UI.Corner.BottomLeft,
                        new UI.MenuText("Agent", master.Font),
                        new UI.MenuButton("Path", master.Font, () => this.focusMode = FocusOption.ChoosePath)
                    )
                );
        }
        void IFocusable.Focus(Master master) {
            if(this.focusMode == FocusOption.ChoosePath) { // If we are currently choosing a path,
                IsFocusLocked = true;                      //     lock the focus onto this agent.

                if(master.Input.MouseLeft.IsDown && !master.GUI.IsMouseOverGUI) {             // If the user clicked, but not on GUI:
                    var (seaX, seaY) = Ship.Sea.ScreenPointToSea(master.Input.MousePosition);
                    var (shipX, shipY) = Ship.SeaPointToShip(seaX, seaY);               // The point the user clicked,
                                                                                        //     local to the ship.
                    if(Ship.AreIndicesValid(shipX, shipY) &&                            // If the spot is a valid index,
                        Ship.GetBlock(shipX, shipY) is Block target) {                  // and it has a block,
                        Path = Dijkstra.FindPath(Ship, CurrentBlock, target); //     have the agent path to that block.
                    }

                    this.focusMode = FocusOption.None; // Unset the focus mode,
                    IsFocusLocked = false;             // and release the focus.
                }
            }
        }
        void IFocusable.Unfocus(Master master) {
            if(master.GUI.HasMenu(FocusMenuID))     // If there is a GUI menu for this block,
                master.GUI.RemoveMenu(FocusMenuID); //     remove it.
        }
        #endregion
    }
}

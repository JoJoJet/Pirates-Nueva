using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pirates_Nueva.Path;

namespace Pirates_Nueva
{
    public class Agent : IUpdatable, IDrawable, IFocusable, UI.IScreenSpaceTarget
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

        public Job Job { get; protected set; }

        /// <summary> Linearly interpolate between two values, by amount /f/. </summary>
        private float Lerp(float a, float b, float f) => a * (1 - f) + b * f;

        protected Stack<Block> Path {
            get => this._path ?? (this._path = new Stack<Block>());
            set => this._path = value;
        }

        public Agent(Ship ship, Block floor) {
            Ship = ship;
            CurrentBlock = floor;
        }

        #region IUpdatable Implementation
        void IUpdatable.Update(Master master) => Update(master);
        /// <summary> The update loop of this <see cref="Agent"/>; is called every frame. </summary>
        protected virtual void Update(Master master) {
            if(Job == null)                      // If this agent has no job,
                Job = Ship.GetWorkableJob(this); //     get a workable job from the ship.

            if(Job != null) {                  // If there is a job:
                if(Job.Qualify(this, out _)) { //     If the job is workable,
                    if(Job.Work(this))         //         work it. If it's done,
                        Job = null;            //             unassign the job.
                }                              //
                else {                         //     If the job is not workable,
                    Ship.AddJob(Job);          //         put it back on the ship's list of jobs,
                    Job = null;                //         and unset our current job.
                }
            }

            if(NextBlock == null && Path.Count > 0) // If we're on a path but aren't moving towards a block,
                NextBlock = Path.Pop();             //     set the next block as the next step on the math.

            if(NextBlock != null) {                                     // If we are moving towards a block:
                MoveProgress += master.FrameTime.DeltaSeconds() * 1.5f; // increment our progress towards it.
                                                                        //
                if(MoveProgress >= 1) {                                 // If we have reached the block,
                    CurrentBlock = NextBlock;                           //     set it as our current block.
                    if(Path.Count > 0)                                  //     If we are currently on a path,
                        NextBlock = Path.Pop();                         //         set the next block as the next step on the path.
                    else                                                //     If we are not on a path,
                        NextBlock = null;                               //         unassign the next block.
                                                                        //
                    if(NextBlock != null)                               //     If we are still moving towards a block,
                        MoveProgress -= 1f;                             //         subtract 1 from our move progress.
                    else                                                //     If we are no longer moving towrards a block,
                        MoveProgress = 0;                               //         set our move progress to be 0.
                }
            }
        }
        #endregion

        #region IDrawable Implementation
        void IDrawable.Draw(Master master) => Draw(master);
        /// <summary> Draw this <see cref="Agent"/> onscreen. </summary>
        protected virtual void Draw(Master master) {
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
            if(master.GUI.TryGetMenu(FocusMenuID, out var menu)) { // If there is a menu:
                if(focusMode == FocusOption.None)                  //     If the focus mode is unset,
                    menu.Unhide();                                 //         unhide the menu.
                else                                               //     If the focus mode IS set.
                    menu.Hide();                                   //         hide the menu.
            }

            if(this.focusMode == FocusOption.ChoosePath) { // If we are currently choosing a path,
                IsFocusLocked = true;                      //     lock the focus onto this agent.

                if(master.Input.MouseLeft.IsDown && !master.GUI.IsMouseOverGUI) {        // If the user clicked, but not on GUI:
                    var (seaX, seaY) = Ship.Sea.ScreenPointToSea(master.Input.MousePosition);
                    var (shipX, shipY) = Ship.SeaPointToShip(seaX, seaY);                // The point the user clicked,
                                                                                         //     local to the ship.
                    if(Ship.AreIndicesValid(shipX, shipY) &&                             // If the spot is a valid index,
                        Ship.GetBlock(shipX, shipY) is Block target) {                   // and it has a block,
                        Path = Dijkstra.FindPath(Ship, NextBlock??CurrentBlock, target); //     have the agent path to that block.
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

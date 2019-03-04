﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pirates_Nueva.Path;

namespace Pirates_Nueva.Ocean
{
    public class Agent : IUpdatable, IDrawable, IFocusable, UI.IScreenSpaceTarget
    public interface IAgentContainer<TSelf, TSpot>
        where TSelf : IGraph<TSpot>
        where TSpot : IAgentSpot<TSpot>, INode<TSpot>
    {
        
    }
    public interface IAgentSpot<T>
        where T : INode<T>
    {
        int X { get; }
        int Y { get; }
    }

    {
        private Stack<Block> _path;

        /// <summary> The <see cref="Ocean.Ship"/> that contains this <see cref="Agent"/>. </summary>
        public Ship Ship { get; }

        /// <summary> The <see cref="Block"/> that this <see cref="Agent"/> is standing on or moving from. </summary>
        public Block CurrentBlock { get; protected set; }
        /// <summary> The <see cref="Block"/> that this <see cref="Agent"/> is moving to. </summary>
        public Block NextBlock { get; protected set; }
        /// <summary>
        /// This <see cref="Agent"/>'s progress in moving between <see cref="CurrentBlock"/> and <see cref="NextBlock"/>.
        /// </summary>
        public float MoveProgress { get; protected set; }
        
        /// <summary> The X coordinate of this <see cref="Agent"/>, local to its <see cref="Ocean.Ship"/>. </summary>
        public float X => Lerp(CurrentBlock.X, (NextBlock??CurrentBlock).X, MoveProgress);
        /// <summary> The Y coordinate of this <see cref="Agent"/>, local to its <see cref="Ocean.Ship"/>. </summary>
        public float Y => Lerp(CurrentBlock.Y, (NextBlock??CurrentBlock).Y, MoveProgress);

        public Job Job { get; protected set; }

        /// <summary> Linearly interpolate between two values, by amount /f/. </summary>
        private float Lerp(float a, float b, float f) => a * (1 - f) + b * f;

        /// <summary> The block that this <see cref="Agent"/> is currently pathing to. Is null if there is no path. </summary>
        public Block PathingTo => Path.Count > 0 ? Path.Last() : null;

        protected Stack<Block> Path {
            get => this._path ?? (this._path = new Stack<Block>());
            set => this._path = value;
        }

        public Agent(Ship ship, Block floor) {
            Ship = ship;
            CurrentBlock = floor;
        }

        #region Pathing
        /// <summary>
        /// Returns whether or not the specified <see cref="Block"/> is accessible to this <see cref="Agent"/>.
        /// </summary>
        public bool IsAccessible(Block target) {
            return Dijkstra.IsAccessible(Ship, NextBlock??CurrentBlock, target);
        }
        /// <summary>
        /// Returns whether or not this <see cref="Agent"/> can access a <see cref="Block"/> that matches /destination/.
        /// </summary>
        public bool IsAccessible(IsAtDestination<Block> destination) {
            return Dijkstra.IsAccessible(Ship, NextBlock??CurrentBlock, destination);
        }

        /// <summary> Have this <see cref="Agent"/> path to the specified <see cref="Block"/>. </summary>
        public void PathTo(Block target) {
            Path = Dijkstra.FindPath(Ship, NextBlock??CurrentBlock, target);
        }
        /// <summary> Have this <see cref="Agent"/> path to the first <see cref="Block"/> that matches /destination/. </summary>
        public void PathTo(IsAtDestination<Block> destination) {
            Path = Dijkstra.FindPath(Ship, NextBlock??CurrentBlock, destination);
        }
        #endregion

        #region IUpdatable Implementation
        void IUpdatable.Update(Master master, Time delta) => Update(master, delta);
        /// <summary> The update loop of this <see cref="Agent"/>; is called every frame. </summary>
        protected virtual void Update(Master master, Time delta) {
            if(Job == null) {                    // If this agent has no job,
                Job = Ship.GetWorkableJob(this); //     get a workable job from the ship,
                if(Job != null)                  //     If there was a workable job,
                    Job.Worker = this;           //         assign this agent to it.
            }

            if(Job != null) {                   // If there is a job:
                if(Job.Qualify(this, out _)) {  //     If the job is workable,
                    if(Job.Work(this, delta)) { //         work it. If it's done,
                        Ship.RemoveJob(Job);    //             remove the job from the ship,
                        Job = null;             //             and unassign it.
                    }                           //
                }                               //
                else {                          //     If the job is not workable,
                    Job.Worker = null;          //         unassign this agent from the job,
                    Job = null;                 //         and unset it.
                }
            }

            if(NextBlock == null && Path.Count > 0) // If we're on a path but aren't moving towards a block,
                NextBlock = Path.Pop();             //     set the next block as the next step on the math.

            if(NextBlock != null) {             // If we are moving towards a block:
                MoveProgress += delta * 1.5f;   // increment our progress towards it.
                                                //
                if(MoveProgress >= 1) {         // If we have reached the block,
                    CurrentBlock = NextBlock;   //     set it as our current block.
                    if(Path.Count > 0)          //     If we are currently on a path,
                        NextBlock = Path.Pop(); //         set the next block as the next step on the path.
                    else                        //     If we are not on a path,
                        NextBlock = null;       //         unassign the next block.
                                                //
                    if(NextBlock != null)       //     If we are still moving towards a block,
                        MoveProgress -= 1f;     //         subtract 1 from our move progress.
                    else                        //     If we are no longer moving towrards a block,
                        MoveProgress = 0;       //         set our move progress to be 0.
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
            master.Renderer.DrawRotated(tex, screenX, screenY, Ship.Sea.PPU, Ship.Sea.PPU, -Ship.Angle, (0, 0));
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

                if(master.Input.MouseLeft.IsDown && !master.GUI.IsMouseOverGUI) { // If the user clicked, but not on GUI:
                    var (seaX, seaY) = Ship.Sea.MousePosition;                    //
                    var (shipX, shipY) = Ship.SeaPointToShip(seaX, seaY);         // The point the user clicked,
                                                                                  //     local to the ship.
                    if(Ship.AreIndicesValid(shipX, shipY) &&                      // If the spot is a valid index,
                        Ship.GetBlock(shipX, shipY) is Block target) {            // and it has a block,
                        PathTo(target);                                           //     have the agent path to that block.
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

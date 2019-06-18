using System;
using System.Collections.Generic;
using System.Linq;
using Pirates_Nueva.Path;

namespace Pirates_Nueva.Ocean.Agents
{
    /// <summary>
    /// A character that exists in a container and can complete Jobs.
    /// </summary>
    /// <typeparam name="TC">The type of Container that this Agent exists in.</typeparam>
    /// <typeparam name="TSpot">The type of Spot that this Agent can rest on.</typeparam>
    public abstract class Agent<TC, TSpot> : IStockClaimant, IUpdatable, IDrawable, IFocusable, UI.IScreenSpaceTarget
        where TC    : class, IAgentContainer<TC, TSpot>
        where TSpot : class, IAgentSpot<TC, TSpot>
    {
        private Stack<TSpot>? _path;

        /// <summary> The object that contains this <see cref="Agent{TC, TSpot}"/>. </summary>
        protected TC Container { get; }

        /// <summary> The <see cref="TSpot"/> that this <see cref="Agent"/> is standing on or moving from. </summary>
        public TSpot CurrentSpot { get; protected set; }
        /// <summary> The <see cref="TSpot"/> that this <see cref="Agent"/> is moving to. </summary>
        public TSpot? NextSpot { get; protected set; }
        /// <summary>
        /// This <see cref="Agent"/>'s progress in moving between <see cref="CurrentSpot"/> and <see cref="NextSpot"/>.
        /// </summary>
        public float MoveProgress { get; protected set; }
        
        /// <summary> The X coordinate of this <see cref="Agent"/>, local to its container. </summary>
        public float X => Lerp(CurrentSpot.X, (NextSpot ?? CurrentSpot).X, MoveProgress);
        /// <summary> The Y coordinate of this <see cref="Agent"/>, local to its container. </summary>
        public float Y => Lerp(CurrentSpot.Y, (NextSpot ?? CurrentSpot).Y, MoveProgress);

        /// <summary> The item that this instance is currently holding, if applicable. </summary>
        public Stock<TC, TSpot>? Holding { get; set; }
        public Stock<TC, TSpot>? ClaimedStock { get; private set; }

        public Job<TC, TSpot>? Job { get; protected set; }

        /// <summary> Linearly interpolate between two values, by amount /f/. </summary>
        private float Lerp(float a, float b, float f) => a * (1 - f) + b * f;

        /// <summary> The block that this <see cref="Agent"/> is currently pathing to. Null if there is no path. </summary>
        public TSpot? PathingTo => Path.Count > 0 ? Path.Last() : null;

        protected Stack<TSpot> Path {
            get => this._path ?? (this._path = new Stack<TSpot>());
            set => this._path = value;
        }

        public Agent(TC container, TSpot floor) {
            Container = container;
            CurrentSpot = floor;
        }

        #region Pathing
        /// <summary>
        /// Returns whether or not the specified Spot is accessible to this <see cref="Agent"/>.
        /// </summary>
        public bool IsAccessible(TSpot target) {
            return Dijkstra.IsAccessible(Container, NextSpot??CurrentSpot, target);
        }
        /// <summary>
        /// Returns whether or not this <see cref="Agent"/> can access a Spot that matches <paramref name="destination"/>.
        /// </summary>
        public bool IsAccessible(IsAtDestination<TSpot> destination) {
            return Dijkstra.IsAccessible(Container, NextSpot??CurrentSpot, destination);
        }
        /// <summary>
        /// Finds the closest accessible Spot that matches <paramref name="destination"/>.
        /// </summary>
        public TSpot? FindAccessible(IsAtDestination<TSpot> destination) {
            return Dijkstra.FindPath(Container, NextSpot ?? CurrentSpot, destination).LastOrDefault();
        }

        /// <summary> Have this <see cref="Agent"/> path to the specified <see cref="Block"/>. </summary>
        public void PathTo(TSpot target) {
            Path = Dijkstra.FindPath(Container, NextSpot??CurrentSpot, target);
        }
        /// <summary> Have this <see cref="Agent"/> path to the first <see cref="Block"/> that matches /destination/. </summary>
        public void PathTo(IsAtDestination<TSpot> destination) {
            Path = Dijkstra.FindPath(Container, NextSpot??CurrentSpot, destination);
        }
        #endregion

        #region Stock Claiming
        /// <summary>
        /// Has the current Agent claim the specified <see cref="Stock"/>.
        /// </summary>
        public void ClaimStock(Stock<TC, TSpot> stock) {
            //
            // Throw an exception if we already have a claim.
            if(ClaimedStock != null) {
                throw new InvalidOperationException("This Agent has already claimed stock!");
            }
            stock.Claim(this);
            ClaimedStock = stock;
        }
        /// <summary>
        /// Has the current Agent unclaim the specified Stock.
        /// </summary>
        public void UnclaimStock(Stock<TC, TSpot> stock) {
            if(ClaimedStock is null) {                                                       // If we haven't claimed anything,
                throw new InvalidOperationException("This Agent hasn't claimed any Stock!"); //     throw an exception.
            }                                                                                //
            if(ClaimedStock != stock) {                                                      // If the stock isn't the one we've claimed,
                throw new InvalidOperationException("Not the correct Stock!");               //     throw an exception.
            }
            ClaimedStock.Unclaim(this); // Unclaim the Stock.
            ClaimedStock = null;        // Unassign it.
        }

        public bool Equals(IStockClaimant other) => other == this;
        #endregion

        #region IUpdatable Implementation
        void IUpdatable.Update(Master master, Time delta) => Update(master, delta);
        /// <summary> The update loop of this <see cref="Agent"/>; is called every frame. </summary>
        protected virtual void Update(Master master, Time delta) {
            if(Job == null) {                         // If this agent has no job,
                Job = Container.GetWorkableJob(this); //     get a workable job from the ship,
                if(Job != null)                       //     If there was a workable job,
                    Job.Worker = this;                //         assign this agent to it.
            }

            if(Job?.IsCancelled ?? false) { // If the job has been cancelled,
                Container.RemoveJob(Job!);  //     remove it from the container,
                Job = null;                 //     and unassign it.
            }
            if(Job != null) {                     // If there is a job:
                if(Job.Qualify(this, out _)) {    //     If the job is workable,
                    if(Job.Work(this, delta)) {   //         work it. If it's done,
                        Container.RemoveJob(Job); //             remove the job from the ship,
                        Job = null;               //             and unassign it.
                    }                             //
                }                                 //
                else {                            //     If the job is not workable,
                    Job.Worker = null;            //         unassign this agent from the job,
                    Job = null;                   //         and unset it.
                }
            }

            if(NextSpot == null && Path.Count > 0) // If we're on a path but aren't moving towards a block,
                NextSpot = Path.Pop();             //     set the next block as the next step on the math.

            if(NextSpot != null) {             // If we are moving towards a block:
                MoveProgress += delta * 1.5f;  // increment our progress towards it.
                                               //
                if(MoveProgress >= 1) {        // If we have reached the block,
                    CurrentSpot = NextSpot;    //     set it as our current block.
                    if(Path.Count > 0)         //     If we are currently on a path,
                        NextSpot = Path.Pop(); //         set the next block as the next step on the path.
                    else                       //     If we are not on a path,
                        NextSpot = null;       //         unassign the next block.
                                               //
                    if(NextSpot != null)       //     If we are still moving towards a block,
                        MoveProgress -= 1f;    //         subtract 1 from our move progress.
                    else                       //     If we are no longer moving towrards a block,
                        MoveProgress = 0;      //         set our move progress to be 0.
                }
            }
        }
        #endregion

        #region IDrawable Implementation
        void IDrawable.Draw(Master master) => Draw(master);
        /// <summary> Draws this <see cref="Agent{TC, TSpot}"/> onscreen. </summary>
        protected abstract void Draw(Master master);
        #endregion

        #region IScreenSpaceTarget Implementation
        int UI.IScreenSpaceTarget.X => ScreenTarget.X;
        int UI.IScreenSpaceTarget.Y => ScreenTarget.Y;
        /// <summary> The center of this Agent, in screen-space. </summary>
        protected abstract PointI ScreenTarget { get; }
        #endregion

        #region IFocusable Implementation
        protected bool IsFocused { get; private set; }
        bool IFocusable.IsFocused { set => IsFocused = value; }
        IFocusMenuProvider IFocusable.GetProvider(Master master) => GetFocusProvider(master);
        protected abstract IFocusMenuProvider GetFocusProvider(Master master);
        #endregion
    }
}

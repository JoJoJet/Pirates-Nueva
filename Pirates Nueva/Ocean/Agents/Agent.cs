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
    public abstract class Agent<TC, TSpot> : IStockClaimant<TC, TSpot>, IUpdatable, IDrawable<TC>, IFocusable, UI.IScreenSpaceTarget
        where TC    : class, IAgentContainer<TC, TSpot>
        where TSpot : class, IAgentSpot<TC, TSpot>
    {
        private Stack<TSpot>? path;

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

        /// <summary> The end of this <see cref="Agent"/>'s current path. Null if there is no path. </summary>
        public TSpot? PathEnd => this.path?.Count > 0 ? this.path.Last() : null;

        /// <summary> Whether or not this <see cref="Agent"/> currently has a path. </summary>
        public bool HasPath => this.path?.Count > 0;

        /// <summary> Checks if the specified Spot could be the destination. </summary>
        protected IsAtDestination<TSpot>? Destination { get; private set; }

        public Agent(TC container, TSpot floor) {
            Container = container;
            CurrentSpot = floor;
        }

        #region Pathing
        /// <summary> Returns whether or not the specified Spot is accessible to this <see cref="Agent"/>. </summary>
        public bool IsAccessible(TSpot target)
            => Dijkstra.IsAccessible(Container, NextSpot??CurrentSpot, target);
        /// <summary>
        /// Returns whether or not this <see cref="Agent"/> can access a Spot that matches <paramref name="destination"/>.
        /// </summary>
        public bool IsAccessible(IsAtDestination<TSpot> destination)
            => Dijkstra.IsAccessible(Container, NextSpot??CurrentSpot, destination);
        /// <summary> Finds the closest accessible Spot that matches <paramref name="destination"/>. </summary>
        public TSpot? FindAccessible(IsAtDestination<TSpot> destination)
            => Dijkstra.FindPath(Container, NextSpot ?? CurrentSpot, destination).LastOrDefault();

        /// <summary> Has this <see cref="Agent"/> path to the specified <see cref="Block"/>. </summary>
        public void PathTo(TSpot target) {
            Destination = n => n.Equals(target);
            this.path = Dijkstra.FindPath(Container, NextSpot??CurrentSpot, target);
            foreach(var spot in this.path)
                spot.SubscribeOnDestroyed(OnPathDestroyed);
        }
        /// <summary> Has this <see cref="Agent"/> path to the first <see cref="Block"/> that matches /destination/. </summary>
        public void PathTo(IsAtDestination<TSpot> destination) {
            Destination = destination;
            this.path = Dijkstra.FindPath(Container, NextSpot??CurrentSpot, destination);
            foreach(var spot in this.path)
                spot.SubscribeOnDestroyed(OnPathDestroyed);
        }

        /// <summary>
        /// Grabs the next Spot from the path, and assigns it to <see cref="NextSpot"/>.
        /// <para /> Also handles subscribing and unsubscribing.
        /// </summary>
        private void PopPath() {
            NextSpot?.UnsubscribeOnDestroyed(OnNextDestroyed);   // Unsub from the old reference to the next spot.
            if(this.path?.Count > 0) {                           // If we're currenty on a path,
                NextSpot = this.path.Pop();                      // |   grab the next spot from the path.
                NextSpot.UnsubscribeOnDestroyed(OnPathDestroyed);// |   Unsubscribe from recalculating the path,
                NextSpot.SubscribeOnDestroyed(OnNextDestroyed);  // |   and subsribe to the next spot.
                                                                 // |
                if(this.path.Count == 0)                         // |   If that was the last spot in the path,
                    Destination = null;                          // |   |   unassign the reference to the destination.
            }                                                    // |
            else {                                               // If we are NOT on a path,
                NextSpot = null;                                 // |   remove our reference to the next spot.
            }
        }

        private static void OnCurrentDestroyed(TSpot spot) => throw new InvalidOperationException("The agent's current block was destroyed!");
        private void OnNextDestroyed(TSpot spot) {
            NextSpot = null;
            MoveProgress = 0f;
            RecalculatePath();
        }
        private void OnPathDestroyed(TSpot spot) => RecalculatePath();

        /// <summary> Finds a new path to the current <see cref="Destination"/>. </summary>
        private void RecalculatePath() {
            const string Sig = nameof(Agent<TC, TSpot>) + "." + nameof(RecalculatePath) + "()";
            if(Destination is null)
                throw new InvalidOperationException($"{Sig}: This Agent has no path to recalculate!");
            PathTo(Destination);
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

        bool IStockClaimant<TC, TSpot>.Equals(IStockClaimant<TC, TSpot> other) => other == this;
        void IStockClaimant<TC, TSpot>.Unclaim(Stock<TC, TSpot> stock) {
            if(stock == ClaimedStock) {
                ClaimedStock?.Unclaim(this);
                ClaimedStock = null;
            }
        }
        #endregion

        /// <summary> Linearly interpolates between two values, by amount /f/. </summary>
        private static float Lerp(float a, float b, float f) => a * (1 - f) + b * f;

        #region IUpdatable Implementation
        void IUpdatable.Update(Master master, Time delta) => Update(master, delta);
        /// <summary> The update loop of this <see cref="Agent"/>; is called every frame. </summary>
        protected virtual void Update(Master master, Time delta) {
            if(Job == null) {                         // If this agent has no job,
                Job = Container.GetWorkableJob(this); // |   get a workable job from the ship,
                if(Job != null)                       // |   If there was a workable job,
                    Job.Assign(this);                 // |   |   assign this agent to it.
            }                                         //
            if(Job?.IsCancelled ?? false) {           // If the job has been cancelled,
                Container.RemoveJob(Job!);            // |   remove it from the container,
                Job = null;                           // |   and unassign it.
            }                                         //
            if(Job != null) {                         // If there is a job:
                if(Job.Qualify(this, out _)) {        // |   If the job is workable,
                    if(Job.Work(this, delta)) {       // |   |   work it. If it's done,
                        Container.RemoveJob(Job);     // |   |   |   remove the job from the ship,
                        Job = null;                   // |   |   |   and unassign it.
                    }                                 // |
                }                                     // |
                else {                                // |   If the job is not workable,
                    Job.Quit(this);                   // |   |   quit the job,
                    Job = null;                       // |   |   and unassign it.
                }
            }

            if(NextSpot == null && HasPath) {                               // If we're on a path but aren't moving yet,
                PopPath();                                                  // |   start moving down the path.
            }                                                               //
            if(NextSpot != null) {                                          // When we're moving on a path:
                MoveProgress += delta * 1.5f;                               // Increment our progress towards the next spot.
                                                                            //
                if(MoveProgress >= 1) {                                     // If we've reached the next spot,
                    CurrentSpot.UnsubscribeOnDestroyed(OnCurrentDestroyed); // |   unsub from the old spot,
                    CurrentSpot = NextSpot;                                 // |   assign the next as the current spot,
                    CurrentSpot.SubscribeOnDestroyed(OnCurrentDestroyed);   // |   and sub to the new one.
                                                                            // |   
                    PopPath();                                              // |   Find an new next spot.
                    if(NextSpot != null)                                    // |   If there's still another spot,
                        MoveProgress -= 1f;                                 // |   |   subtract 1 from our move progress.
                    else                                                    // |   If there's no more spots,
                        MoveProgress = 0f;                                  // |   |   set out move progress to 0.
                }
            }
        }
        #endregion

        #region IDrawable Implementation
        void IDrawable<TC>.Draw(ILocalDrawer<TC> drawer) => Draw(drawer);
        /// <summary> Draws this <see cref="Agent{TC, TSpot}"/> onscreen. </summary>
        protected virtual void Draw(ILocalDrawer<TC> drawer) {
            var tex = Resources.LoadSprite("agent");

            drawer.DrawCenter(tex, X, Y, width: 1, height: 1);

            (Holding as IDrawable<TC>)?.Draw(drawer);
        }
        #endregion

        #region IScreenSpaceTarget Implementation
        int UI.IScreenSpaceTarget.X => (int)ScreenTarget.X;
        int UI.IScreenSpaceTarget.Y => (int)ScreenTarget.Y;
        private PointF ScreenTarget => Container.Transformer.PointToRoot(new PointF(X + 0.5f, Y + 0.5f));
        #endregion

        #region IFocusable Implementation
        protected bool IsFocused { get; private set; }
        bool IFocusable.IsFocused { set => IsFocused = value; }
        IFocusMenuProvider IFocusable.GetProvider(Master master) => GetFocusProvider(master);
        protected abstract IFocusMenuProvider GetFocusProvider(Master master);
        #endregion
    }
}

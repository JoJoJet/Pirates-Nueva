using System;

namespace Pirates_Nueva.Ocean.Agents
{
    /// <summary>
    /// A requirement with only 2 cases; a binary requirement.
    /// </summary>
    public abstract class SimpleRequirement<TC, TSpot> : Job<TC, TSpot>.Requirement
        where TC    : class, IAgentContainer<TC, TSpot>
        where TSpot : class, IAgentSpot<TC, TSpot>
    {
        /// <param name="executor">A Toil that, when completed, should fulfill the current Requirement.</param>
        protected SimpleRequirement(Job<TC, TSpot>.Toil? executor = null) : base(executor) {  }

        protected sealed override bool Qualify(Agent<TC, TSpot> worker, out string reason) {
            if(Check(worker)) {  // If the requirement is met,
                reason = "";     //     output no reason,
                return true;     //     and return true.
            }                    //
            else {               // If the requirement is NOT met,
                reason = Reason; //     output the reason,
                return false;    //     and return true.
            }
        }

        /// <summary>
        /// Checks if the specified Agent meets this requirement.
        /// </summary>
        protected abstract bool Check(Agent<TC, TSpot> worker);
        /// <summary>
        /// The reason that this requirement is not met, if applicable.
        /// </summary>
        protected abstract string Reason { get; }
    }

    /// <summary>
    /// Requires that an Agent be adjacent to the Job or Toil.
    /// </summary>
    public class IsAdjacentToToil<TC, TSpot> : SimpleRequirement<TC, TSpot>
        where TC    : class, IAgentContainer<TC, TSpot>
        where TSpot : class, IAgentSpot<TC, TSpot>
    {
        public IsAdjacentToToil(Job<TC, TSpot>.Toil? executor = null) : base(executor) {  }

        protected override bool Check(Agent<TC, TSpot> worker)
            => PointF.SqrDistance((worker.X, worker.Y), Toil.Index) == 1;
        protected override string Reason => "Worker is not adjacent to the job.";
    }
    /// <summary>
    /// Has an Agent place a <see cref="Block"/> in a <see cref="Ship"/>.
    /// </summary>
    public class PlaceBlock : Job<Ship, Block>.Action
    {
        /// <summary> The ID of the <see cref="Block"/> to place. </summary>
        public string PlaceID { get; }
        /// <summary> The progress, out of 1, towards placing the <see cref="Block"/>. </summary>
        public float Progress { get; protected set; }

        public PlaceBlock(string id) {
            PlaceID = id;
        }

        /// <summary>
        /// Have the specified <see cref="Agent"/> work at placing this block.
        /// </summary>
        /// <returns>Whether or not the action was just completed.</returns>
        protected override bool Work(Agent<Ship, Block> worker, Time delta) {
            Progress += delta;                                 // Increment the building progress.

            if(Progress >= 1) {                                // If the progress has reached '1',
                var def = BlockDef.Get(PlaceID);               //
                Container.PlaceBlock(def, Toil.X, Toil.Y);     //     place a block at the toil's position,
                return true;                                   //     and return true.
            }                                                  //
            else {                                             // If the progress has yet to reach '1',
                return false;                                  //     return false.
            }
        }

        protected override void Draw(ILocalDrawer<Ship> drawer, Agent<Ship, Block>? worker) {
            var def = BlockDef.Get(PlaceID);
            var tex = Resources.LoadTexture(def.TextureID);
            
            drawer.Draw(tex, Toil.X, Toil.Y, 1, 1, in UI.Color.Lime);
        }
    }
    
    /// <summary>
    /// Requires that an Agent be able to path to the Job or Toil.
    /// </summary>
    public class IsAccessibleToToilAdj<TC, TSpot> : SimpleRequirement<TC, TSpot>
        where TC    : class, IAgentContainer<TC, TSpot>
        where TSpot : class, IAgentSpot<TC, TSpot>
    {
        public IsAccessibleToToilAdj(Job<TC, TSpot>.Toil? executor = null) : base(executor) {  }

        protected override bool Check(Agent<TC, TSpot> worker)
            => worker.IsAccessible(n => PointI.SqrDistance(n.Index, Toil.Index) == 1);
        protected override string Reason => "Worker can't path to the spot.";
    }

    /// <summary>
    /// Has an agent path to some spot.
    /// </summary>
    public abstract class PathTo<TC, TSpot> : Job<TC, TSpot>.Action
        where TC    : class, IAgentContainer<TC, TSpot>
        where TSpot : class, IAgentSpot<TC, TSpot>
    {
        private Agent<TC, TSpot>? worker;
        protected Agent<TC, TSpot> Worker => this.worker
            ?? throw new InvalidOperationException($"Property '{nameof(Worker)}' can't be accessed right now!");

        protected abstract bool IsAtDestination(TSpot spot);

        protected override bool Work(Agent<TC, TSpot> worker, Time delta) {
            this.worker = worker;
            if(worker.PathingTo == null) {                     // If the worker is currently still:
                if(IsAtDestination(worker.CurrentSpot)) {      //     If its standing next to the toil,
                    this.worker = null;
                    return true;                               //         return true.
                }                                              //
                else {                                         //     If its standing away from the toil,
                    worker.PathTo(IsAtDestination);            //         have it path to a spot adjacent to the toil,
                    this.worker = null;
                    return false;                              //         and return false.
                }                                              //
            }                                                  //
            else {                                             // If the worker is currently pathing:
                if(IsAtDestination(worker.PathingTo) == false) //     if the worker's destination is not adjacent to the toil,
                    worker.PathTo(IsAtDestination);            //         have it path to a spot adjacent to the toil.
                this.worker = null;
                return false;                                  //     Return false.
            }
        }
    }
    /// <summary>
    /// Has an Agent path to a spot adjacent to the Job or Toil.
    /// </summary>
    public class PathToToilAdjacent<TC, TSpot> : PathTo<TC, TSpot>
        where TC    : class, IAgentContainer<TC, TSpot>
        where TSpot : class, IAgentSpot<TC, TSpot>
    {
        protected override bool IsAtDestination(TSpot spot) => PointI.SqrDistance(spot.Index, Toil.Index) == 1;
    }




    /// <summary>
    /// Requires that an agent be at the job or toil.
    /// </summary>
    public class IsAtToil<TC, TSpot> : SimpleRequirement<TC, TSpot>
        where TC    : class, IAgentContainer<TC, TSpot>
        where TSpot : class, IAgentSpot<TC, TSpot>
    {
        public IsAtToil(Job<TC, TSpot>.Toil? executor = null) : base(executor) {  }

        protected override bool Check(Agent<TC, TSpot> worker)
            => PointI.SqrDistance(worker.CurrentSpot.Index, Toil.Index) == 0;
        protected override string Reason => "Worker is not at the job.";
    }
    /// <summary>
    /// Requires that an agent be holding a specific type of <see cref="Stock{TC, TSpot}"/>.
    /// </summary>
    public class IsHolding<TC, TSpot> : SimpleRequirement<TC, TSpot>
        where TC    : class, IAgentContainer<TC, TSpot>
        where TSpot : class, IAgentSpot<TC, TSpot>
    {
        public ItemDef Holding { get; }

        public IsHolding(ItemDef holding, Job<TC, TSpot>.Toil? executor = null)
            : base(executor)
            => Holding = holding;

        protected override bool Check(Agent<TC, TSpot> worker) => worker.Holding?.Def == Holding;
        protected override string Reason => Holding != null
                                            ? "Worker is not holding the correct item."
                                            : "Worker's hands must be empty.";
    }

    /// <summary>
    /// Has an agent place the item that they are holding at their current spot.
    /// </summary>
    public class PlaceStockAtAgent<TC, TSpot> : Job<TC, TSpot>.Action
        where TC    : class, IAgentContainer<TC, TSpot>
        where TSpot : class, IAgentSpot<TC, TSpot>
    {
        protected override bool Work(Agent<TC, TSpot> worker, Time delta) {
            //
            // Throw an exception if the worker's hands are empty.
            if(worker.Holding == null)
                throw new InvalidOperationException("The worker is not holding anything!");
            worker.Holding.Place(worker.CurrentSpot);
            return true;
        }
    }


    /// <summary>
    /// Requires that an agent be able to path to the Job or Toil.
    /// </summary>
    public class IsToilAccessible<TC, TSpot> : SimpleRequirement<TC, TSpot>
        where TC    : class, IAgentContainer<TC, TSpot>
        where TSpot : class, IAgentSpot<TC, TSpot>
    {
        public IsToilAccessible(Job<TC, TSpot>.Toil? executor = null) : base(executor) {  }

        protected override bool Check(Agent<TC, TSpot> worker)
            => worker.IsAccessible(sp => sp.Index == Toil.Index);
        protected override string Reason => "Worker can't path to the job.";
    }

    /// <summary>
    /// Has an agent path to the Job or Toil.
    /// </summary>
    public class PathToToil<TC, TSpot> : PathTo<TC, TSpot>
        where TC    : class, IAgentContainer<TC, TSpot>
        where TSpot : class, IAgentSpot<TC, TSpot>
    {
        protected override bool IsAtDestination(TSpot spot) => PointI.SqrDistance(spot.Index, Toil.Index) == 0;
    }
    

    /// <summary>
    /// Requires that an Agent be standing at its claimed Stock.
    /// </summary>
    public class IsStandingAtClaimedStock<TC, TSpot> : SimpleRequirement<TC, TSpot>
        where TC    : class, IAgentContainer<TC, TSpot>
        where TSpot : class, IAgentSpot<TC, TSpot>
    {
        public IsStandingAtClaimedStock(Job<TC, TSpot>.Toil? executor = null) : base(executor) {  }

        protected override bool Check(Agent<TC, TSpot> worker)
            => worker.ClaimedStock?.Spot == worker.CurrentSpot;
        protected override string Reason => "Worker is not standing at its claimed Stock.";
    }

    /// <summary>
    /// Has an Agent pick up its claimed stock at which it is standing. <para />
    /// Will throw an <see cref="InvalidOperationException"/> if the worker hasn't claimed any Stock,
    /// or if it is not standing at the claimed stock.
    /// </summary>
    public class PickUpClaimedStock<TC, TSpot> : Job<TC, TSpot>.Action
        where TC    : class, IAgentContainer<TC, TSpot>
        where TSpot : class, IAgentSpot<TC, TSpot>
    {
        /// <summary>Whether or not the agent should unclaim the stock after picking it up.</summary>
        public bool UnclaimAfter { get; }
        public bool DropOnStopped { get; }

        public PickUpClaimedStock(bool unclaimAfter, bool dropOnStopped = true) {
            UnclaimAfter = unclaimAfter;
            DropOnStopped = dropOnStopped;
        }

        protected override bool Work(Agent<TC, TSpot> worker, Time delta) {
            var stock = worker.ClaimedStock;                 // Store a local reference to the claimed stock.
            if(stock is null)                                // If the worker hasn't claimed any Stock,
                throw new InvalidOperationException(         // |   throw an exception.
                    "The worker hasn't claimed any Stock!"); //
            if(stock.Spot == worker.CurrentSpot) {           // If the worker is standing at its claimed Stock,
                stock.PickUp(worker);                        // |   pick it up.
                if(UnclaimAfter)                             // |   If we're told to unclaim it,
                    worker.UnclaimStock(stock);              // |   |   unclaim it.
                return true;                                 // |   Return true.
            }                                                //
            else {                                           // If the worker is NOT standing at its claimed Stock,
                throw new InvalidOperationException(         // |   throw an exception.
                    "The worker isn't standing at its claimed Stock!");
            }
        }

        protected override void OnStopped(Agent<TC, TSpot> worker) {
            if(DropOnStopped && worker.Holding != null) {
                worker.Holding.Place(worker.CurrentSpot);
            }
        }
    }


    /// <summary>
    /// Requires that an Agent have claimed some Stock that is accessible to it.
    /// </summary>
    public class IsClaimedStockAccessible<TC, TSpot> : Job<TC, TSpot>.Requirement
        where TC    : class, IAgentContainer<TC, TSpot>
        where TSpot : class, IAgentSpot<TC, TSpot>
    {
        public IsClaimedStockAccessible(Job<TC, TSpot>.Toil? executor = null) : base(executor) {  }

        protected override bool Qualify(Agent<TC, TSpot> worker, out string reason) {
            if(worker.ClaimedStock is Stock<TC, TSpot>) {                // If the worker has claimed stock:
                if(worker.ClaimedStock.Spot is TSpot spot) {             // |   If the stock is on the ground:
                    if(worker.IsAccessible(spot)) {                      // |   |   If the stock is accessible,
                        reason = "";                                     // |   |   |   return true.
                        return true;                                     // |   |
                    }                                                    // |   |
                    else {                                               // |   |   If the stock isn't accessible, 
                        reason = "The claimed stock is not accessible."; // |   |   |   return false.
                        return false;                                    // |
                    }                                                    // |
                }                                                        // |
                else {                                                   // |   If the stock is NOT on the ground,
                    reason = "The claimed stock is not on the ground.";  // |   |   return false.
                    return false;                                        //
                }                                                        //
            }                                                            //
            else {                                                       // If the worker has NOT claimed stock,
                reason = "Worker hasn't claimed any stock.";             // |   return false.
                return false;
            }
        }
    }

    public class PathToClaimedStock<TC, TSpot> : PathTo<TC, TSpot>
        where TC    : class, IAgentContainer<TC, TSpot>
        where TSpot : class, IAgentSpot<TC, TSpot>
    {
        protected override bool IsAtDestination(TSpot spot)
            => spot == (Worker.ClaimedStock ?? Throw()).Spot;
        private static Stock<TC, TSpot> Throw() => throw new InvalidOperationException("Worker doesn't have any claimed Stock!");
    }


    /// <summary>
    /// Requires that a specific type of Stock be accessible to an Agent.
    /// </summary>
    public class IsStockAccessible<TC, TSpot> : SimpleRequirement<TC, TSpot>
        where TC    : class, IAgentContainer<TC, TSpot>
        where TSpot : class, IAgentSpot<TC, TSpot>
    {
        public StockSelector<TC, TSpot> Selector { get; }
        public bool RequireUnclaimed { get; }

        public IsStockAccessible(StockSelector<TC, TSpot> selector, bool requireUnclaimed = true, Job<TC, TSpot>.Toil? executor = null)
            : base(executor)
        {
            Selector = selector;
            RequireUnclaimed = requireUnclaimed;
        }

        protected override bool Check(Agent<TC, TSpot> worker)
            => worker.IsAccessible(sp => StockChecker<TC, TSpot>.Check(worker, sp, Selector, RequireUnclaimed));

        protected override string Reason => "Worker cannot path to the correct item.";
    }

    /// <summary>
    /// Has an Agent claim its closest accessible unclaimed Stock. <para />
    /// May throw an <see cref="InvalidOperationException"/> if the Agent has already claimed something.
    /// </summary>
    public class ClaimAccessibleStock<TC, TSpot> : Job<TC, TSpot>.Action
        where TC    : class, IAgentContainer<TC, TSpot>
        where TSpot : class, IAgentSpot<TC, TSpot>
    {
        public StockSelector<TC, TSpot> Selector { get; }
        public bool UnclaimOnStopped { get; }

        public ClaimAccessibleStock(StockSelector<TC, TSpot> selector, bool unclaimOnStopped = true) {
            Selector = selector;
            UnclaimOnStopped = unclaimOnStopped;
        }

        protected override bool Work(Agent<TC, TSpot> worker, Time delta) {
            //
            // If there's an accessible spot with unclaimed stock of specified type,
            // claim it and return true.
            if(worker.FindAccessible(checkSpot) is TSpot spot) {
                worker.ClaimStock(spot.Stock!);
                return true;
            }
            //
            // If there's no accessible stock, return false.
            else {
                return false;
            }

            bool checkSpot(TSpot s) => StockChecker<TC, TSpot>.Check(worker, s, Selector, true);
        }

        protected override void OnStopped(Agent<TC, TSpot> worker) {
            if(UnclaimOnStopped && worker.ClaimedStock != null) {
                worker.UnclaimStock(worker.ClaimedStock);
            }
        }
    }

    
    /// <summary>
    /// Finds, paths to, and picks up a <see cref="Stock{TC, TSpot}"/>
    /// that matches the <see cref="StockSelector{TC, TSpot}"/>,
    /// if it exists and is accessible.
    /// </summary>
    public class FindAndPickUpStock<TC, TSpot> : Job<TC, TSpot>.Toil
        where TC    : class, IAgentContainer<TC, TSpot>
        where TSpot : class, IAgentSpot<TC, TSpot>
    {
        public StockSelector<TC, TSpot> Selector { get; }

        public FindAndPickUpStock(StockSelector<TC, TSpot> selector, Job<TC, TSpot>.Toil? executor = null)
            : base(
                  //
                  // Pick up the claimed Stock,
                  // if it exists and we're standing at it.
                  new PickUpClaimedStock<TC, TSpot>(unclaimAfter: true),
                  new IsStandingAtClaimedStock<TC, TSpot>(
                      executor: new Job<TC, TSpot>.Toil(
                          //
                          // Walk to the claimed Stock,
                          // if it exists and is accessible.
                          new PathToClaimedStock<TC, TSpot>(),
                          new IsClaimedStockAccessible<TC, TSpot>(
                              executor: new Job<TC, TSpot>.Toil(
                                  //
                                  // Claim some Stock,
                                  // if it exists and is acessible.
                                  new ClaimAccessibleStock<TC, TSpot>(selector),
                                  new IsStockAccessible<TC, TSpot>(selector, executor: executor)
                                  )
                              )
                          )
                      )
                  )
        {
            Selector = selector;
        }
    }



    internal static class StockChecker<TC, TSpot>
        where TC    : class, IAgentContainer<TC, TSpot>
        where TSpot : class, IAgentSpot<TC, TSpot>
    {
        /// <summary>
        /// Checks if the specified spot contains a Stock that is
        /// unclaimed and has the specified <see cref="ItemDef"/>.
        /// </summary>
        public static bool Check(Agent<TC, TSpot> worker, TSpot spot, StockSelector<TC, TSpot> selector, bool requireUnclaimed)
            => spot.Stock is Stock<TC, TSpot> stock
               ? selector.Qualify(stock) && (!requireUnclaimed || (stock.Claimant?.Equals(worker) ?? true))
               : false;
    }
}

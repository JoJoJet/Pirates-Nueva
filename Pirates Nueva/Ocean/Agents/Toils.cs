using System;
#nullable enable

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
    public class IsAdjacentTo<TC, TSpot> : SimpleRequirement<TC, TSpot>
        where TC    : class, IAgentContainer<TC, TSpot>
        where TSpot : class, IAgentSpot<TC, TSpot>
    {
        public IsAdjacentTo(Job<TC, TSpot>.Toil? executor = null) : base(executor) {  }

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

        protected override void Draw(Master master, Agent<Ship, Block>? worker) {
            var def = BlockDef.Get(PlaceID);
            var tex = master.Resources.LoadTexture(def.TextureID);
            
            var (seaX, seaY) = Container.ShipPointToSea(Toil.Index + (0, 1));  // The top left of the Block's texture in sea-space.
            var (screenX, screenY) = Container.Sea.SeaPointToScreen(seaX, seaY); // The top left of the Block's texture in screen-space.
            master.Renderer.DrawRotated(
                tex,
                screenX, screenY,
                Container.Sea.PPU, Container.Sea.PPU,
                -Container.Angle, (0, 0),
                UI.Color.Lime
                );
        }
    }
    
    /// <summary>
    /// Requires that an Agent be able to path to the Job or Toil.
    /// </summary>
    public class IsAccessibleAdj<TC, TSpot> : SimpleRequirement<TC, TSpot>
        where TC    : class, IAgentContainer<TC, TSpot>
        where TSpot : class, IAgentSpot<TC, TSpot>
    {
        public IsAccessibleAdj(Job<TC, TSpot>.Toil? executor = null) : base(executor) {  }

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
        protected abstract bool IsAtDestination(TSpot spot);

        protected override bool Work(Agent<TC, TSpot> worker, Time delta) {
            if(worker.PathingTo == null) {                     // If the worker is currently still:
                if(IsAtDestination(worker.CurrentSpot)) {      //     If its standing next to the toil,
                    return true;                               //         return true.
                }                                              //
                else {                                         //     If its standing away from the toil,
                    worker.PathTo(IsAtDestination);            //         have it path to a spot adjacent to the toil,
                    return false;                              //         and return false.
                }                                              //
            }                                                  //
            else {                                             // If the worker is currently pathing:
                if(IsAtDestination(worker.PathingTo) == false) //     if the worker's destination is not adjacent to the toil,
                    worker.PathTo(IsAtDestination);            //         have it path to a spot adjacent to the toil.
                return false;                                  //     Return false.
            }
        }
    }
    /// <summary>
    /// Has an Agent path to a spot adjacent to the Job or Toil.
    /// </summary>
    public class PathToAdjacent<TC, TSpot> : PathTo<TC, TSpot>
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
    public class IsAccessible<TC, TSpot> : SimpleRequirement<TC, TSpot>
        where TC    : class, IAgentContainer<TC, TSpot>
        where TSpot : class, IAgentSpot<TC, TSpot>
    {
        public IsAccessible(Job<TC, TSpot>.Toil? executor = null) : base(executor) {  }

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
    /// Requires that an Agent be standing on a Stock of specified type.
    /// </summary>
    public class IsStandingAtStock<TC, TSpot> : SimpleRequirement<TC, TSpot>
        where TC    : class, IAgentContainer<TC, TSpot>
        where TSpot : class, IAgentSpot<TC, TSpot>
    {
        public ItemDef StockType { get; }

        public IsStandingAtStock(ItemDef stockType, Job<TC, TSpot>.Toil? executor = null)
            : base(executor)
            => StockType = stockType;

        protected override bool Check(Agent<TC, TSpot> worker)
            => worker.CurrentSpot.Stock?.Def == StockType;
        protected override string Reason => "Worker is not standing at the correct item.";
    }

    /// <summary>
    /// Has an Agent pick up the Stock that they are standing on.
    /// </summary>
    public class PickUpStock<TC, TSpot> : Job<TC, TSpot>.Action
        where TC    : class, IAgentContainer<TC, TSpot>
        where TSpot : class, IAgentSpot<TC, TSpot>
    {
        protected override bool Work(Agent<TC, TSpot> worker, Time delta) {
            //
            // If there's stock, pick it up.
            if(worker.CurrentSpot.Stock is Stock<TC, TSpot> stock) {
                stock.PickUp(worker);
                return true;
            }
            //
            // If there's no stock, throw an exception.
            else {
                throw new InvalidOperationException("There is nothing for the worker to pick up!");
            }
        }
    }

    /// <summary>
    /// Requires that a specific type of Stock be accessible to an Agent.
    /// </summary>
    public class IsStockAcesible<TC, TSpot> : SimpleRequirement<TC, TSpot>
        where TC    : class, IAgentContainer<TC, TSpot>
        where TSpot : class, IAgentSpot<TC, TSpot>
    {
        public ItemDef StockType { get; }

        public IsStockAcesible(ItemDef stockType, Job<TC, TSpot>.Toil? executor = null)
            : base(executor)
            => StockType = stockType;

        protected override bool Check(Agent<TC, TSpot> worker)
            => worker.IsAccessible(sp => sp.Stock?.Def == StockType);
        protected override string Reason => "Worker cannot path to the correct item.";
    }

    /// <summary>
    /// Has an Agent path to a specific type of Stock.
    /// </summary>
    public class PathToStock<TC, TSpot> : PathTo<TC, TSpot>
        where TC    : class, IAgentContainer<TC, TSpot>
        where TSpot : class, IAgentSpot<TC, TSpot>
    {
        public ItemDef StockType { get; }

        public PathToStock(ItemDef stockType) => StockType = stockType;

        protected override bool IsAtDestination(TSpot spot) => spot.Stock?.Def == StockType;
    }
}

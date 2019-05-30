using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pirates_Nueva.Ocean.Agents
{
    /// <summary>
    /// Requires that an Agent be adjacent to the Job or Toil.
    /// </summary>
    /// <typeparam name="TC">The type of Container that this Job exists in.</typeparam>
    public class IsAdjacentTo<TC, TSpot> : Job<TC, TSpot>.Requirement
        where TC    : class, IAgentContainer<TC, TSpot>
        where TSpot : class, IAgentSpot<TC, TSpot>
    {
        /// <summary>
        /// Check if the specified Agent is adjacent to the <see cref="Job.Toil"/>.
        /// </summary>
        protected override bool Qualify(Agent<TC, TSpot> worker, out string reason) {
            if(PointF.SqrDistance((worker.X, worker.Y), Toil.Index) == 1f) { // If the worker is adjacent to the toil,
                reason = "";                                                 //     set the reason as an empty string,
                return true;                                                 //     and return true.
            }                                                                //
            else {                                                           // If the worker is NOT adjacent to the toil,
                reason = "Worker is not adjacent to the job.";               //     set that as the reason,
                return false;                                                //     and return false.
            }
        }
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

        protected override void Draw(Master master, Agent<Ship, Block> worker) {
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
    /// <typeparam name="TC">The type of Container that this Job exists in.</typeparam>
    public class IsAccessibleAdj<TC, TSpot> : Job<TC, TSpot>.Requirement
        where TC    : class, IAgentContainer<TC, TSpot>
        where TSpot : class, IAgentSpot<TC, TSpot>
    {
        protected override bool Qualify(Agent<TC, TSpot> worker, out string reason) {
            if(worker.IsAccessible(isAdjacent)) {           // If a spot adjacent to the toil is accessible to the worker,
                reason = "";                                //     set the reason as an empty string,
                return true;                                //     and return true.
            }                                               //
            else {                                          // If a spot is NOT accessible to the worker,
                reason = "Worker can't path to the spot.";  //     set that as the reason,
                return false;                               //     and return false.
            }

            bool isAdjacent(TSpot n) => PointI.SqrDistance(n.Index, Toil.Index) == 1;
        }
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
    /// <typeparam name="TC">The type of Container that this Job exists in.</typeparam>
    public class PathToAdjacent<TC, TSpot> : PathTo<TC, TSpot>
        where TC    : class, IAgentContainer<TC, TSpot>
        where TSpot : class, IAgentSpot<TC, TSpot>
    {
        protected override bool IsAtDestination(TSpot spot) => PointI.SqrDistance(spot.Index, Toil.Index) == 1;
    }




    /// <summary>
    /// Requires that an agent be at the job or toil.
    /// </summary>
    public class IsAtToil<TC, TSpot> : Job<TC, TSpot>.Requirement
        where TC    : class, IAgentContainer<TC, TSpot>
        where TSpot : class, IAgentSpot<TC, TSpot>
    {
        protected override bool Qualify(Agent<TC, TSpot> worker, out string reason) {
            if(PointI.SqrDistance(worker.CurrentSpot.Index, (Toil.Index)) == 0) {
                reason = "";
                return true;
            }
            else {
                reason = "Worker is not at the job.";
                return false;
            }
        }
    }
    /// <summary>
    /// Requires that an agent be holding a specific type of <see cref="Stock{TC, TSpot}"/>.
    /// </summary>
    public class IsHolding<TC, TSpot> : Job<TC, TSpot>.Requirement
        where TC    : class, IAgentContainer<TC, TSpot>
        where TSpot : class, IAgentSpot<TC, TSpot>
    {
        public ItemDef Holding { get; }

        public IsHolding(ItemDef holding) => Holding = holding;

        protected override bool Qualify(Agent<TC, TSpot> worker, out string reason) {
            if(worker.Holding?.Def == Holding) {
                reason = "";
                return true;
            }
            else if(Holding != null) {
                reason = "Worker is not holding the correct item.";
                return false;
            }
            else {
                reason = "Worker's hands must be empty.";
                return false;
            }
        }
    }

    /// <summary>
    /// Has an agent place the item that they are holding at their current spot.
    /// </summary>
    public class PlaceStockAtAgent<TC, TSpot> : Job<TC, TSpot>.Action
        where TC    : class, IAgentContainer<TC, TSpot>
        where TSpot : class, IAgentSpot<TC, TSpot>
    {
        protected override bool Work(Agent<TC, TSpot> worker, Time delta) {
            worker.Holding.Place(worker.CurrentSpot);
            return true;
        }
    }


    /// <summary>
    /// Requires that an agent be able to path to the Job or Toil.
    /// </summary>
    public class IsAccessible<TC, TSpot> : Job<TC, TSpot>.Requirement
        where TC    : class, IAgentContainer<TC, TSpot>
        where TSpot : class, IAgentSpot<TC, TSpot>
    {
        protected override bool Qualify(Agent<TC, TSpot> worker, out string reason) {
            if(worker.IsAccessible(sp => sp.Index == Toil.Index)) {
                reason = "";
                return true;
            }
            else {
                reason = "Worker can't path to the job.";
                return false;
            }
        }
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
    public class IsStandingAtStock<TC, TSpot> : Job<TC, TSpot>.Requirement
        where TC    : class, IAgentContainer<TC, TSpot>
        where TSpot : class, IAgentSpot<TC, TSpot>
    {
        public ItemDef StockType { get; }

        public IsStandingAtStock(ItemDef stockType) => StockType = stockType;

        protected override bool Qualify(Agent<TC, TSpot> worker, out string reason) {
            if(worker.CurrentSpot.Stock?.Def == StockType) {
                reason = "";
                return true;
            }
            else {
                reason = "Worker is not standing at the correct item.";
                return false;
            }
        }
    }

    /// <summary>
    /// Has an Agent pick up the Stock that they are standing on.
    /// </summary>
    public class PickUpStock<TC, TSpot> : Job<TC, TSpot>.Action
        where TC    : class, IAgentContainer<TC, TSpot>
        where TSpot : class, IAgentSpot<TC, TSpot>
    {
        protected override bool Work(Agent<TC, TSpot> worker, Time delta) {
            worker.CurrentSpot.Stock.PickUp(worker);
            return true;
        }
    }

    /// <summary>
    /// Requires that a specific type of Stock be accessible to an Agent.
    /// </summary>
    public class IsStockAcesible<TC, TSpot> : Job<TC, TSpot>.Requirement
        where TC    : class, IAgentContainer<TC, TSpot>
        where TSpot : class, IAgentSpot<TC, TSpot>
    {
        public ItemDef StockType { get; }

        public IsStockAcesible(ItemDef stockType) => StockType = stockType;

        protected override bool Qualify(Agent<TC, TSpot> worker, out string reason) {
            if(worker.IsAccessible(at)) {
                reason = "";
                return true;
            }
            else {
                reason = "Worker cannot path to the correct item.";
                return false;
            }

            bool at(TSpot spot) => spot.Stock?.Def == StockType;
        }
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

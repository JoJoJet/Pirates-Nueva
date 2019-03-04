using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Pirates_Nueva.Ocean.Agents
{
    public class IsAdjacentTo<TC, TSpot> : Job<TC, TSpot>.Requirement
        where TC    : class, IContainer<TC, TSpot>, Path.IGraph<TSpot>
        where TSpot : class, ISpot<TSpot>,          Path.INode<TSpot>
    {
        /// <summary>
        /// Check if the specified <see cref="Agent"/> is adjacent to the <see cref="Job.Toil"/>.
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
                Container.PlaceBlock(PlaceID, Toil.X, Toil.Y); //     place a block at the toil's position,
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
                Color.Lime
                );
        }
    }
    
    public class IsAccessibleAdj<TC, TSpot> : Job<TC, TSpot>.Requirement
        where TC    : class, IContainer<TC, TSpot>, Path.IGraph<TSpot>
        where TSpot : class, ISpot<TSpot>,          Path.INode<TSpot>
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

    public class PathToAdjacent<TC, TSpot> : Job<TC, TSpot>.Action
        where TC    : class, IContainer<TC, TSpot>, Path.IGraph<TSpot>
        where TSpot : class, ISpot<TSpot>,          Path.INode<TSpot>
    {
        protected override bool Work(Agent<TC, TSpot> worker, Time delta) {
            if(worker.PathingTo == null) {                // If the worker is currently still:
                if(isAdjacent(worker.CurrentSpot)) {      //     If its standing next to the toil,
                    return true;                          //         return true.
                }                                         //
                else {                                    //     If its standing away from the toil,
                    worker.PathTo(isAdjacent);            //         have it path to a spot adjacent to the toil,
                    return false;                         //         and return false.
                }                                         //
            }                                             //
            else {                                        // If the worker is currently pathing:
                if(isAdjacent(worker.PathingTo) == false) //     if the worker's destination is not adjacent to the toil,
                    worker.PathTo(isAdjacent);            //         have it path to a spot adjacent to the toil.
                return false;                             //     Return false.
            }
            
            bool isAdjacent(TSpot n) => PointI.SqrDistance(n.Index, Toil.Index) == 1;
        }
    }
}

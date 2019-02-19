using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Pirates_Nueva
{
    public class IsAdjacentTo : Job.Requirement
    {
        /// <summary>
        /// Check if the specified <see cref="Agent"/> is adjacent to the <see cref="Job.Toil"/>.
        /// </summary>
        protected override bool Qualify(Agent worker, out string reason) {
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
    public class PlaceBlock : Job.Action
    {
        /// <summary> The ID of the <see cref="Block"/> to place. </summary>
        public string PlaceID { get; }

        public PlaceBlock(string id) {
            PlaceID = id;
        }

        /// <summary>
        /// Have the specified <see cref="Agent"/> work at placing this block.
        /// </summary>
        /// <returns>Whether or not the action was just completed.</returns>
        protected override bool Work(Agent worker) {
            Toil.Ship.PlaceBlock(PlaceID, Toil.X, Toil.Y); // Place a block at the toil's position,
            return true;                                   // and return true.
        }

        protected override void Draw(Master master, Agent worker) {
            var def = BlockDef.Get(PlaceID);
            var tex = master.Resources.LoadTexture(def.TextureID);
            
            (float seaX, float seaY) = Toil.Ship.ShipPointToSea(Toil.Index + (0, 1));  // The top left of the Block's texture in sea-space.
            (int screenX, int screenY) = Toil.Ship.Sea.SeaPointToScreen(seaX, seaY); // The top left of the Block's texture in screen-space.
            master.Renderer.DrawRotated(tex, screenX, screenY, Block.Pixels, Block.Pixels, -Toil.Ship.Angle, (0, 0), Color.Lime);
        }
    }
    
    public class IsAccessibleAdj : Job.Requirement
    {
        protected override bool Qualify(Agent worker, out string reason) {
            if(worker.IsAccessible(isAdjacent)) {           // If a spot adjacent to the toil is accessible to the worker,
                reason = "";                                //     set the reason as an empty string,
                return true;                                //     and return true.
            }                                               //
            else {                                          // If a spot is NOT accessible to the worker,
                reason = "Worker can't path to the block."; //     set that as the reason,
                return false;                               //     and return false.
            }

            bool isAdjacent(Path.INode<Block> n) => PointI.SqrDistance((n as Block).Index, Toil.Index) == 1;
        }
    }

    public class PathToAdjacent : Job.Action
    {
        protected override bool Work(Agent worker) {
            if(worker.PathingTo == null) {                // If the worker is currently still:
                if(isAdjacent(worker.CurrentBlock)) {     //     If its standing next to the toil,
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
            
            bool isAdjacent(Path.INode<Block> n) => PointI.SqrDistance((n as Block).Index, Toil.Index) == 1;
        }
    }
}

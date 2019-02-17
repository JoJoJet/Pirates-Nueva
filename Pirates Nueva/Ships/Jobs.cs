using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pirates_Nueva
{
    public class IsAdjacentToBlock : Job.Requirement
    {
        /// <summary>
        /// Check if the specified <see cref="Agent"/> is adjacent to the <see cref="Job.Toil"/>.
        /// </summary>
        protected override bool Qualify(Agent worker, out string reason) {
            if(PointF.SqrDistance((worker.X, worker.Y), (Toil.X, Toil.Y)) == 1f) {
                reason = "";
                return true;
            }
            else {
                reason = "Worker is not adjacent to the job.";
                return false;
            }
        }
    }
    public class PlaceBlock : Job.Action
    {
        /// <summary>
        /// Have the specified <see cref="Agent"/> work at placing this block.
        /// </summary>
        /// <returns>Whether or not the action was just completed.</returns>
        protected override bool Work(Agent worker) {
            Toil.Ship.PlaceBlock("wood", Toil.X, Toil.Y);
            return true;
        }
    }
}

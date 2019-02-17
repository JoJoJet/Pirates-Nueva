using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pirates_Nueva
{
    /// <summary>
    /// Something that an <see cref="Agent"/> can do in a <see cref="Ship"/>.
    /// </summary>
    public class Job
    {
        private readonly Toil[] _toils;

        public Ship Ship { get; }

        public Job(Ship ship, params Toil[] toils) {
            Ship = ship;

            foreach(IToilContract toil in toils) {
                toil.Ship = ship;
            }
            this._toils = toils;
        }

        /// <summary>
        /// Check if this <see cref="Job"/> can currently be completed by the specified <see cref="Agent"/>
        /// </summary>
        /// <param name="reason">The reason this job cannot be completed.</param>
        public bool Qualify(Agent worker, out string reason) {
            reason = "The job is empty!";                             // Set the default reason to be that the job is empty.
            for(int i = _toils.Length-1; i >= 0; i--) {               // For every toil, working backwards from the end:
                if(_toils[i].Requirement.Qualify(worker, out reason)) // If the toil's requirement is fulfilled,
                    return true;                                      //     return true;
            }
                          // If we got this far without leaving the method,
            return false; //     return false.
        }

        /// <summary>
        /// Have the specified <see cref="Agent"/> work this job.
        /// </summary>
        /// <returns>Whether or not the job is still workable.</returns>
        public bool Work(Agent worker) {
            if(_toils.Length == 0) { // If the job is empty,
                return false;        //     return false.
            }

            for(int i = _toils.Length-1; i >= 0; i--) {    // For every toil, working backwards from the end:
                var t = _toils[i];                         //
                if(t.Requirement.Qualify(worker, out _)) { // If the toil's requirement is met:
                                                           //     Work the action.
                    if(t.Action.Work(worker))              //     If the toil was just completed,
                        return false;                      //         return false.
                    else                                   //     If the toil still has more work,
                        return true;                       //         return true.
                }
            }
                          // If we got this far without leaving the method,
            return false; //    return false.
        }

        /// <summary> Makes the Toil.Ship property only settable from within the Job class. </summary>
        private interface IToilContract
        {
            Ship Ship { set; }
        }
        /// <summary>
        /// An action paired with a requirement.
        /// </summary>
        public sealed class Toil : IToilContract
        {
            public Ship Ship { get; private set; }

            /// <summary> The X index of this <see cref="Toil"/>, local to its <see cref="Pirates_Nueva.Ship"/>. </summary>
            public int X { get; }
            /// <summary> The Y index of this <see cref="Toil"/>, local to its <see cref="Pirates_Nueva.Ship"/>. </summary>
            public int Y { get; }

            public Requirement Requirement { get; }
            public Action Action { get; }
            
            Ship IToilContract.Ship { set => Ship = value; }

            public Toil(int x, int y, Requirement req, Action action) {
                X = x;
                Y = y;

                (req as IToilSegmentContract).Toil = this;    // Set the requirement's reference to its Toil.
                (action as IToilSegmentContract).Toil = this; // Set the action's reference to its Toil.

                Requirement = req;
                Action = action;
            }
        }
        /// <summary> Makes the Toil property of a toil segment only settable from with this class. </summary>
        private interface IToilSegmentContract
        {
            Toil Toil { set; }
        }
        /// <summary> Base class for a <see cref="Requirement"/> or <see cref="Action"/>. </summary>
        public abstract class ToilSegment : IToilSegmentContract
        {
            /// <summary>
            /// The <see cref="Job.Toil"/> that contains this <see cref="Requirement"/> or <see cref="Action"/>.
            /// </summary>
            protected Toil Toil { get; private set; }
            Toil IToilSegmentContract.Toil { set => Toil = value; }

            internal ToilSegment() {  } // Ensures that this class can only be derived from within this assembly.
        }
        /// <summary>
        /// Something that must be fulfilled before a <see cref="Toil"/> can be completed.
        /// </summary>
        public abstract class Requirement : ToilSegment
        {
            /// <summary> Check if this <see cref="Requirement"/> has been fulfilled. </summary>
            /// <param name="reason">The reason that this <see cref="Requirement"/> is not fulfilled.</param>
            public abstract bool Qualify(Agent worker, out string reason);
        }
        /// <summary>
        /// What a <see cref="Toil"/> will do after its <see cref="Requirement"/> is fulfilled.
        /// </summary>
        public abstract class Action : ToilSegment
        {
            /// <summary> Have the specified <see cref="Agent"/> work at completing this <see cref="Action"/>. </summary>
            /// <returns>Whether or not the action was just completed.</returns>
            public abstract bool Work(Agent worker);
        }
    }
}

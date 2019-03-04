using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pirates_Nueva.Ocean
{
    /// <summary>
    /// Something that an <see cref="Agent"/> can do.
    /// </summary>
    public class Job<TC, TSpot> : IDrawable
        where TC    : class, IAgentContainer<TC, TSpot>, Path.IGraph<TSpot>
        where TSpot : class, IAgentSpot<TSpot>         , Path.INode<TSpot>
    {
        private readonly Toil[] _toils;

        public TC Container { get; }
        public Agent<TC, TSpot> Worker { get; set; }

        public int X { get; }
        public int Y { get; }

        public Job(TC container, int x, int y, params Toil[] toils) {
            Container = container;
            X = x;
            Y = y;

            foreach(IToilContract toil in toils) {
                toil.Job = this;
            }
            this._toils = toils;
        }

        /// <summary>
        /// Check if this <see cref="Job"/> can currently be completed by the specified <see cref="Agent"/>
        /// </summary>
        /// <param name="reason">The reason this job cannot be completed.</param>
        public bool Qualify(Agent<TC, TSpot> worker, out string reason) {
            reason = "The job is empty!";                             // Set the default reason to be that the job is empty.
            for(int i = _toils.Length-1; i >= 0; i--) {               // For every toil, working backwards from the end:
                var req = _toils[i].Requirement as IReqContract;      //
                if(req.Qualify(worker, out reason))                   // If the toil's requirement is fulfilled,
                    return true;                                      //     return true;
            }
                          // If we got this far without leaving the method,
            return false; //     return false.
        }

        /// <summary>
        /// Have the specified <see cref="Agent"/> work this job.
        /// </summary>
        /// <returns>Whether or not the job was just completed.</returns>
        public bool Work(Agent<TC, TSpot> worker, Time delta) {
            if(_toils.Length == 0) { // If the job is empty,
                return false;        //     return false.
            }

            for(int i = _toils.Length-1; i >= 0; i--) {                  // For every toil, working backwards from the end:
                var t = _toils[i];                                       //
                var req = t.Requirement as IReqContract;                 //
                if(req.Qualify(worker, out _)) {                         // If the toil's requirement is met:
                    var act = t.Action as IActionContract;               //     Work the action.
                    if(act.Work(worker, delta) && i == _toils.Length-1)  //     If the last toil was just completed,
                        return true;                                     //         return true.
                    else                                                 //     If the toil still has more work,
                        return false;                                    //         return false.
                }
            }
                          // If we got this far without leaving the method,
            return false; //    return false.
        }

        #region IDrawable Implementation
        void IDrawable.Draw(Master master) {
            for(int i = _toils.Length-1; i >= 0; i--) {                             // For each toil, working backwards from the end:
                var act = _toils[i].Action as IToilSegmentContract;                 //
                var req = _toils[i].Requirement as IToilSegmentContract;            //
                                                                                    //
                act.Draw(master, Worker);                                           // Draw its action,
                req.Draw(master, Worker);                                           // then draw its requirement on top.
                                                                                    //
                if(Worker != null && (req as IReqContract).Qualify(Worker, out _))  // If this job's worker qualifies for this toil,
                    break;                                                          //     break from this loop.
            }
        }
        #endregion

        /// <summary> Makes the Toil.Ship property only settable from within the Job class. </summary>
        private interface IToilContract
        {
            Job<TC, TSpot> Job { set; }
        }
        /// <summary>
        /// An action paired with a requirement.
        /// </summary>
        public sealed class Toil : IToilContract
        {
            private PointI? nullableIndex;

            public TC Container => Job.Container;

            /// <summary>
            /// The X and Y indices of this <see cref="Toil"/>, local to its <see cref="Ocean.Ship"/>.
            /// </summary>
            public PointI Index => this.nullableIndex ?? (Job.X, Job.Y); // If this toil has no position, default to the position of its job.

            /// <summary> The X index of this <see cref="Toil"/>, local to its <see cref="Ocean.Ship"/>. </summary>
            public int X => Index.X;
            /// <summary> The Y index of this <see cref="Toil"/>, local to its <see cref="Ocean.Ship"/>. </summary>
            public int Y => Index.Y;

            public Requirement Requirement { get; }
            public Action Action { get; }
            
            private Job<TC, TSpot> Job { get; set; }
            Job<TC, TSpot> IToilContract.Job { set => Job = value; }

            /// <summary>
            /// Create a <see cref="Toil"/>, adopting the index of its parent <see cref="Ocean.Job"/>.
            /// </summary>
            public Toil(Requirement req, Action action) {
                (req as IToilSegmentContract).Toil = this;    // Set the requirement's reference to its Toil.
                (action as IToilSegmentContract).Toil = this; // Set the action's reference to its Toil.

                Requirement = req;
                Action = action;
            }
            /// <summary>
            /// Create a <see cref="Toil"/> at the specified indices in the <see cref="Ocean.Ship"/>.
            /// </summary>
            public Toil(int x, int y, Requirement req, Action action) : this(req, action) {
                nullableIndex = (x, y);
            }
        }
        /// <summary> Makes the Toil property of a toil segment only settable from with this class. </summary>
        private interface IToilSegmentContract
        {
            Toil Toil { set; }

            void Draw(Master master, Agent<TC, TSpot> worker);
        }
        /// <summary> Base class for a <see cref="Requirement"/> or <see cref="Action"/>. </summary>
        public abstract class ToilSegment : IToilSegmentContract
        {
            /// <summary>
            /// The <see cref="Job.Toil"/> that contains this <see cref="Requirement"/> or <see cref="Action"/>.
            /// </summary>
            protected Toil Toil { get; private set; }
            Toil IToilSegmentContract.Toil { set => Toil = value; }

            /// <summary> The <see cref="Ocean.Ship"/> that contains this <see cref="Job.Toil"/>. </summary>
            protected TC Container => Toil.Container;

            internal ToilSegment() {  } // Ensures that this class can only be derived from within this assembly.

            void IToilSegmentContract.Draw(Master master, Agent<TC, TSpot> worker) => Draw(master, worker);
            /// <summary> Draw this <see cref="Requirement"/> or <see cref="Action"/> to the screen. </summary>
            protected virtual void Draw(Master master, Agent<TC, TSpot> worker) {  }
        }
        
        private interface IReqContract // Restricts access of some members to this class.
        {
            bool Qualify(Agent<TC, TSpot> worker, out string reason);
        }
        /// <summary>
        /// Something that must be fulfilled before a <see cref="Toil"/> can be completed.
        /// </summary>
        public abstract class Requirement : ToilSegment, IReqContract
        {
            bool IReqContract.Qualify(Agent<TC, TSpot> worker, out string reason) => Qualify(worker, out reason);
            /// <summary> Check if this <see cref="Requirement"/> has been fulfilled. </summary>
            /// <param name="reason">The reason that this <see cref="Requirement"/> is not fulfilled.</param>
            protected abstract bool Qualify(Agent<TC, TSpot> worker, out string reason);
        }
        
        private interface IActionContract // Restricts the access of some members to this class.
        {
            bool Work(Agent<TC, TSpot> worker, Time delta);
        }
        /// <summary>
        /// What a <see cref="Toil"/> will do after its <see cref="Requirement"/> is fulfilled.
        /// </summary>
        public abstract class Action : ToilSegment, IActionContract
        {
            bool IActionContract.Work(Agent<TC, TSpot> worker, Time delta) => Work(worker, delta);
            /// <summary> Have the specified <see cref="Agent"/> work at completing this <see cref="Action"/>. </summary>
            /// <returns>Whether or not the action was just completed.</returns>
            protected abstract bool Work(Agent<TC, TSpot> worker, Time delta);
        }
    }
}

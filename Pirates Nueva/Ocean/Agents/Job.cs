using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pirates_Nueva.Ocean.Agents
{
    /// <summary>
    /// Something that an <see cref="Agent{TC, TSpot}"/> can do.
    /// </summary>
    /// <typeparam name="TC">The type of Container that this Job exists in.</typeparam>
    /// <typeparam name="TSpot">The type of Spot that an Agent can rest on.</typeparam>
    public class Job<TC, TSpot> : IDrawable
        where TC    : class, IAgentContainer<TC, TSpot>
        where TSpot : class, IAgentSpot<TC, TSpot>
    {
        private readonly Toil[] _toils;

        public TC Container { get; }
        public Agent<TC, TSpot> Worker { get; set; }

        public bool IsCancelled { get; private set; }

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
        /// Cancels this job.
        /// </summary>
        public void Cancel() => IsCancelled = true;

        /// <summary>
        /// Check if this <see cref="Job"/> can currently be completed by the specified <see cref="Agent"/>
        /// </summary>
        /// <param name="reason">The reason this job cannot be completed.</param>
        public bool Qualify(Agent<TC, TSpot> worker, out string reason) {
            reason = "The job is empty!";                             // Set the default reason to be that the job is empty.
            for(int i = _toils.Length-1; i >= 0; i--) {               // For every toil, working backwards from the end:
                //
                // If all of the toil's requirements are fulfilled,
                // return true.
                var reqs = _toils[i].Requirements;
                var all = true;
                foreach(IReqContract r in reqs) {
                    if(!r.Qualify(worker, out reason)) {
                        all = false;
                        break;
                    }
                }
                if(all)
                    return true;
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

            for(int i = _toils.Length-1; i >= 0; i--) {                   // For every toil, working backwards from the end:
                var t = _toils[i];                                        //
                var reqs = t.Requirements as IReadOnlyList<IReqContract>; //                  
                if(reqs.All(r => r.Qualify(worker, out _))) {             // If the toil's requirements are met:
                    var act = t.Action as IActionContract;                //     Work the action.
                    if(act.Work(worker, delta) && i == _toils.Length-1)   //     If the last toil was just completed,
                        return true;                                      //         return true.
                    else                                                  //     If the toil still has more work,
                        return false;                                     //         return false.
                }
            }
                          // If we got this far without leaving the method,
            return false; //    return false.
        }

        #region IDrawable Implementation
        void IDrawable.Draw(Master master) {
            for(int i = _toils.Length-1; i >= 0; i--) {             // For each toil, working backwards from the end:
                var act = _toils[i].Action as IToilSegmentContract; //
                var reqs = _toils[i].Requirements;                  //
                act.Draw(master, Worker);                           // Draw its action,
                foreach(IToilSegmentContract req in reqs)           // then draw its requirements on top.
                    req.Draw(master, Worker);
                //
                // If this job's worker qualifies for this toil,
                // break from this loop.
                if(Worker != null && reqs.All((IReqContract r) => r.Qualify(Worker, out _))) 
                    break;
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

            public IReadOnlyList<Requirement> Requirements { get; }
            public Action Action { get; }
            
            private Job<TC, TSpot> Job { get; set; }
            Job<TC, TSpot> IToilContract.Job { set => Job = value; }

            /// <summary>
            /// Create a <see cref="Toil"/>, adopting the index of its parent <see cref="Ocean.Job"/>.
            /// </summary>
            public Toil(Requirement req, Action action) : this(new[] { req }, action) {  }
            public Toil(Requirement[] reqs, Action action) {
                foreach(var req in reqs)
                    (req as IToilSegmentContract).Toil = this;
                (action as IToilSegmentContract).Toil = this;

                Requirements = reqs;
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

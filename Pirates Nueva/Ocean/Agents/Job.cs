using System;
using System.Collections.Generic;
using static Pirates_Nueva.NullableUtil;

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
        //
        // The top item in the stack of toils.
        private readonly Toil top;

        public TC Container { get; }
        public Agent<TC, TSpot>? Worker { get; private set; }

        public bool IsCancelled { get; private set; }

        public int X { get; }
        public int Y { get; }

        public Job(TC container, int x, int y, Toil task) {
            Container = container;
            X = x;   Y = y;

            (task as IToil).Job = this;
            this.top = task;
        }

        /// <summary>
        /// Cancels this job.
        /// </summary>
        public void Cancel() => IsCancelled = true;

        /// <summary>
        /// Assigns the specified Agent to this Job.
        /// </summary>
        public void Assign(Agent<TC, TSpot> worker) {
            if(Worker != null) {
                throw new InvalidOperationException("This Job already has a Worker!");
            }
            Worker = worker;
        }
        /// <summary>
        /// Unassigns the specified Agent from this Job.
        /// </summary>
        public void Quit(Agent<TC, TSpot> worker) {
            if(Worker is null) {
                throw new InvalidOperationException("This job has no worker!");
            }
            if(Worker != worker) {
                throw new InvalidOperationException("Not the correct Agent!");
            }
            Worker = null;
            (this.top as IToil).OnQuit(worker);
        }

        /// <summary>
        /// Check if this <see cref="Job"/> can currently be completed by the specified <see cref="Agent"/>
        /// </summary>
        /// <param name="reason">The reason this job cannot be completed.</param>
        public bool Qualify(Agent<TC, TSpot> worker, out string reason)
            => (this.top as IToil).Qualify(worker, out reason);

        /// <summary>
        /// Have the specified <see cref="Agent"/> work this job.
        /// </summary>
        /// <returns>Whether or not the job was just completed.</returns>
        public bool Work(Agent<TC, TSpot> worker, Time delta)
            => (this.top as IToil).Work(worker, delta);

        #region IDrawable Implementation
        void IDrawable.Draw(Master master)
            => (top as IToil).Draw(master);
        #endregion

        /// <summary> Makes the Toil.Ship property only settable from within the Job class. </summary>
        private interface IToil
        {
            Job<TC, TSpot> Job { set; }

            bool Qualify(Agent<TC, TSpot> worker, out string reason);
            bool Work(Agent<TC, TSpot> worker, Time delta);
            void OnQuit(Agent<TC, TSpot> worker);

            void Draw(Master master);
        }
        /// <summary>
        /// An action paired with a requirement.
        /// </summary>
        public sealed class Toil : IToil
        {
            private Job<TC, TSpot>? _job;

            #region Properties
            private PointI? nullableIndex;

            public TC Container => Job.Container;

            /// <summary>
            /// The X and Y indices of this <see cref="Toil"/>, local to its <see cref="Ocean.Ship"/>.
            /// </summary>
            public PointI Index => this.nullableIndex ?? (Job.X, Job.Y); // If this toil has no position, default to the position of its job.

            /// <summary> The X index of this <see cref="Toil"/>, local to its <see cref="Ocean.Ship"/>. </summary>
            public int X => this.nullableIndex?.X ?? Job.X;
            /// <summary> The Y index of this <see cref="Toil"/>, local to its <see cref="Ocean.Ship"/>. </summary>
            public int Y => this.nullableIndex?.Y ?? Job.Y;

            public Action Action { get; }
            public IReadOnlyList<Requirement> Requirements { get; }

            private Job<TC, TSpot> Job => this._job ?? ThrowNotInitialized<Job<TC, TSpot>>(nameof(Toil));
            Job<TC, TSpot> IToil.Job {
                set {
                    //
                    // Set this job and each child toil's reference to the parent Job.
                    this._job = value;
                    foreach(var req in Requirements) {
                        if(req.Executor is IToil exec)
                            exec.Job = value;
                    }
                }
            }
            #endregion

            /// <summary>
            /// Create a <see cref="Toil"/>, adopting the index of its parent <see cref="Ocean.Job"/>.
            /// </summary>
            public Toil(Action action, params Requirement[] reqs) {
                //
                // Give the Action and Requirements
                // references to this Toil
                foreach(var req in reqs)
                    (req as ISegment).Toil = this;
                (action as ISegment).Toil = this;
                //
                // Set the fields of this object.
                Action = action;
                Requirements = reqs;
            }
            /// <summary>
            /// Create a <see cref="Toil"/> at the specified indices in the <see cref="Ocean.Ship"/>.
            /// </summary>
            public Toil(int x, int y, Action action, params Requirement[] reqs) : this(action, reqs) {
                nullableIndex = (x, y);
            }

            bool IToil.Qualify(Agent<TC, TSpot> worker, out string reason) {
                foreach(var req in Requirements) {
                    //
                    // If any of the requirements do NOT qualify.
                    var ireq = req as IReq;
                    if(!ireq.Qualify(worker, out reason)) {
                        //
                        // Check if its executor qualifies.
                        // If there's no executor, return false.
                        // If it doesn't qualify, return false.
                        // If it DOES qualify, continue.
                        var exec = req.Executor as IToil;
                        if(!exec?.Qualify(worker, out reason) ?? true)
                            return false;
                    }
                }
                //
                // If we got this far, that means
                // that either all requirement qualified,
                // OR it means that there are no requirements.
                // Either way, return true.
                reason = "";
                return true;
            }
            bool IToil.Work(Agent<TC, TSpot> worker, Time delta) {
                foreach(var req in Requirements) {
                    //
                    // If any of the requirements do NOT qualify.
                    var ireq = req as IReq;
                    if(!ireq.Qualify(worker, out _)) {
                        //
                        // Work on its executor.
                        // Always return false here, because we should
                        // only return true if the top-level Toil is completed.
                        var exec = req.Executor as IToil;
                        (req.Executor as IToil)?.Work(worker, delta);
                        return false;
                    }
                }
                //
                // If we got this far, that means that all
                // requirements qualified (or that there are no requirements).
                // Work on the action.
                return (Action as IAction).Work(worker, delta);
            }
            void IToil.OnQuit(Agent<TC, TSpot> worker) {
                foreach(var req in Requirements) {
                    if(req.Executor is IToil exec)
                        exec.OnQuit(worker);
                }
                (Action as IAction).OnQuit(worker);
            }

            void IToil.Draw(Master master) {
                var worker = Job.Worker;
                //
                // Draw each action.
                (Action as ISegment).Draw(master, worker);
                foreach(var req in Requirements) {
                    //
                    // Draw each requirement.
                    (req as ISegment).Draw(master, worker);
                    //
                    // If the requirement is NOT fulfilled,
                    // draw the requirement's executor toil.
                    if(req.Executor is IToil exec) {
                        if(worker == null || !(req as IReq).Qualify(worker, out _))
                            exec.Draw(master);
                    }
                }
            }
        }
        /// <summary> Makes the Toil property of a toil segment only settable from with this class. </summary>
        private interface ISegment
        {
            Toil Toil { set; }

            void Draw(Master master, Agent<TC, TSpot>? worker);
        }
        /// <summary> Base class for a <see cref="Requirement"/> or <see cref="Action"/>. </summary>
        public abstract class ToilSegment : ISegment
        {
            private Toil? toil;

            /// <summary>
            /// The <see cref="Job.Toil"/> that contains this <see cref="Requirement"/> or <see cref="Action"/>.
            /// </summary>
            protected Toil Toil => this.toil ?? ThrowNotInitialized<Toil>(nameof(ToilSegment));
            Toil ISegment.Toil { set => this.toil = value; }

            /// <summary> The object that contains this <see cref="Requirement"/> or <see cref="Action"/>. </summary>
            protected TC Container => Toil.Container;

            internal ToilSegment() {  } // Ensures that this class can only be derived from within this assembly.

            void ISegment.Draw(Master master, Agent<TC, TSpot>? worker) => Draw(master, worker);
            /// <summary> Draws this <see cref="Requirement"/> or <see cref="Action"/> to the screen. </summary>
            protected virtual void Draw(Master master, Agent<TC, TSpot>? worker) {  }
        }
        
        private interface IReq // Restricts access of some members to this class.
        {
            bool Qualify(Agent<TC, TSpot> worker, out string reason);
        }
        /// <summary>
        /// Something that must be fulfilled before a <see cref="Toil"/> can be completed.
        /// </summary>
        public abstract class Requirement : ToilSegment, IReq
        {
            /// <summary>
            /// A <see cref="Toil"/> that fulfills this requirement when it is completed.
            /// </summary>
            public Toil? Executor { get; }

            /// <param name="executor">
            /// A Toil that, when completed, should fulfill the current Requirement.
            /// </param>
            protected Requirement(Toil? executor = null) => Executor = executor;

            bool IReq.Qualify(Agent<TC, TSpot> worker, out string reason) => Qualify(worker, out reason);
            /// <summary> Check if this <see cref="Requirement"/> has been fulfilled. </summary>
            /// <param name="reason">The reason that this <see cref="Requirement"/> is not fulfilled.</param>
            protected abstract bool Qualify(Agent<TC, TSpot> worker, out string reason);
        }
        
        private interface IAction // Restricts the access of some members to this class.
        {
            bool Work(Agent<TC, TSpot> worker, Time delta);
            void OnQuit(Agent<TC, TSpot> worker);
        }
        /// <summary>
        /// What a <see cref="Toil"/> will do after its <see cref="Requirement"/> is fulfilled.
        /// </summary>
        public abstract class Action : ToilSegment, IAction
        {
            bool IAction.Work(Agent<TC, TSpot> worker, Time delta) => Work(worker, delta);
            /// <summary> Have the specified <see cref="Agent"/> work at completing this <see cref="Action"/>. </summary>
            /// <returns>Whether or not the action was just completed.</returns>
            protected abstract bool Work(Agent<TC, TSpot> worker, Time delta);

            void IAction.OnQuit(Agent<TC, TSpot> worker) => OnQuit(worker);
            /// <summary> Optional action to perform when a worker quits the job. </summary>
            protected virtual void OnQuit(Agent<TC, TSpot> worker) {  }
        }
    }
}

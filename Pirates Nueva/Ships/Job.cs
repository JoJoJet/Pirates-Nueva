﻿using System;
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
        public Toil[] Toils => this._toils.ToArray();

        public Job(Ship ship, params Toil[] toils) {
            Ship = ship;

            foreach(IToilContract toil in toils) {
                toil.Ship = ship;
            }
            this._toils = toils;
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
            public abstract bool Qualify(Agent worker);
        }
        /// <summary>
        /// What a <see cref="Toil"/> will do after its <see cref="Requirement"/> is fulfilled.
        /// </summary>
        public abstract class Action : ToilSegment
        {
            /// <summary> Whether or not this action has been completed. </summary>
            public abstract bool IsCompleted { get; }

            /// <summary> Have the specified <see cref="Agent"/> work at completing this <see cref="Action"/>. </summary>
            public abstract bool Work(Agent worker);
        }
    }
}

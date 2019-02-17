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

        private interface IToilContract
        {
            Ship Ship { get; set; }
        }
        /// <summary>
        /// An action paired with a requirement.
        /// </summary>
        public class Toil : IToilContract
        {
            public Requirement Requirement { get; }
            public Action Action { get; }
            
            Ship IToilContract.Ship { get; set; }

            public Toil(Requirement req, Action action) {
                (req as IToilSegmentContract).Toil = this;    // Set the requirement's reference to its Toil.
                (action as IToilSegmentContract).Toil = this; // Set the action's reference to its Toil.

                Requirement = req;
                Action = action;
            }
        }
        private interface IToilSegmentContract
        {
            Toil Toil { set; }
        }
        /// <summary> Base class for a <see cref="Requirement"/> or <see cref="Action"/>. </summary>
        public abstract class ToilSegment : IToilSegmentContract
        {
            protected Toil Toil { get; private set; }
            Toil IToilSegmentContract.Toil { set => Toil = value; }

            internal ToilSegment() {  } // Ensures that this class can only be derived from within this assembly.
        }
        /// <summary>
        /// Something that must be fulfilled before a <see cref="Toil"/> can be completed.
        /// </summary>
        public abstract class Requirement : ToilSegment
        {
            public abstract bool Qualify(Agent worker);
        }
        /// <summary>
        /// What a <see cref="Toil"/> will do after its <see cref="Requirement"/> is fulfilled.
        /// </summary>
        public abstract class Action : ToilSegment
        {
            public abstract bool IsCompleted { get; }
        }
    }
}
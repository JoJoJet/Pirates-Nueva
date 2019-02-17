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
        public Toil[] Toils => this._toils.ToArray();

        public Job(Ship ship, params Toil[] toils) {
            Ship = ship;
            this._toils = toils;
        }

        /// <summary>
        /// An action paired with a requirement.
        /// </summary>
        public class Toil
        {
            public Requirement Requirement { get; }
            public Action Action { get; }
            
            public Toil(Requirement req, Action action) {
                Requirement = req;
                Action = action;
            }
        }
        /// <summary>
        /// Something that must be fulfilled before a <see cref="Toil"/> can be completed.
        /// </summary>
        public abstract class Requirement
        {
            public abstract bool Qualify(Agent worker);
        }
        /// <summary>
        /// What a <see cref="Toil"/> will do after its <see cref="Requirement"/> is fulfilled.
        /// </summary>
        public abstract class Action
        {
            public abstract bool IsCompleted { get; }
        }
    }
}

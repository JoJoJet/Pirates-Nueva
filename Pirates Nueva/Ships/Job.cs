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
        }
    }
}

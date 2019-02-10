using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pirates_Nueva
{
    /// <summary>
    /// Controls the user interface for <see cref="Pirates_Nueva"/>.
    /// </summary>
    public class GUI : IUpdatable, IDrawable
    {
        public Master Master { get; }

        internal GUI(Master master) {
            Master = master;
        }

        void IUpdatable.Update(Master master) {

        }

        void IDrawable.Draw(Master master) {

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pirates_Nueva
{
    public class PlayerController : IUpdatable
    {
        public Master Master { get; }

        private Sea Sea { get; }

        internal PlayerController(Master master, Sea sea) {
            Master = master;
            Sea = sea;
        }

        void IUpdatable.Update(Master master) {

        }
    }
}

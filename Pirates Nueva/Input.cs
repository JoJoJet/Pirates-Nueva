using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Pirates_Nueva
{
    public class Input : IUpdatable
    {
        public Master Master { get; }

        public Point MousePosition { get; private set; }

        internal Input(Master master) {
            Master = master;
        }

        void IUpdatable.Update(Master master) {
            var mouse = Mouse.GetState();

            MousePosition = mouse.Position;
        }
    }
}

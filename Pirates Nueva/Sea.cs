using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pirates_Nueva
{
    public class Sea : IUpdatable, IDrawable
    {
        private readonly List<Ship> ships = new List<Ship>();

        public Sea() {
            this.ships.Add(new Ship(10, 5));
        }

        public void Update(Master master) {
            foreach(Ship ship in this.ships) {
                ship.Update(master);
            }
        }

        public void Draw(Master master) {
            foreach(Ship ship in this.ships) {
                ship.Draw(master);
            }
        }
    }
}

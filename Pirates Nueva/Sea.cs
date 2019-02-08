using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Point = Microsoft.Xna.Framework.Point;

namespace Pirates_Nueva
{
    public sealed class Sea : IUpdatable, IDrawable
    {
        private readonly List<Ship> ships = new List<Ship>();

        private Master Master { get; }

        public Sea(Master master) {
            Master = master;

            this.ships.Add(new Ship(this, 10, 5));
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

        /// <summary>
        /// Transform the input <see cref="Point"/> from screen space to a pair of coordinates within this <see cref="Sea"/>.
        /// </summary>
        /// <param name="screenPoint">A <see cref="Point"/> in screen space.</param>
        public Vector2 ScreenPointToSea(Point screenPoint) {
            var (x, y) = ScreenPointToSea((screenPoint.X, screenPoint.Y));
            return new Vector2(x, y);
        }

        /// <summary>
        /// Transform the input coordinates from screen space to a pair of coordinates within this <see cref="Sea"/>.
        /// </summary>
        /// <param name="screenPoint">Coordinates local to the screen.</param>
        internal (float x, float y) ScreenPointToSea((int x, int y) screenPoint) => ScreenPointToSea(screenPoint.x, screenPoint.y);
        /// <summary>
        /// Transform the input coordinates from screen space to a pair of coordinates within this <see cref="Sea"/>.
        /// </summary>
        /// <param name="x">The x coordinate local to the screen.</param>
        /// <param name="y">The y coordinate local to the screen.</param>
        internal(float x, float y) ScreenPointToSea(int x, int y) {
            int height = Master.GraphicsDevice.Viewport.Height;
            return ((float)x / Block.Pixels, (float)(height - y) / Block.Pixels);
        }
    }
}

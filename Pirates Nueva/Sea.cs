using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Color = Microsoft.Xna.Framework.Color;

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
            
            var (mouseX, mouseY) = ScreenPointToSea(master.Input.MousePosition);
            master.SpriteBatch.DrawString(master.Font, $"Mouse: {mouseX:.00}, {mouseY:.00}", PointF.Zero, Color.Black);
        }

        #region Space Transformation
        /// <summary>
        /// Transform the input <see cref="PointI"/> from screen space to a <see cref="PointF"/> within this <see cref="Sea"/>.
        /// </summary>
        /// <param name="screenPoint">A pair of coordinates in screen space.</param>
        public PointF ScreenPointToSea(PointI screenPoint) => ScreenPointToSea(screenPoint.X, screenPoint.Y);
        /// <summary>
        /// Transform the input coordinates from screen space to a pair of coordinates within this <see cref="Sea"/>.
        /// </summary>
        /// <param name="x">The x coordinate local to the screen.</param>
        /// <param name="y">The y coordinate local to the screen.</param>
        internal (float x, float y) ScreenPointToSea(int x, int y) {
            int height = Master.GraphicsDevice.Viewport.Height;
            return ((float)x / Block.Pixels, (float)(height - y) / Block.Pixels);
        }

        /// <summary>
        /// Transform the input <see cref="PointF"/> from this <see cref="Sea"/> to <see cref="PointI"/> local to the screen.
        /// </summary>
        /// <param name="seaPoint">A pair of coordinates within this <see cref="Sea"/>.</param>
        public PointI SeaPointToScreen(PointF seaPoint) => SeaPointToScreen(seaPoint.X, seaPoint.Y);
        /// <summary>
        /// Transform the input coordinates from this <see cref="Sea"/> to the screen.
        /// </summary>
        /// <param name="x">The x coordinate local to this <see cref="Sea"/>.</param>
        /// <param name="y">The y coordinate local to this <see cref="Sea"/>.</param>
        internal (int x, int y) SeaPointToScreen(float x, float y) {
            int height = Master.GraphicsDevice.Viewport.Height;
            return ((int)Math.Round(x *  Block.Pixels), (int)Math.Round(height - y * Block.Pixels));
        }
        #endregion
    }
}

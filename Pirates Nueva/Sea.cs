using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Color = Microsoft.Xna.Framework.Color;

namespace Pirates_Nueva
{
    public sealed class Sea : IUpdatable, IDrawable, IFocusableParent
    {
        private readonly List<Entity> entities = new List<Entity>();

        private Master Master { get; }

        public Sea(Master master) {
            Master = master;

            this.entities.Add(new PlayerShip(this, 10, 5));

            master.GUI.AddEdge("debug_mouse", new UI.EdgeText("mouse position", master.Font, GUI.Edge.Top, GUI.Direction.Right));
        }

        void IUpdatable.Update(Master master) {
            foreach(Ship ship in this.entities) {
                ship.Update(master);
            }

            if(master.GUI.TryGetEdge<UI.EdgeText>("debug_mouse", out var tex)) {
                tex.Text = $"Mouse: {ScreenPointToSea(master.Input.MousePosition)}";
            }
        }

        void IDrawable.Draw(Master master) {
            foreach(Ship ship in this.entities) {
                ship.Draw(master);
            }
        }

        #region IFocusableParent Implementation
        /// <summary>
        /// Get any <see cref="IFocusable"/> objects located at /seaPoint/, in sea-space.
        /// </summary>
        List<IFocusable> IFocusableParent.GetFocusable(PointF seaPoint) {
            var focusable = new List<IFocusable>();

            foreach(var ent in entities) {          // For every ship:
                if(ent.IsColliding(seaPoint)) {     // If it is colliding with /seaPoint/,
                    if(ent is IFocusable f) {       // and it implements IFocusable,
                        focusable.Add(f);           // add it to the list of focusable objects.
                    }                                                  // Otherwise:
                    if(ent is IFocusableParent fp)                    // If it implement IFocusableParent,
                        focusable.AddRange(fp.GetFocusable(seaPoint)); // add any of its focusable children to the list.
                }
            }
            return focusable;
        }
        #endregion

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
            int height = Master.GUI.ScreenHeight;
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
            int height = Master.GUI.ScreenHeight;
            return ((int)Math.Round(x *  Block.Pixels), (int)Math.Round(height - y * Block.Pixels));
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Pirates_Nueva
{
    public class Furniture : IDrawable
    {
        public Ship Ship => Floor.Ship;

        public FurnitureDef Def { get; private set; }
        public string ID => Def.ID;

        /// <summary>
        /// The <see cref="Block"/> that this <see cref="Furniture"/> is resting upon.
        /// </summary>
        public Block Floor { get; private set; }

        /// <summary> The X index of this <see cref="Furniture"/>, local to its <see cref="Pirates_Nueva.Ship"/>. </summary>
        public int X => Floor.X;
        /// <summary> The Y index of this <see cref="Furniture"/>, local to its <see cref="Pirates_Nueva.Ship"/>. </summary>
        public int Y => Floor.Y;

        /// <summary> The X and Y indices of of this <see cref="Furniture"/> within its <see cref="Ship"/>. </summary>
        public PointI Index => (X, Y);

        /// <summary>
        /// Create a <see cref="Furniture"/>, defined by the <see cref="FurnitureDef"/> /def/, and placed on the <see cref="Block"/> /block/.
        /// </summary>
        public Furniture(FurnitureDef def, Block floor) {
            Def = def;
            Floor = floor;
        }

        public void Draw(Master master) {
            var tex = master.Resources.LoadTexture(Def.ID);

            (float seaX, float seaY) = Ship.ShipPointToSea(X, Y+1);
            (int screenX, int screenY) = Ship.Sea.SeaPointToScreen(seaX, seaY);
            master.SpriteBatch.DrawRotated(tex, new Rectangle(screenX, screenY, Block.Pixels, Block.Pixels), -Ship.Angle, new PointF(0));
        }
    }
}

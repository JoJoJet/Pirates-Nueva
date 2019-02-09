using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Pirates_Nueva
{
    public class Block : IDrawable
    {
        /// <summary>
        /// The number of pixels in a <see cref="Block"/> (square).
        /// </summary>
        internal const int Pixels = 32;

        public Ship Ship { get; }

        public BlockDef Def { get; private set; }
        public string ID => Def.ID;

        /// <summary> The X index of this <see cref="Block"/>, local to its <see cref="Pirates_Nueva.Ship"/>. </summary>
        public int X { get; private set; }
        /// <summary> The Y index of this <see cref="Block"/>, local to its <see cref="Pirates_Nueva.Ship"/>. </summary>
        public int Y { get; private set; }

        /// <summary> The x and y indices of this <see cref="Block"/> within its <see cref="Pirates_Nueva.Ship"/>. </summary>
        public PointI Index => new PointI(X, Y);

        /// <summary>
        /// Create a <see cref="Block"/> with position (/x/. /y/), defined by the <see cref="BlockDef"/> /def/.
        /// </summary>
        public Block(Ship parent, BlockDef def, int x, int y) {
            Ship = parent;
            Def = def;
            X = x;
            Y = y;
        }

        public void Draw(Master master) {
            var tex = master.Resources.LoadTexture(Def.TextureID);

            (float seaX, float seaY) = Ship.ShipPointToSea(X, Y);
            // SpriteBatch.Draw() draws the texture from the top left, while our indices are positioned on the bottom left.
            // We need to bump this position upwards by one block length.
            (int screenX, int screenY) = Ship.Sea.SeaPointToScreen(seaX, seaY+1);
            master.SpriteBatch.Draw(tex, new Rectangle(screenX, screenY, Pixels, Pixels), Color.White);
        }
    }
}

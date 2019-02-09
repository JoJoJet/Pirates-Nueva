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

        /// <summary> The X coordinate of this <see cref="Block"/>, local to its <see cref="Pirates_Nueva.Ship"/>. </summary>
        public int X { get; private set; }
        /// <summary> The Y coordinate of this <see cref="Block"/>, local to its <see cref="Pirates_Nueva.Ship"/>. </summary>
        public int Y { get; private set; }

        /// <summary> A <see cref="ValueTuple"/> containing the position of this <see cref="Block"/>. </summary>
        internal (int x, int y) Position => (X, Y);

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
            var (x, y) = Ship.Sea.SeaPointToScreen(X, Y);
            master.SpriteBatch.Draw(tex, new Rectangle(x, y-Pixels, Pixels, Pixels), Color.White);
        }
    }
}

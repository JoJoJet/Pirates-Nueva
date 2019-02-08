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
        internal const int Pixels = 32;

        public Ship Parent { get; }

        public BlockDef Def { get; private set; }

        /// <summary> The X coordinate of this <see cref="Block"/>, local to its <see cref="Ship"/>. </summary>
        public int X { get; private set; }
        /// <summary> The Y coordinate of this <see cref="Block"/>, local to its <see cref="Ship"/>. </summary>
        public int Y { get; private set; }

        /// <summary> A <see cref="ValueTuple"/> containing the position of this <see cref="Block"/>. </summary>
        internal (int x, int y) Position => (X, Y);

        /// <summary>
        /// Create a <see cref="Block"/> with position (/x/. /y/), defined by the <see cref="BlockDef"/> /def/.
        /// </summary>
        public Block(Ship parent, BlockDef def, int x, int y) {
            Parent = parent;
            Def = def;
            X = x;
            Y = y;
        }

        public void Draw(Master master) {
            var tex = master.Resources.LoadTexture(Def.TextureID);
            master.SpriteBatch.Draw(tex, new Rectangle(X * Pixels, Y * Pixels, Pixels, Pixels), Color.White);
        }
    }
}

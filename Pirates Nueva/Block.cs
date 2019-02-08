﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Pirates_Nueva
{
    public class Block : IDrawable
    {
        internal const int Size = 32;

        public BlockDef Def { get; private set; }

        /// <summary> The X coordinate of this <see cref="Block"/>, local to its <see cref="Ship"/>. </summary>
        public int X { get; private set; }
        /// <summary> The Y coordinate of this <see cref="Block"/>, local to its <see cref="Ship"/>. </summary>
        public int Y { get; private set; }

        /// <summary> A Tuple containing the position of this <see cref="Block"/>. </summary>
        internal (int x, int y) Position => (X, Y);

        /// <summary>
        /// Create a <see cref="Block"/> with position (/x/. /y/), defined by the <see cref="BlockDef"/> /def/.
        /// </summary>
        public Block(BlockDef def, int x, int y) {
            Def = def;
            X = x;
            Y = y;
        }
        /// <summary>
        /// Create a <see cref="Block"/> with position (/x/, /y/), with a <see cref="BlockDef"/> identified by /defId/.
        /// </summary>
        /// <exception cref="KeyNotFoundException">Thrown if there is no <see cref="BlockDef"/> identified by /defId/.</exception>
        /// <exception cref="InvalidCastException">Thrown if the <see cref="Def"/> identifed by /defId/ is not a <see cref="BlockDef"/>.</exception>
        public Block(string defId, int x, int y) : this(BlockDef.Get(defId), x, y) {  }

        public void Draw(Master master) {
            var tex = master.Resources.LoadTexture(Def.TextureID);
            master.SpriteBatch.Draw(tex, new Rectangle(X * Size, Y * Size, Size, Size), Color.White);
        }
    }
}

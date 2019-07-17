using System;
using System.Collections.Generic;
using System.Text;

namespace Pirates_Nueva.Ocean
{
    public enum IslandBlockShape { Solid = 0, TopRight, BottomRight, BottomLeft, TopLeft };

    public sealed class IslandBlock : IDrawable<Island>
    {
        /// <summary>
        /// The <see cref="Ocean.Island"/> that contains this <see cref="IslandBlock"/>.
        /// </summary>
        public Island Island { get; }

        public IslandBlockDef Def { get; }

        /// <summary> The X index of this <see cref="IslandBlock"/>, local to its <see cref="Ocean.Island"/>. </summary>
        public int X { get; }
        /// <summary> The Y index of this <see cref="IslandBlock"/>, local to its <see cref="Ocean.Island"/>. </summary>
        public int Y { get; }

        public IslandBlockShape Shape { get; }

        public IslandBlock(Island island, IslandBlockDef def, int x, int y, IslandBlockShape shape) {
            Island = island;
            Def = def;
            X = x;    Y = y;
            Shape = shape;
        }

        void IDrawable<Island>.Draw(ILocalDrawer<Island> drawer) {
            drawer.DrawCenter(Def.GetSprite(Shape), X, Y, 1, 1);
        }
    }
}

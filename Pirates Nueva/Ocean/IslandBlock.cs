using System;
using System.Collections.Generic;
using System.Text;

namespace Pirates_Nueva.Ocean
{
    public sealed class IslandBlock : IDrawable<Island>
    {
        /// <summary>
        /// The <see cref="Ocean.Island"/> that contains this <see cref="IslandBlock"/>.
        /// </summary>
        public Island Island { get; }

        /// <summary> The X index of this <see cref="IslandBlock"/>, local to its <see cref="Ocean.Island"/>. </summary>
        public int X { get; }
        /// <summary> The Y index of this <see cref="IslandBlock"/>, local to its <see cref="Ocean.Island"/>. </summary>
        public int Y { get; }

        public IslandBlock(Island island, int x, int y) {
            Island = island;
            X = x;    Y = y;
        }

        void IDrawable<Island>.Draw(ILocalDrawer<Island> drawer) {
            var tex = Resources.LoadTexture("woodBlock");
            drawer.DrawCenter(tex, X, Y, 1, 1);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Pirates_Nueva.Path;
using Pirates_Nueva.Ocean.Agents;

namespace Pirates_Nueva.Ocean
{
    using Stock = Stock<Island, IslandBlock>;

    public enum IslandBlockShape { Solid = 0, TopRight, BottomRight, BottomLeft, TopLeft };
    public sealed class IslandBlock : IAgentSpot<Island, IslandBlock>, IDrawable<Island>
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
        public PointI Index => (X, Y);

        public IslandBlockShape Shape { get; }

        public Stock? Stock { get; set; }

        public IslandBlock(Island island, IslandBlockDef def, int x, int y, IslandBlockShape shape) {
            Island = island;
            Def = def;
            X = x;    Y = y;
            Shape = shape;
        }

        IEnumerable<Edge<IslandBlock>> INode<IslandBlock>.Edges {
            get {
                if(Island.TryGetBlock(X-1, Y, out var block))
                    yield return new Edge<IslandBlock>(1, block);
                if(Island.TryGetBlock(X, Y+1, out block))
                    yield return new Edge<IslandBlock>(1, block);
                if(Island.TryGetBlock(X+1, Y, out block))
                    yield return new Edge<IslandBlock>(1, block);
                if(Island.TryGetBlock(X, Y-1, out block))
                    yield return new Edge<IslandBlock>(1, block);
            }
        }

        void IDrawable<Island>.Draw<TDrawer>(in TDrawer drawer) {
            drawer.DrawCenter(Def.GetSprite(Shape), X, Y, 1, 1);
        }
    }
}

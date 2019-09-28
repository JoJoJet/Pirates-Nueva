using System;
using System.Collections.Generic;
using Pirates_Nueva.Ocean.Agents;

namespace Pirates_Nueva.Ocean
{
    public class Block : Ship.Part, IAgentSpotDestroyable<Ship, Block>, Path.INode<Block>
    {
        private List<Action<Block>> onDestroyed = new List<Action<Block>>();

        /// <summary> The <see cref="Ocean.Ship"/> that contains this <see cref="Block"/>. </summary>
        public sealed override Ship Ship { get; }

        public BlockDef Def { get; }
        public string ID => Def.ID;

        /// <summary> The X index of this <see cref="Block"/>, local to its <see cref="Ocean.Ship"/>. </summary>
        public sealed override int X { get; }
        /// <summary> The Y index of this <see cref="Block"/>, local to its <see cref="Ocean.Ship"/>. </summary>
        public sealed override int Y { get; }

        /// <summary> The direction that this <see cref="Block"/> is facing. </summary>
        public override Dir Direction => Dir.Forward;

        /// <summary>
        /// The <see cref="Ocean.Furniture"/> placed on this block.
        /// </summary>
        public Furniture? Furniture { get; private set; }

        /// <summary>
        /// The stock that is resting on this <see cref="Block"/>, if it exists.
        /// </summary>
        public Stock<Ship, Block>? Stock { get; set; }

        /// <summary> Whether or not this <see cref="Block"/> has been destroyed. </summary>
        public bool IsDestroyed { get; private set; }

        /// <summary>
        /// Static constructor. Is called the first time that this class is mentioned.
        /// </summary>
        static Block() {
            /*
             * Give 'Ship' class a delegate that allows it
             * to assign the 'Furniture' property of this class.
             */
            Ship.SetBlockFurniture = (block, furn) => block.Furniture = furn;
        }

        /// <summary>
        /// Create a <see cref="Block"/> with position (/x/. /y/), defined by the <see cref="BlockDef"/> /def/.
        /// </summary>
        public Block(Ship parent, BlockDef def, int x, int y) {
            Ship = parent;
            Def = def;
            X = x;
            Y = y;
        }

        public void SubscribeOnDestroyed(Action<Block> action) => this.onDestroyed.Add(action);
        public void UnsubscribeOnDestroyed(Action<Block> action) => this.onDestroyed.Remove(action);

        /// <summary> Destroys this <see cref="Block"/>. </summary>
        public void Destroy() {
            IsDestroyed = true;
            foreach(var act in this.onDestroyed) {
                act.Invoke(this);
            }
        }

        /// <summary> Draw this <see cref="Block"/> to the screen. </summary>
        protected override void Draw(ILocalDrawer<Ship> drawer) {
            var tex = Resources.LoadSprite(Def.SpriteID);

            drawer.Draw(tex, X, Y, 1, 1, Angle, (0.5f, 0.5f));
        }

        #region IFocusable Implementation
        protected override IFocusMenuProvider GetFocusProvider(Master master)
            => new BlockFocusProvider<Block>(this, master);

        protected class BlockFocusProvider<TBlock> : FocusProvider<TBlock>
            where TBlock : Block
        {
            public BlockFocusProvider(TBlock block, Master master) : base(block)
                => master.GUI.AddMenu(
                      MenuID, new UI.FloatingMenu(
                          Part, (0f, -0.1f), UI.Corner.BottomLeft,
                          new UI.Text<UI.GUI.Menu>("ID: " + Part.ID, master.Font)
                          )
                      );
            public override void Close(Master master)
                => master.GUI.RemoveMenu(MenuID);
        }
        #endregion

        #region Path.INode Implementation
        IEnumerable<Path.Edge<Block>> Path.INode<Block>.Edges {
            get {
                if(check(X - 1, Y, out var b))               // If there's a block astern,
                    yield return new Path.Edge<Block>(1, b); //     return an edge connecting to it.
                if(check(X, Y+1, out b))                     // If there's a block aport,
                    yield return new Path.Edge<Block>(1, b); //     return an edge connecting to it.
                if(check(X+1, Y, out b))                     // If there's a block forwards,
                    yield return new Path.Edge<Block>(1, b); //     return an edge connecting to it.
                if(check(X, Y-1, out b))                     // If there's a block astarboard,
                    yield return new Path.Edge<Block>(1, b); //     return an edge connecting to it.

                bool check(int x, int y, out Block block)
                    // FIXME: When we get attributes in local functions,
                    //     add a NotNullWhen(true) attribute to /block/ parameter.
                    => Ship.TryGetBlock(x, y, out block!) && !block.IsDestroyed;
            }
        }
        #endregion
    }
}

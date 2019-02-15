using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pirates_Nueva
{
    public class Block : Ship.Part, IFocusable, UI.IScreenSpaceTarget
    {
        /// <summary> The number of pixels in a <see cref="Block"/> (square). </summary>
        internal const int Pixels = 32;

        /// <summary> The <see cref="Pirates_Nueva.Ship"/> that contains this <see cref="Block"/>. </summary>
        public override Ship Ship { get; }

        public BlockDef Def { get; private set; }
        public string ID => Def.ID;

        /// <summary> The X index of this <see cref="Block"/>, local to its <see cref="Pirates_Nueva.Ship"/>. </summary>
        public override int X { get; }
        /// <summary> The Y index of this <see cref="Block"/>, local to its <see cref="Pirates_Nueva.Ship"/>. </summary>
        public override int Y { get; }

        /// <summary> The direction that this <see cref="Block"/> is facing. </summary>
        public override Dir Direction => Dir.Right;

        /// <summary>
        /// The <see cref="Pirates_Nueva.Furniture"/> placed on this block. Might be null.
        /// </summary>
        public Furniture Furniture { get; private set; }

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

        /// <summary> Draw this <see cref="Block"/> to the screen. </summary>
        protected override void Draw(Master master) {
            var tex = master.Resources.LoadTexture(Def.TextureID);

            PointF texOffset = (0.5f, 0.5f);                  // As MonoGame draws from the top left, ofset the texture by
            texOffset += PointF.Rotate((-0.5f, 0.5f), Angle); //     a rotated constant to account for this.
            
            (float seaX, float seaY) = Ship.ShipPointToSea(Index + texOffset);  // The top left of the Block's texture in sea-space.
            (int screenX, int screenY) = Ship.Sea.SeaPointToScreen(seaX, seaY); // The top left of the Block's texture in screen-space.
            master.SpriteBatch.DrawRotated(tex, screenX, screenY, Pixels, Pixels, -Angle - Ship.Angle, (0, 0));
        }

        #region IScreenSpaceTarget Implementation
        private PointI ScreenTarget => Ship.Sea.SeaPointToScreen(Ship.ShipPointToSea(X, Y));
        int UI.IScreenSpaceTarget.X => ScreenTarget.X;
        int UI.IScreenSpaceTarget.Y => ScreenTarget.Y;
        #endregion

        #region IFocusable Implementation
        bool IFocusable.IsLocked => false;

        const string FocusMenuID = "blockfloating";
        void IFocusable.StartFocus(Master master) {
            if(master.GUI.HasMenu(FocusMenuID) == false) // If there's no GUI menu for this block,
                master.GUI.AddMenu(                      //     add one.
                    FocusMenuID,
                    new UI.FloatingMenu(this, (0f, -0.1f), UI.Corner.BottomLeft,
                    new UI.MenuText("ID: " + ID, master.Font))
                    );
        }
        void IFocusable.Focus(Master master) {

        }
        void IFocusable.Unfocus(Master master) {
            if(master.GUI.HasMenu(FocusMenuID))     // If there is a GUI menu for this block,
                master.GUI.RemoveMenu(FocusMenuID); // remove it.
        }
        #endregion
    }
}

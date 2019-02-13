﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pirates_Nueva
{
    public class Block : IDrawable, IFocusable, UI.IScreenSpaceTarget
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

        public void Draw(Master master) {
            var tex = master.Resources.LoadTexture(Def.TextureID);

            // SpriteBatch.Draw() draws the texture from the top left, while our indices are positioned on the bottom left.
            // We need to bump this position upwards (local to the ship) by one block length.
            (float seaX, float seaY) = Ship.ShipPointToSea(X, Y+1);
            (int screenX, int screenY) = Ship.Sea.SeaPointToScreen(seaX, seaY);
            master.SpriteBatch.DrawRotated(tex, screenX, screenY, Pixels, Pixels, -Ship.Angle, (0, 0));
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

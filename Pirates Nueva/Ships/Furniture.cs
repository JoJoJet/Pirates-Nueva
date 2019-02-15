using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pirates_Nueva
{
    /// <summary>
    /// A relative direction.
    /// </summary>
    public enum Dir { Up, Right, Down, Left };
    public class Furniture : Ship.Part, IFocusable, UI.IScreenSpaceTarget
    {
        /// <summary> The <see cref="Pirates_Nueva.Ship"/> that contains this <see cref="Furniture"/>. </summary>
        public Ship Ship => Floor.Ship;

        public FurnitureDef Def { get; private set; }
        public string ID => Def.ID;

        /// <summary>
        /// The <see cref="Block"/> that this <see cref="Furniture"/> is resting upon.
        /// </summary>
        public Block Floor { get; private set; }

        /// <summary> The X index of this <see cref="Furniture"/>, local to its <see cref="Pirates_Nueva.Ship"/>. </summary>
        public override int X => Floor.X;
        /// <summary> The Y index of this <see cref="Furniture"/>, local to its <see cref="Pirates_Nueva.Ship"/>. </summary>
        public override int Y => Floor.Y;

        /// <summary> The direction that this <see cref="Furniture"/> is facing. </summary>
        public Dir Direction { get; protected set; }
        
        public virtual Angle Angle => Angle.FromDegrees(Direction == Dir.Up ? 90 : (Direction == Dir.Right ? 0 : (Direction == Dir.Down ? 270 : 180)));

        /// <summary>
        /// Create a <see cref="Furniture"/>, defined by the <see cref="FurnitureDef"/> /def/, and placed on the <see cref="Block"/> /block/.
        /// </summary>
        public Furniture(FurnitureDef def, Block floor, Dir direction) {
            Def = def;
            Floor = floor;
            Direction = direction;
        }
        
        /// <summary> Draw this <see cref="Furniture"/> to the screen. </summary>
        protected override void Draw(Master master) {
            var tex = master.Resources.LoadTexture(Def.ID);
            (int sizeX, int sizeY) = Def.TextureSize * Block.Pixels;
            
            // /Def.TextureOffset/ is the coordinate, local to the texture, from which it will be drawn.
            // Subtract it from '1' to turn the origin into an offset.
            PointF texOffset = (1, 1) - Def.TextureOrigin;
            texOffset = (texOffset.X * Def.TextureSize.X, texOffset.Y * Def.TextureSize.Y); // Multiply the offset by the size
                                                                                            // of the texture in ship-space.
            texOffset += PointF.Rotate((-0.5f, 0.5f), Angle); // As MonoGame draws from the top left, offset by a rotated constant.

            (float seaX, float seaY) = Ship.ShipPointToSea(Index + texOffset);  // The top left of this Furniture in sea-space.
            (int screenX, int screenY) = Ship.Sea.SeaPointToScreen(seaX, seaY); // The top left of this Furniture in screen-space.
            master.SpriteBatch.DrawRotated(tex, screenX, screenY, sizeX, sizeY, -Angle - Ship.Angle, (0, 0));
        }

        #region IScreenSpaceTarget Implementation
        private PointI ScreenTarget => Ship.Sea.SeaPointToScreen(Ship.ShipPointToSea(X, Y));
        int UI.IScreenSpaceTarget.X => ScreenTarget.X;
        int UI.IScreenSpaceTarget.Y => ScreenTarget.Y;
        #endregion

        #region IFocusable Implementation
        bool IFocusable.IsLocked => false;

        const string FocusMenuID = "furniturefloating";
        void IFocusable.StartFocus(Master master) {
            if(master.GUI.HasMenu(FocusMenuID) == false) // If there's no GUI menu for this furniture,
                master.GUI.AddMenu(                      //     add one.
                    FocusMenuID,
                    new UI.FloatingMenu(this, (0f, -0.1f), UI.Corner.BottomLeft,
                    new UI.MenuText("ID: " + ID, master.Font))
                    );
        }
        void IFocusable.Focus(Master master) {

        }
        void IFocusable.Unfocus(Master master) {
            if(master.GUI.HasMenu(FocusMenuID))     // If there is a GUI menu for this furniture,
                master.GUI.RemoveMenu(FocusMenuID); //     remove it.
        }
        #endregion
    }
}

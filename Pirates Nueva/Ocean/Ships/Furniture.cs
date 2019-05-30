using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pirates_Nueva.Ocean
{
    /// <summary>
    /// A relative direction.
    /// </summary>
    public enum Dir { Up, Right, Down, Left };
    public class Furniture : Ship.Part, IFocusable, UI.IScreenSpaceTarget
    {
        /// <summary> The <see cref="Ocean.Ship"/> that contains this <see cref="Furniture"/>. </summary>
        public override Ship Ship => Floor.Ship;

        public FurnitureDef Def { get; private set; }
        public string ID => Def.ID;

        /// <summary>
        /// The <see cref="Block"/> that this <see cref="Furniture"/> is resting upon.
        /// </summary>
        public Block Floor { get; private set; }

        /// <summary> The X index of this <see cref="Furniture"/>, local to its <see cref="Ocean.Ship"/>. </summary>
        public override int X => Floor.X;
        /// <summary> The Y index of this <see cref="Furniture"/>, local to its <see cref="Ocean.Ship"/>. </summary>
        public override int Y => Floor.Y;

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
            var tex = master.Resources.LoadTexture(Def.TextureID);
            (int sizeX, int sizeY) = Def.TextureSize * Ship.Sea.PPU;
            
            // /Def.TextureOffset/ is the coordinate, local to the texture, from which it will be drawn.
            // Subtract it from '1' to turn the origin into an offset.
            PointF texOffset = (1, 1) - Def.TextureOrigin;
            texOffset = (texOffset.X * Def.TextureSize.X, texOffset.Y * Def.TextureSize.Y); // Multiply the offset by the size
                                                                                            // of the texture in ship-space.
            texOffset += PointF.Rotate((-0.5f, 0.5f), Angle); // As MonoGame draws from the top left, offset by a rotated constant.

            (float seaX, float seaY) = Ship.ShipPointToSea(Index + texOffset);  // The top left of this Furniture's texture in sea-space.
            (int screenX, int screenY) = Ship.Sea.SeaPointToScreen(seaX, seaY); // The top left of this Furniture's texture in screen-space.
            master.Renderer.DrawRotated(tex, screenX, screenY, sizeX, sizeY, -Angle - Ship.Angle, (0, 0));
        }

        #region IScreenSpaceTarget Implementation
        private PointI ScreenTarget => Ship.Sea.SeaPointToScreen(Ship.ShipPointToSea(X, Y));
        int UI.IScreenSpaceTarget.X => ScreenTarget.X;
        int UI.IScreenSpaceTarget.Y => ScreenTarget.Y;
        #endregion

        #region IFocusable Implementation
        IFocusMenuProvider IFocusable.GetProvider() => new FocusProvider(this);

        private sealed class FocusProvider : IFocusMenuProvider
        {
            const string MenuID = "furnitureFocusFloating";

            public bool IsLocked => false;
            private Furniture Furn { get; }

            public FocusProvider(Furniture furniture) => Furn = furniture;

            public void Start(Master master) {
                master.GUI.AddMenu(
                    MenuID, new UI.FloatingMenu(
                        Furn, (0f, -0.1f), UI.Corner.BottomLeft,
                        new UI.MenuText("ID: " + Furn.ID, master.Font)
                        )
                    );
            }
            public void Update(Master master) {  }
            public void Close(Master master)
                => master.GUI.RemoveMenu(MenuID);
        }
        #endregion
    }
}

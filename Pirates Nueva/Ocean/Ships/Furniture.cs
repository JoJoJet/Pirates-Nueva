namespace Pirates_Nueva.Ocean
{
    public class Furniture : Ship.Part
    {
        /// <summary> The <see cref="Ocean.Ship"/> that contains this <see cref="Furniture"/>. </summary>
        public override Ship Ship => Floor.Ship;

        public FurnitureDef Def { get; }
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
        protected override void Draw(ILocalDrawer<Ship> drawer) {
            var tex = Resources.LoadSprite(Def.SpriteID);

            drawer.Draw(tex, X, Y, Def.SpriteSize.X, Def.SpriteSize.Y, Angle, Def.SpriteOrigin);
        }

        #region IFocusable Implementation
        protected override IFocusMenuProvider GetFocusProvider(Master master)
            => new FurnitureFocusProvider<Furniture>(this, master);

        protected class FurnitureFocusProvider<TFurn> : FocusProvider<TFurn>
            where TFurn : Furniture
        {
            public FurnitureFocusProvider(TFurn furniture, Master master) : base(furniture)
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
    }
}

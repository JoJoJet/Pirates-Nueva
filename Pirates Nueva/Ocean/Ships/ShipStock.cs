using Pirates_Nueva.Ocean.Agents;

namespace Pirates_Nueva.Ocean
{
    /// <summary>
    /// An implementation of <see cref="Agents.Stock{TC, TSpot}"/> for a <see cref="Ship"/>.
    /// </summary>
    internal class ShipStock : Stock<Ship, Block>
    {
        /// <summary>
        /// The <see cref="Ocean.Ship"/> that contains this <see cref="ShipStock"/>.
        /// </summary>
        public Ship Ship => Container;

        protected override PointI ScreenTarget => Container.Sea.SeaPointToScreen(Container.ShipPointToSea(X + 0.5f, Y + 0.5f));

        public ShipStock(ItemDef def, Ship ship, Block floor) : base(def, ship, floor) {  }

        protected override IFocusMenuProvider GetFocusProvider(Master master)
            => new ShipStockFocusMenuProvider(this, master);
    }

    internal sealed class ShipStockFocusMenuProvider : IFocusMenuProvider
    {
        const string MenuID = "shipStockFocusFloating";

        public bool IsLocked => false;
        public ShipStock Stock { get; }

        public ShipStockFocusMenuProvider(ShipStock stock, Master master) {
            Stock = stock;
            MakeMenu(master);
        }
        public void Update(Master master) {
            master.GUI.RemoveMenu(MenuID);
            MakeMenu(master);
        }
        public void Close(Master master)
            => master.GUI.RemoveMenu(MenuID);

        private void MakeMenu(Master master)
            => master.GUI.AddMenu(
                  MenuID,
                  new UI.FloatingMenu(
                      Stock, (0, -0.05f), UI.Corner.BottomLeft,
                      new UI.MenuText("Claimed by: " + (Stock.Claimant?.ToString() ?? "Nothing"), master.Font)
                      )
                  );
    }
}

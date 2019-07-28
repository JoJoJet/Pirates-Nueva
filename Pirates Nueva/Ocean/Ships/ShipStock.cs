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

        public ShipStock(ItemDef def, Ship ship, Block floor) : base(def, ship, floor) {  }
    }

}

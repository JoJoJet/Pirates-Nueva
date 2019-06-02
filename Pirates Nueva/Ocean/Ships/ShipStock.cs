using System;
using System.Collections.Generic;
using System.Text;
using Pirates_Nueva.Ocean.Agents;
#nullable enable

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

        protected override void Draw(Master master) {
            var tex = master.Resources.LoadTexture(Def.TextureID);

            (float seaX, float seaY) = Ship.ShipPointToSea(X, Y + 1);
            (int screenX, int screenY) = Ship.Sea.SeaPointToScreen(seaX, seaY);
            master.Renderer.DrawRotated(tex, screenX, screenY, Ship.Sea.PPU, Ship.Sea.PPU, -Ship.Angle, (0, 0));
        }
    }
}

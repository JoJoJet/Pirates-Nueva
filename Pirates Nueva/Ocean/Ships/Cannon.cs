using System;
using System.Collections.Generic;
using System.Text;
using Pirates_Nueva.Ocean.Agents;

namespace Pirates_Nueva.Ocean
{
    using Toil = Job<Ship, Block>.Toil;
    using Requirement = Job<Ship, Block>.Requirement;
    public class Cannon : Furniture
    {
        private Job<Ship, Block> haulJob;

        public ItemDef FuelType { get; } = ItemDef.Get("gunpowder");

        public Cannon(FurnitureDef def, Block floor, Dir direction) : base(def, floor, direction) {  }
    }
}

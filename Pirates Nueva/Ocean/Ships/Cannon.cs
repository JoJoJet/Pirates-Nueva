using Pirates_Nueva.Ocean.Agents;

namespace Pirates_Nueva.Ocean
{
    using Toil = Job<Ship, Block>.Toil;
    using StockSelector = StockSelector<Ship, Block>;
    public class Cannon : Furniture, IStockClaimant
    {
        private Job<Ship, Block>? haulJob;

        new public CannonDef Def => (CannonDef)base.Def;

        public ItemDef FuelType => ItemDef.Get(Def.FuelTypeID);
        /// <summary>
        /// The item, positioned on this <see cref="Cannon"/>, that is to be used as fuel.
        /// </summary>
        public Stock<Ship, Block>? Fuel { get; private set; }

        public Cannon(CannonDef def, Block floor, Dir direction) : base(def, floor, direction) {  }

        protected override void Update(Master master) {
            base.Update(master);
            //
            // If the haul job has been cancelled, unassign it.
            if(this.haulJob?.IsCancelled ?? false) {
                Ship.RemoveJob(this.haulJob!);
                this.haulJob = null;
            }
            //
            // If our claimed fuel isn't positioned here, or it isn't the correct type of fuel,
            // unclaim it.
            if(Fuel != null && (Fuel != Ship.GetStockOrNull(X, Y) || Fuel.Def != FuelType)) {
                Fuel.Unclaim(this);
                Fuel = null;
            }
            //
            // If we don't have any claimed fuel, try to claim some.
            if(Fuel == null) {
                //
                // If there is already a stock at this cannon with the correct fuel type,
                // claim it.
                if(Ship.TryGetStock(X, Y, out var stock) && stock.Def == FuelType && !stock.IsClaimed) {
                    Fuel = stock;
                    Fuel.Claim(this);
                }
                //
                // If there is NOT any gunpowder here, make a job to haul some over.
                else {
                    haul();
                }
            }
            //
            // If we have fuel, cancel the haul job.
            if(Fuel != null && this.haulJob != null) {
                this.haulJob.Cancel();
                Ship.RemoveJob(this.haulJob);
                this.haulJob = null;
            }


            //
            // Creates a job to haul fuel to this Cannon.
            void haul() {
                //
                // If there's already a job to haul, return early.
                if(this.haulJob != null)
                    return;
                //
                // Create a job to haul gunpowder to this cannon.
                this.haulJob = Ship.CreateJob(
                    X, Y,
                    new Toil(
                        //
                        // Place gunpowder at the job if requirements are met.
                        new PlaceStockAtAgent<Ship, Block>(),
                        //
                        // Require the agent to be holding gunpowder.
                        new IsHolding<Ship, Block>(
                            FuelType,
                            //
                            // Find and pick up some gunpowder.
                            executor: new FindAndPickUpStock<Ship, Block>(new StockSelector(FuelType))
                            ),
                        //
                        // Require the agent to be at the job.
                        new IsAtToil<Ship, Block>(
                            new Toil(
                                //
                                // Walk to the job if it's accessible.
                                new PathToToil<Ship, Block>(),
                                new IsToilAccessible<Ship, Block>()
                                )
                            )
                        )
                    );
            }
        }

        public bool Equals(IStockClaimant other) => other == this;

        void IStockClaimant.Unclaim() {
            Fuel?.Unclaim(this);
            Fuel = null;
        }
    }
}

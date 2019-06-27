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
            // If the job has been cancelled, unassign it.
            if(this.haulJob?.IsCancelled ?? false) {
                Ship.RemoveJob(this.haulJob!);
                this.haulJob = null;
            }
            //
            // If there is no gunpowder placed at this block.
            if(!Ship.TryGetStock(X, Y, out var stock) || stock.Def != FuelType) {
                //
                // If we still have some fuel claimed,
                // unclaim it.
                if(Fuel != null) {
                    Fuel.Unclaim(this);
                    Fuel = null;
                }
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
            //
            // If there IS gunpowder here.
            else {
                //
                // If we have already claimed a different Stock,
                // unclaim it.
                if(Fuel != null && Fuel != stock) {
                    Fuel.Unclaim(this);
                    Fuel = null;
                }
                //
                // If this is the first frame that the gunpowder has existed,
                // and it is unclaimed, claim it.
                if(Fuel == null && !stock.IsClaimed) {
                    Fuel = stock;
                    Fuel.Claim(this);
                }

                //
                // Cancel the job to haul gunpowder, if it still exists.
                if(Fuel != null && this.haulJob != null) {
                    this.haulJob.Cancel();
                    Ship.RemoveJob(this.haulJob);
                    this.haulJob = null;
                }
            }
        }

        public bool Equals(IStockClaimant other) => other == this;
    }
}

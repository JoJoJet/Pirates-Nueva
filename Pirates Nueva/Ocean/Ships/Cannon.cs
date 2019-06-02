using Pirates_Nueva.Ocean.Agents;

namespace Pirates_Nueva.Ocean
{
    using Toil = Job<Ship, Block>.Toil;
    public class Cannon : Furniture
    {
        private Job<Ship, Block>? haulJob;

        public ItemDef FuelType { get; } = ItemDef.Get("gunpowder");

        public Cannon(FurnitureDef def, Block floor, Dir direction) : base(def, floor, direction) {  }

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
                            new Toil(
                                //
                                // Pick up gunpowder if we are standing next to some.
                                new PickUpStock<Ship, Block>(),
                                new IsStandingAtStock<Ship, Block>(
                                    FuelType,
                                    new Toil(
                                        //
                                        // Walk to gunpowder if some exists
                                        // and is accessible.
                                        new PathToStock<Ship, Block>(FuelType),
                                        new IsStockAcesible<Ship, Block>(FuelType)
                                        )
                                    )
                                )
                            ),
                        //
                        // Require the agent to be at the job.
                        new IsAtToil<Ship, Block>(
                            new Toil(
                                //
                                // Walk to the job if it's accessible.
                                new PathToToil<Ship, Block>(),
                                new IsAccessible<Ship, Block>()
                                )
                            )
                        )
                    );
            }
            //
            // If there IS gunpowder here, but there's already a job to haul stuff here,
            // cancel the job & unassign it.
            else if(this.haulJob != null) {
                this.haulJob.Cancel();
                Ship.RemoveJob(this.haulJob);
                this.haulJob = null;
            }
        }
    }
}

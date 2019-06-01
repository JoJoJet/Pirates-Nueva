using System;
#nullable enable

namespace Pirates_Nueva.Ocean.Agents
{
    public abstract class Stock<TC, TSpot> : IDrawable
        where TC    : class, IAgentContainer<TC, TSpot>
        where TSpot : class, IAgentSpot<TC, TSpot>
    {
        /// <summary> The object that contains this <see cref="Stock{TC, TSpot}"/>. </summary>
        protected TC Container { get; }

        public ItemDef Def { get; }

        /// <summary> The spot that this instance is resting on. </summary>
        public TSpot? Spot { get; private set; }
        /// <summary> The agent that is holding this instance. </summary>
        public Agent<TC, TSpot>? Holder { get; private set; }

        /// <summary> The X coordinate of this instance, local to its container. </summary>
        public float X => Holder?.X ?? Spot!.X; // We can assume that the spot is not null,
                                                //     because it is guaranteed that either
                                                //     /Holder/ or /Spot/ will always have a value.
                                                //     (But never both at once).
        /// <summary> The Y coordinate of this instance, local to its container. </summary>
        public float Y => Holder?.Y ?? Spot!.Y;

        protected Stock(ItemDef def, TC container, TSpot spot) {
            Def = def;
            Container = container;
            Spot = spot;
            Spot.Stock = this;
        }
        protected Stock(ItemDef def, TC container, Agent<TC, TSpot> holder) {
            Def = def;
            Container = container;
            Holder = holder;
            holder.Holding = this;
        }

        /// <summary>
        /// Places this instance on the specified spot.
        /// </summary>
        public void Place(TSpot spot) {
            if(Holder != null) {
                Holder.Holding = null;
                Holder = null;
            }
            Spot = spot ?? throw new ArgumentNullException(nameof(spot));
            Spot.Stock = this;
        }
        /// <summary>
        /// Puts this instance into the hands of the specified agent.
        /// </summary>
        public void PickUp(Agent<TC, TSpot> holder) {
            if(Spot != null) {
                Spot.Stock = null;
                Spot = null;
            }
            Holder = holder ?? throw new ArgumentNullException(nameof(holder));
            Holder.Holding = this;
        }

        #region IDrawable Implementation
        void IDrawable.Draw(Master master) => Draw(master);
        /// <summary> Draws this <see cref="Stock{TC, TSpot}"/> onscreen. </summary>
        protected abstract void Draw(Master master);
        #endregion
    }
}

﻿using System;

namespace Pirates_Nueva.Ocean.Agents
{
    public interface IStockClaimant : IEquatable<IStockClaimant>
    {

    }

    public abstract class Stock<TC, TSpot> : IDrawable<TC>, IFocusable, UI.IScreenSpaceTarget
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

        /// <summary> Whether or not this Stock is currently claimed. </summary>
        public bool IsClaimed => Claimant != null;
        /// <summary> The object that has claimed this Stock, if applicable. </summary>
        public IStockClaimant? Claimant { get; private set; }

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

        #region Picking up
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
        #endregion

        #region Claiming
        public void Claim(IStockClaimant claimant) {
            if(IsClaimed) {
                throw new InvalidOperationException("This Stock has already been claimed!");
            }
            Claimant = claimant;
        }
        public void Unclaim(IStockClaimant claimant) {
            if(Claimant is null) {
                throw new InvalidOperationException("This stock is not claimed!");
            }
            if(!Claimant.Equals(claimant)) {
                throw new ArgumentException("Not the correct claimant!", nameof(claimant));
            }
            Claimant = null;
        }
        #endregion

        #region IDrawable Implementation
        void IDrawable<TC>.Draw(ILocalDrawer<TC> drawer) => Draw(drawer);
        /// <summary> Draws this <see cref="Stock{TC, TSpot}"/> onscreen. </summary>
        protected virtual void Draw(ILocalDrawer<TC> drawer) {
            var tex = Resources.LoadTexture(Def.TextureID);

            drawer.DrawCenter(tex, X, Y, width: 1, height: 1);
        }
        #endregion

        #region IScreenSpaceTarget Implementation
        int UI.IScreenSpaceTarget.X => ScreenTarget.X;
        int UI.IScreenSpaceTarget.Y => ScreenTarget.Y;
        protected abstract PointI ScreenTarget { get; }
        #endregion

        #region IFocusable Implementation
        protected bool IsFocused { get; private set; }
        bool IFocusable.IsFocused { set => IsFocused = value; }

        protected abstract IFocusMenuProvider GetFocusProvider(Master master);
        IFocusMenuProvider IFocusable.GetProvider(Master master) => GetFocusProvider(master);
        #endregion
    }
}

using System;
using System.Diagnostics.CodeAnalysis;

namespace Pirates_Nueva.Ocean.Agents
{
    /// <summary>
    /// An object that can claim <see cref="Stock{TC, TSpot}"/>.
    /// </summary>
    public interface IStockClaimant<TC, TSpot>
        where TC    : class, IAgentContainer<TC, TSpot>
        where TSpot : class, IAgentSpot<TC, TSpot>
    {
        /// <summary> Unclaims this object's claimed Stock. </summary>
        void Unclaim(Stock<TC, TSpot> stock);

        /// <summary>
        /// Returns whether or not this instance is equal to the specified <see cref="IStockClaimant"/>.
        /// </summary>
        bool Equals(IStockClaimant<TC, TSpot> other);
    }

    public class Stock<TC, TSpot> : IDrawable<TC>, IFocusable, UI.IScreenSpaceTarget
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
        public float X => Holder?.X ?? Spot?.X ?? ThrowBothNull();
        /// <summary> The Y coordinate of this instance, local to its container. </summary>
        public float Y => Holder?.Y ?? Spot?.Y ?? ThrowBothNull();

        [DoesNotReturn]
        private static float ThrowBothNull() => throw new InvalidOperationException(
            $"The properties {nameof(Holder)} and {nameof(Spot)} cannot both be null! Something went wrong.");

        /// <summary> Whether or not this Stock is currently claimed. </summary>
        public bool IsClaimed => Claimant != null;
        /// <summary> The object that has claimed this Stock, if applicable. </summary>
        public IStockClaimant<TC, TSpot>? Claimant { get; private set; }

        /// <summary> Whether or not this Stock has been destroyed. </summary>
        public bool IsDestroyed { get; private set; }

        public Stock(ItemDef def, TC container, TSpot spot) {
            Def = def;
            Container = container;
            Spot = spot;
            Spot.Stock = this;
        }
        public Stock(ItemDef def, TC container, Agent<TC, TSpot> holder) {
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
            ThrowIfDestroyed(nameof(Place));
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
            ThrowIfDestroyed(nameof(PickUp));
            if(Spot != null) {
                Spot.Stock = null;
                Spot = null;
            }
            Holder = holder ?? throw new ArgumentNullException(nameof(holder));
            Holder.Holding = this;
        }
        #endregion

        #region Claiming
        public void Claim(IStockClaimant<TC, TSpot> claimant) {
            ThrowIfDestroyed(nameof(Claim));
            if(IsClaimed) {
                throw new InvalidOperationException("This Stock has already been claimed!");
            }
            Claimant = claimant;
        }
        public void Unclaim(IStockClaimant<TC, TSpot> claimant) {
            if(Claimant is null) {
                throw new InvalidOperationException("This stock is not claimed!");
            }
            if(!Claimant.Equals(claimant)) {
                throw new ArgumentException("Not the correct claimant!", nameof(claimant));
            }
            Claimant = null;
        }
        #endregion

        /// <summary>
        /// Marks this <see cref="Stock{TC, TSpot}"/> as destroyed.
        /// <para />
        /// Also removes references to it in any of these that apply: <see cref="Claimant"/>, <see cref="Spot"/>, <see cref="Holder"/>.
        /// </summary>
        public void Destroy() {
            //
            // Remove references to this Stock.
            if(Holder != null) {
                Holder.Holding = null;
            }
            if(Spot != null) {
                Spot.Stock = null;
            }
            if(Claimant != null) {
                Claimant.Unclaim(this);
            }
            //
            // Mark it as desroyed.
            IsDestroyed = true;
        }

        /// <summary> Throws an exception if this Stock has been destroyed. </summary>
        protected void ThrowIfDestroyed(string callingMethod) {
            if(IsDestroyed)
                throw new InvalidOperationException($"{nameof(Stock<TC, TSpot>)}.{callingMethod}: This Stock has been destroyed!");
        }

        #region IDrawable Implementation
        void IDrawable<TC>.Draw<TDrawer>(TDrawer drawer) => Draw(drawer);
        /// <summary> Draws this <see cref="Stock{TC, TSpot}"/> onscreen. </summary>
        protected virtual void Draw<TDrawer>(TDrawer drawer)
            where TDrawer : ILocalDrawer<TC>
        {
            if(IsDestroyed) return;

            var tex = Resources.LoadSprite(Def.SpriteID);
            drawer.DrawCenter(tex, X, Y, width: 1, height: 1);
        }
        #endregion

        #region IScreenSpaceTarget Implementation
        int UI.IScreenSpaceTarget.X => (int)ScreenTarget.X;
        int UI.IScreenSpaceTarget.Y => (int)ScreenTarget.Y;
        private PointF ScreenTarget => Container.Transformer.PointTo<Screen>(new PointF(X + 0.5f, Y + 0.5f));
        #endregion

        #region IFocusable Implementation
        protected bool IsFocused { get; private set; }
        bool IFocusable.IsFocused { set => IsFocused = value; }

        IFocusMenuProvider IFocusable.GetProvider(Master master)
            => new FocusMenuProvider(this, master);

        protected class FocusMenuProvider : IFocusMenuProvider
        {
            const string MenuID = "shipStockFocusFloating";

            public bool IsLocked => false;
            public Stock<TC, TSpot> Stock { get; }

            public FocusMenuProvider(Stock<TC, TSpot> stock, Master master) {
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
                          new UI.Text<UI.GUI.Menu>("Claimed by: " + (Stock.Claimant?.ToString() ?? "Nothing"), master.Font)
                          )
                      );
        }
        #endregion
    }
}

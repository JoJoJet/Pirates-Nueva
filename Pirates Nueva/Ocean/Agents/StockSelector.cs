namespace Pirates_Nueva.Ocean.Agents
{
    /// <summary>
    /// An object that checks if <see cref="Stock{TC, TSpot}"/> matches some requirements.
    /// </summary>
    public class StockSelector<TC, TSpot>
        where TC    : class, IAgentContainer<TC, TSpot>
        where TSpot : class, IAgentSpot<TC, TSpot>
    {
        public ItemDef? Type { get; }

        public StockSelector(ItemDef? type = null) {
            Type = type;
        }

        /// <summary>
        /// Checks if the specified <see cref="Stock{TC, TSpot}"/> matches this Selector.
        /// </summary>
        public virtual bool Qualify(Stock<TC, TSpot> stock) {
            if(Type != null && stock.Def != Type) {
                return false;
            }
            return true;
        }
    }
}

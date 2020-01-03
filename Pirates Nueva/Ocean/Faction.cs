using System;
using System.Collections.Generic;
using System.Text;

namespace Pirates_Nueva.Ocean
{
    public sealed class Faction
    {
        private static readonly List<Faction> all = new List<Faction>();

        public static IReadOnlyList<Faction> All => all;

        /// <summary> Whether or not this is the player's <see cref="Faction"/>. </summary>
        public bool IsPlayer { get; }

        /// <param name="addToGlobal">Whether or not the new <see cref="Faction"/>
        ///                           should be added to the global list.</param>
        internal Faction(bool isPlayer, bool addToGlobal = true)
        {
            IsPlayer = isPlayer;
            if(addToGlobal)
                all.Add(this);
        }
    }
}

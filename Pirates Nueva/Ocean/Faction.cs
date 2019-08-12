using System;
using System.Collections.Generic;
using System.Text;

namespace Pirates_Nueva.Ocean
{
    public sealed class Faction
    {
        /// <summary> Whether or not this is the player's <see cref="Faction"/>. </summary>
        public bool IsPlayer { get; }

        internal Faction(bool isPlayer) => IsPlayer = isPlayer;
    }
}

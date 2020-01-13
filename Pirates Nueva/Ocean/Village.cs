﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pirates_Nueva.Ocean
{
    public sealed class Village : IDrawable<Island>
    {
        /// <summary>
        /// The minimum width of a segment of a village.
        /// </summary>
        public const int DomainUnit = 5;

        public Island Island { get; }

        public VillageDef Def { get; }

        private readonly Domain domain;

        public Village(Island island, VillageDef def)
        {
            Island = island;
            Def = def;

            //
            // Find a domain for the Village that fits on the Island.
            var domains = new List<Domain>();
            findDomain(out this.domain);

            bool findDomain(out Domain domain)
            {
                //
                // Compile a list of possible domains,
                // with one starting at each and every coordinate on the Island.
                var possibleDomains = new List<Domain>();
                for(int x = 0; x < Island.Width - DomainUnit; x++) {
                    for(int y = 0; y < Island.Height - DomainUnit; y++) {
                        //
                        // Create a minimum-sized domain at the current point.
                        var dom = new Domain() { bottomLeft = (x, y), topRight = (x + DomainUnit, y + DomainUnit) };
                        if(!fits(dom))
                            continue;
                        //
                        // Expand this domain into every possible larger shape,
                        // adding each one that fits into the list of possible domains.
                        ref int x2 = ref dom.topRight.x,
                                y2 = ref dom.topRight.y;
                        for(x2 = Island.Width-1; x2 >= x + DomainUnit; x2--) {
                            for(y2 = Island.Height-1; y2 >= y + DomainUnit; y2--) {
                                //
                                // If the domain fits, add it to the list of domains and break from the Y loop.
                                // Any future iterations of this loop will just make the domain smaller,
                                // so there's no point in checking.
                                if(fits(dom)) {
                                    possibleDomains.Add(dom);
                                    break;
                                }
                            }
                        }
                    }
                }
                //
                // Find the domain with the largest area.
                if(possibleDomains.Count > 0) {
                    domain = possibleDomains[0];
                    int area = domain.Area;
                    for(int i = 1; i < possibleDomains.Count; i++) {
                        int a = possibleDomains[i].Area;
                        if(a > area) {
                            domain = possibleDomains[i];
                            area = a;
                        }
                    }
                    return true;
                }
                //
                // If there was no domains that fit, return false.
                else {
                    domain = default;
                    return false;
                }
            }

            //
            // Returns whether or not the specified Domain would fit on the island.
            // Ensures that the domain does not overlap with others.
            bool fits(in Domain domain)
            {
                //
                // If part of the domain extends past the island,
                // we know for sure that it doesn't fit.
                if(domain.bottomLeft.x < 0 || domain.bottomLeft.y < 0
                   || domain.topRight.x >= Island.Width || domain.topRight.y >= Island.Height)
                    return false;
                //
                // Return false if the domain overlaps any other domains.
                for(int i = 0; i < domains.Count; i++) {
                    if(domains[i].Collides(domain))
                        return false;
                }
                //
                // Return false if any part of the domain goes off of the island.
                for(int x = domain.bottomLeft.x; x <= domain.topRight.x; x++) {
                    for(int y = domain.bottomLeft.y; y <= domain.topRight.y; y++) {
                        if(!Island.HasBlock(x, y))
                            return false;
                    }
                }
                //
                // If we got this far without returning, that means the domain fits.
                return true;
            }
        }

        void IDrawable<Island>.Draw<TDrawer>(in TDrawer drawer)
        {
            float left = this.domain.bottomLeft.x,
                  bottom = this.domain.bottomLeft.y,
                  right = this.domain.topRight.x,
                  top = this.domain.topRight.y;
            drawer.DrawLine((left, bottom), (right+1, bottom), UI.Color.Black);
            drawer.DrawLine((right+1, bottom), (right+1, top+1), UI.Color.Black);
            drawer.DrawLine((right+1, top+1), (left, top+1), UI.Color.Black);
            drawer.DrawLine((left, top+1), (left, bottom), UI.Color.Black);
        }

        private struct Domain
        {
            public (int x, int y) bottomLeft, topRight;

            public readonly int Area => (topRight.x - bottomLeft.x) * (topRight.y - bottomLeft.y);

            public readonly bool Contains(PointI point) => point.X >= bottomLeft.x && point.Y >= bottomLeft.y
                                                        && point.X <= topRight.x   && point.Y <= topRight.y;
            public readonly bool Collides(in Domain other)
                => other.topRight.x >= bottomLeft.x && other.topRight.y >= bottomLeft.y
                && other.bottomLeft.x <= topRight.x && other.bottomLeft.y <= topRight.y;
        }
    }
}

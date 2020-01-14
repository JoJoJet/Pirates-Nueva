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
                // The domain with the largest area.
                Domain? largest = null;
                int area = 0;
                //
                // The topmost and rightmost valid indices within this island.
                int rightmost = Island.Width-1,
                    topmost = Island.Height-1;
                //
                // Check each possible domain that could fit on the Island,
                // and save the one with the highest area.
                for(int x = 0; x < Island.Width - DomainUnit; x++) {
                    for(int y = 0; y < Island.Height - DomainUnit; y++) {
                        //
                        // Create a domain at the current point, with local aliases to its top-right corner.
                        var dom = new Domain() { bottomLeft = (x, y), topRight = (rightmost, topmost) };
                        ref int x2 = ref dom.topRight.x,
                                y2 = ref dom.topRight.y;
                        //
                        // If the largest possible domain from this point is smaller than
                        // the current largest domain so far, break from the Y loop.
                        // Increasing the base Y-index will only decrease the maximum area,
                        // so we don't have to bother checking future iterations.
                        int a = dom.Area;
                        if(a <= area)
                            break;
                        //
                        // If the smallest possible domain won't fit at this point,
                        // that means any larger domains won't fit either.
                        // Skip this iteration of the loop.
                        (x2, y2) = (x + DomainUnit, y + DomainUnit);
                        if(!fits(dom))
                            continue;
                        //
                        // Expand this domain into every possible larger shape,
                        // adding each one that fits into the list of possible domains.
                        for(x2 = rightmost; x2 >= x + DomainUnit; x2--) {
                            //
                            // Set the initial value of y2 for its loop.
                            y2 = topmost;
                            //
                            // If the domain is NOT larger than the current largest,
                            // break from both the X and Y loops.
                            // Any future iterations of either loop would just make the domain smaller,
                            // so we don't have to bother checking them.
                            a = dom.Area;
                            if(a <= area)
                                break;
                            for(; y2 >= y + DomainUnit; y2--) {
                                //
                                // If the domain is NOT larger than the current largest, break from the Y loop.
                                // We don't need to bother checking if it fits since we wouldn't use it anyway.
                                // Also, we can skip the rest of this loop, as future iterations will just be smaller.
                                a = dom.Area;
                                if(a <= area)
                                    break;
                                //
                                // If the domain fits, save it.
                                // We already know that its the largest area,
                                // as we would have broken above if it was smaller than the largest.
                                // Any future iterations of this loop will just make the domain smaller,
                                // so there's no point in checking.
                                if(fits(dom)) {
                                    largest = dom;
                                    area = a;
                                    break;
                                }
                            }
                        }
                    }
                }
                //
                // If there was at least one domain found, return it.
                if(largest != null) {
                    domain = largest.Value;
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
                // We're just gonna assume that the domain fits within the block grid.
                // If this method is used incorrectly, an exception will be thrown.

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
                        if(Island.blocks[x, y] is null)
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pirates_Nueva.Ocean
{
    public sealed class Dock
    {
        /// <summary>
        /// The indices of this <see cref="Dock"/>'s corner.
        /// When <see cref="Angle"/> = <see cref="Angle.Right"/>, this is the top left corner.
        /// </summary>
        public PointI Corner { get; }

        public Angle Angle { get; }

        public int Length { get; }

        public Dock(PointI corner, Angle angle, int length)
            => (Corner, Angle, Length) = (corner, angle, length);
    }
    public sealed class Village : IDrawable<Island>
    {
        /// <summary>
        /// The minimum width of a segment of a village.
        /// </summary>
        public const int DomainUnit = 10;

        public Island Island { get; }

        public VillageDef Def { get; }

        public Dock Dock { get; }

        private readonly List<Domain> domains;

        public Village(Island island, VillageDef def)
        {
            Island = island;
            Def = def;

            //
            // Find a domain for the Village that fits on the Island.
            var domains = new List<Domain>();
            while(findDomain(out var d)) {
                domains.Add(d);
            }
            this.domains = domains;
            //
            // Exit if there's no room on the Island for a Village.
            if(this.domains.Count == 0)
                throw null!;

            //
            // Find the dock that has both the shortest length to open sea,
            // and that has the smallest distance to the main domain of this Village.
            (PointI corner, Angle angle, int length, int distance) bestDock = ((-1, -1), default, int.MaxValue, int.MaxValue);
            var dom = this.domains[0];
            for(int y = dom.bottomLeft.y + 1; y < dom.topRight.y; y++) {
                //
                // Find a Dock facing eastward.
                // If it's more ideal than the previous ideal dock, replace the old one.
                int x = dom.topRight.x;
                var (distance, length) = findDock(x, 1);
                if(length < bestDock.length || length == bestDock.length && distance < bestDock.distance)
                    bestDock = ((x + distance - 1, y), Angle.Right, length, distance);
                //
                // Find a dock facing westward.
                x = dom.bottomLeft.x - 1;
                (distance, length) = findDock(x, -1);
                if(length < bestDock.length || length == bestDock.length && distance < bestDock.distance)
                    bestDock = ((x - distance + 1, y - 1), Angle.Left, length, distance);

                (int distance, int length) findDock(int x, int dir)
                {
                    //
                    // Find the distance from the current point on the edge of the Domain
                    // to the shore of the island.
                    int distance = 1;
                    while(Island.HasBlock(x + distance * dir, y-1) && island.HasBlock(x + distance * 1, y))
                        distance++;
                    //
                    // Find the length of the dock starting from the shore.
                    int length = 0;
                    while(true) {
                        //
                        // If any part under the dock has land, increase the length and repeat the loop.
                        if(Island.HasBlock(x + (distance + length) * dir, y+1)
                        || Island.HasBlock(x + (distance + length) * dir, y)
                        || Island.HasBlock(x + (distance + length) * dir, y-1)
                        || Island.HasBlock(x + (distance + length) * dir, y-2))
                            goto skip;
                        //
                        // If there is any land perpendicular to this point in the dock, increase the length and repeat.
                        for(int r = 0; r < Math.Max(Island.Width, Island.Height); r++) {
                            if(Island.HasBlock(x + (distance + length) * dir, y + r)
                            || Island.HasBlock(x + (distance + length) * dir, y - 1 - r))
                                goto skip;
                        }
                        //
                        // If we got this far, then that means its clear ocean from here on out.
                        // Increase the length of the dock by a constant and break from the loop.
                        length += 5;
                        break;

                    skip:
                        length++;
                    }

                    return (distance, length);
                }
            }
            for(int x = dom.bottomLeft.x + 1; x < dom.topRight.x; x++) {
                int y = dom.bottomLeft.y - 1;
                //
                // Find the distance from the current point on the edge of the Domain
                // to the shore of the Island.
                int distance = 1;
                while(Island.HasBlock(x-1, y - distance) && Island.HasBlock(x, y - distance))
                    distance++;
                //
                // Find the length of the dock starting from the shore.
                int length = 0;
                while(true) {
                    //
                    // If any part under the dock has land, increase the length and repeat the loop.
                    if(Island.HasBlock(x+1, y - distance - length) || Island.HasBlock(x, y - distance - length)
                    || Island.HasBlock(x-1, y - distance - length) || Island.HasBlock(x-2, y - distance - length))
                        goto skip;
                    //
                    // If there is any land perpendicular to this point in the dock, increase the length and repeat.
                    for(int r = 0; r < Math.Max(Island.Width, Island.Height); r++) {
                        if(Island.HasBlock(x + r,     y - distance - length)
                        || Island.HasBlock(x - 1 - r, y - distance - length))
                            goto skip;
                    }
                    //
                    // If we got this far, then that means its clear ocean from here on out.
                    // Increase the length of the dock by a constant and break from the loop.
                    length += 5;
                    break;

                skip:
                    length++;
                }
                //
                // If the dock is more ideal than the previous ideal dock, replace the old one.
                if(length < bestDock.length || length == bestDock.length && distance < bestDock.distance)
                    bestDock = ((x, y - distance + 1), Angle.Down, length, distance);
            }

            Dock = new Dock(bestDock.corner, bestDock.angle, bestDock.length);

            bool findDomain(out Domain domain)
            {
                //
                // The domain with the largest area.
                Domain? largest = null;
                int maxArea = 0;
                //
                // The topmost and rightmost edges of the Island.
                int rightmost = Island.Width,
                    topmost = Island.Height;
                //
                // Check each possible domain that could fit on the Island,
                // and save the one with the highest area.
                for(int x = 0; x < Island.Width - DomainUnit; x++) {
                    for(int y = 0; y < Island.Height - DomainUnit; y++) {
                        //
                        // The maximum value for the top edge of the island.
                        // The idea is that at most, the height should be twice the width.
                        int maxY2 = Math.Min(topmost, y + 2 * (rightmost - x));
                        //
                        // Create a domain at the current point, with local aliases to its top-right corner.
                        var dom = new Domain((x, y), (rightmost, maxY2));
                        ref int x2 = ref dom.topRight.x,
                                y2 = ref dom.topRight.y;
                        //
                        // If the largest possible domain from this point is smaller than
                        // the current largest domain so far, break from the Y loop.
                        // Increasing the base Y-index will only decrease the maximum area,
                        // so we don't have to bother checking future iterations.
                        int a = dom.Area;
                        if(a <= maxArea)
                            break;
                        //
                        // If the smallest possible domain won't fit at this point,
                        // that means any larger domains won't fit either.
                        // Skip this iteration of the loop.
                        if(!fits(new Domain((x, y), (x + DomainUnit, y + DomainUnit))))
                            continue;
                        //
                        // Iterate over the right edge of the domain, starting at the rightmost
                        // edge of the island and moving leftward to the left edge of the domain.
                        // If the current area drops below the max area, break early.
                        // Any future iterations will decrease the area, so we don't need to check them.
                        while(x2 >= x + DomainUnit && a > maxArea) {
                            //
                            // Find the minimum and maximum value for the top edge as the right edge changes.
                            maxY2 = Math.Min(topmost, y + 2 * (x2 - x));
                            int minY2 = y + Math.Max(DomainUnit, (x2 - x) / 2);
                            //
                            // Iterate over the top edge of the domain, starting at the topmost
                            // edge of the island, and moving downward to the bottom edge of the domain.
                            // If the current area drops below the max area, break early.
                            // Any future iterations will decrease the area, so we don't have to check them.
                            for(; y2 >= minY2 && a > maxArea; y2--) {
                                //
                                // If the domain fits, save it, as we already know that its larger
                                // than the previous larget domain.
                                // We can also break from the Y loop as any future iterations will decrease the area.
                                if(fits(dom)) {
                                    largest = dom;
                                    maxArea = a;
                                    break;
                                }
                                //
                                // Update the area of the current domain.
                                a = dom.Area;
                            }
                            //
                            // Reset the top edge of the /domain/ to the top edge of the /island/,
                            // and decrement the right edge of the domain. Also, recalculate the current area.
                            y2 = maxY2; x2--;
                            a = dom.Area;
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
                int left = domain.bottomLeft.x, right = domain.topRight.x-1,
                    bottom = domain.bottomLeft.y, top = domain.topRight.y-1;
                //
                // Return false if any part of the domain goes off of the island.
                // We only need to check the outer rim of the domain,
                // as it's impossible for an island to have holes.
                // The first part of the rim that we check are the four corners, as those are the most
                // likely to be invalid.
                // TODO: If we add lakes or mountains in the future, we have to check for that.
                if(island.blocks[left,  bottom] is null || island.blocks[right, bottom] is null
                || island.blocks[right, top]    is null || island.blocks[left,  top]    is null)
                    return false;
                for(int x = left+1; x < right; x++) {
                    if(island.blocks[x, bottom] is null || island.blocks[x, top] is null)
                        return false;
                }
                for(int y = bottom+1; y < top; y++) {
                    if(island.blocks[left, y] is null || island.blocks[right, y] is null)
                        return false;
                }
                //
                // If we got this far without returning, that means the domain fits.
                return true;
            }
        }

        void IDrawable<Island>.Draw<TDrawer>(in TDrawer drawer)
        {
            foreach(var domain in this.domains) {
                float left   = domain.bottomLeft.x,
                      bottom = domain.bottomLeft.y,
                      right  = domain.topRight.x,
                      top    = domain.topRight.y;
                drawer.DrawLine((left, bottom),  (right, bottom), UI.Color.Black);
                drawer.DrawLine((right, bottom), (right, top),    UI.Color.Black);
                drawer.DrawLine((right, top),    (left, top),     UI.Color.Black);
                drawer.DrawLine((left, top),     (left, bottom),  UI.Color.Black);
            }

            if(Dock.Angle == Angle.Right) {
                int left = Dock.Corner.X,
                    bottom = Dock.Corner.Y - 1,
                    right = Dock.Corner.X + Dock.Length + 1,
                    top = Dock.Corner.Y + 1;

                drawer.DrawLine((left, top),    (right, top),    UI.Color.PaleYellow);
                drawer.DrawLine((left, bottom), (right, bottom), UI.Color.PaleYellow);
            }
            else if(Dock.Angle == Angle.Left) {
                int left = Dock.Corner.X - Dock.Length,
                    bottom = Dock.Corner.Y,
                    right = Dock.Corner.X + 1,
                    top = Dock.Corner.Y + 2;

                drawer.DrawLine((left, top),    (right, top),    UI.Color.PaleYellow);
                drawer.DrawLine((left, bottom), (right, bottom), UI.Color.PaleYellow);
            }
            else if(Dock.Angle == Angle.Down) {
                int left = Dock.Corner.X - 1,
                    bottom = Dock.Corner.Y - Dock.Length,
                    right = Dock.Corner.X + 1,
                    top = Dock.Corner.Y + 1;

                drawer.DrawLine((left,  top), (left,  bottom), UI.Color.PaleYellow);
                drawer.DrawLine((right, top), (right, bottom), UI.Color.PaleYellow);
            }
        }

        private struct Domain
        {
            public (int x, int y) bottomLeft, topRight;

            public Domain((int x, int y) bl, (int x, int y) tr) => (bottomLeft, topRight) = (bl, tr);

            public readonly int Area => (topRight.x - bottomLeft.x) * (topRight.y - bottomLeft.y);

            public readonly bool Contains(PointI point) => point.X >= bottomLeft.x && point.Y >= bottomLeft.y
                                                        && point.X <  topRight.x   && point.Y <  topRight.y;
            public readonly bool Collides(in Domain other)
                => other.topRight.x >= bottomLeft.x && other.topRight.y >= bottomLeft.y
                && other.bottomLeft.x <= topRight.x && other.bottomLeft.y <= topRight.y;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Pirates_Nueva.Ocean.Agents;
using Pirates_Nueva.Path;

namespace Pirates_Nueva.Ocean
{
    public sealed class Island : AgentBlockContainer<Island, IslandBlock>,
        IAgentContainer<Island, IslandBlock>, ISpaceLocus<Island>, IUpdatable, IDrawable<Sea>, IFocusableParent
    {
        /// <summary>
        /// The underlying matrix containing the <see cref="IslandBlock"/>s for this <see cref="Island"/>.
        /// This should only be accessed in a performance-sensitive part of code.
        /// </summary>
        internal readonly IslandBlock?[,] blocks;

        public Sea Sea { get; }

        /// <summary> The left edge of this <see cref="Island"/>, in <see cref="Ocean.Sea"/>-space. </summary>
        public float Left { get; }
        /// <summary> The bottom edge of this <see cref="Island"/>, in <see cref="Ocean.Sea"/>-space. </summary>
        public float Bottom { get; }

        /// <summary> The right edge of this <see cref="Island"/>, in <see cref="Ocean.Sea"/>-space. </summary>
        public float Right => Left + Width;
        /// <summary> The top edge of this <see cref="Island"/>, in <see cref="Ocean.Sea"/>-space. </summary>
        public float Top => Bottom + Height;

        public int Width => this.blocks.GetLength(0);
        public int Height => this.blocks.GetLength(1);

        public Space<Island, IslandTransformer> Transformer { get; }

        public Village Village { get; }

        public Island(Sea sea, int left, int bottom, int rngSeed) {
            //
            // Copy over basic information
            Sea = sea;
            Left = left; Bottom = bottom;

            Transformer = new Space<Island, IslandTransformer>(this);

            //
            // Generate the Island.
            Random r = new Random(rngSeed);

            var shape = GenerateShape(r);
            var (vertices, edges) = FindOutline(shape, r);
            this.blocks = FindBlocks(this, vertices, edges);

            //
            // Create a Village.
            Village = new Village(this, VillageDef.Get("fishing"));

            for(int x = 0; x < Width; x++) {
                for(int y = 0; y < Height; y++) {
                    if(GetBlockOrNull(x, y) is IslandBlock b) {
                        AddAgent(x, y);
                        x = Width; break;
                    }
                }
            }
        }

        #region Shape Generation
        static bool[,] GenerateShape(Random r) {
            const int Width = 15, Height = 15;

            var ground = new bool[Width, Height];

            placeBlobs();     // Scatter shapes around the canvas.
            connectEdges();   // Connect separated but close blocks.
            floodFill();      // Fill in the entire terrain.

            decimate();       // Randomly kill 20% of the blocks.
            floodFill();      // Fill in the terrain.

            breakNecks();     // Break any thin connectors.
            slideSeperates(); // Combine any stray islands into one shape.

            connectEdges();   // Connnect separated but close blocks.
            floodFill();      // Fill in the terrain.

            extraneous();     // Delete unnatural extrusions.
            breakNecks();     // Break any thin connectors.
            slideSeperates(); // Combine any stray shapes into one.

            connectEdges();   // Connect separated but close blocks.
            floodFill();      // Fill in the terrain.

            return ground;

            /*
             * Local Methods.
             */
            void placeBlobs() {
                const int Radius = 3;
                PointI[] shape = {                           // Defines the shape of a blob.
                                   (0,  2),
                         (-1,  1), (0,  1), (1,  1),
                (-2, 0), (-1,  0), (0,  0), (1,  0), (2, 0),
                         (-1, -1), (0, -1), (1, -1),
                                   (0, -2)
                };

                const int BlobCount = 6;                     // Defines the number of blobs to place.
                for(int i = 0; i < BlobCount; i++) {         // Repeat for every blob to place:
                    int x = r.Next(Radius, Width - Radius);  // Choose a random /x/ coordinate in the island.
                    int y = r.Next(Radius, Height - Radius); // Choose a random /y/ coordinate in the island.
                    foreach(var s in shape)                  // Place a blob centered at /x/, /y/.
                        ground[x + s.X, y + s.Y] = true;
                }
            }

            void connectEdges() {
                for(int x = 1; x < Width - 1; x++) {
                    for(int y = 1; y < Height - 1; y++) {
                        if(!ground[x, y] && r.Next(0, 100) < 80) {
                            //   0   //
                            // 1 0 1 //
                            //   0   //
                            if(ground[x - 1, y] && ground[x + 1, y] && !ground[x, y + 1] && !ground[x, y - 1])
                                ground[x, y] = true;
                            //   1   //
                            // 0 0 0 //
                            //   1   //
                            else if(ground[x, y + 1] && ground[x, y - 1] && !ground[x - 1, y] && !ground[x + 1, y])
                                ground[x, y] = true;
                            // 1   0 //
                            //   0   //
                            // 0   1 //
                            else if(ground[x - 1, y + 1] && ground[x + 1, y - 1] && !ground[x - 1, y - 1] && !ground[x + 1, y + 1])
                                ground[x, y] = true;
                            // 0   1 //
                            //   0   //
                            // 1   0 //
                            else if(ground[x + 1, y + 1] && ground[x - 1, y - 1] && !ground[x - 1, y + 1] && !ground[x + 1, y - 1])
                                ground[x, y] = true;
                        }
                    }
                }
            }

            void floodFill() {
                var fill = new bool[Width, Height];   // An empty 2D array.
                for(int x = 0; x < Width; x++) {      // For every point in the array:
                    for(int y = 0; y < Height; y++) { //
                        fill[x, y] = true;            // Default its to be filled in.
                    }
                }
                doFloodFill(ground, (0, Height-1), (x, y) => fill[x, y] = false);
                ground = fill;

                void doFloodFill(bool[,] canvas, PointI start, Action<int, int> paint) {
                    var box = new BoundingBox(0, 0, Width-1, Height-1);

                    var frontier = new Stack<PointI>(); // List of pixels to be searched.
                        frontier.Push(start);
                    var known = new HashSet<PointI>();  // Pixels that have already been searched.

                    while(frontier.Count > 0) {      // While there are coordinates to be searched:
                        var (x, y) = frontier.Pop(); // Get a coordinate to search, /x/, /y/.
                        known.Add((x, y));           // Add it to the list of searched coordinates.
                                                     //
                        paint(x, y);                 // Color in that coordinate.
                                                     //
                        neighbor(x - 1, y);          // Mark its left adjacent neighbor to be searched, if it wasn't already.
                        neighbor(x, y + 1);          // Do the same for its upward neighbor,
                        neighbor(x + 1, y);          // its rightward neighbor,
                        neighbor(x, y - 1);          // and its downward neighbor.
                    }
                    void neighbor(int x, int y) {
                        if(box.Contains(x, y) && !canvas[x, y] && !known.Contains((x, y))) // If the point is empty & not yet searched,
                            frontier.Push((x, y));                                         //     mark it to be searched later on.
                    }
                }
            }

            void decimate() {
                for(int x = 0; x < Width; x++) {                // For every ground pixel:
                    for(int y = 0; y < Height; y++) {           //
                        if(ground[x, y] && r.Next(0, 100) < 20) // Have a 20% chance to delete the pixel.
                            ground[x, y] = false;
                    }
                }
            }

            void breakNecks() {
                /*
                 * Delete longer thin connectors.
                 */
                for(int x = 1; x < Width-2; x++) {
                    for(int y = 1; y < Height-2; y++) {
                        if(q(x, y)) {
                            //   1   //
                            // 0 1 0 //
                            // 0 1 0 //
                            //   1   //
                            if(q(x, y+2) && !q(x-1, y+1) && q(x, y+1) && !q(x+1, y+1) && !q(x-1, y) && !q(x+1, y) && q(x, y-1)) {
                                if(r.Next(0, 100) < 80)
                                    ground[x, y  ] = false;
                                if(r.Next(0, 100) < 80)
                                    ground[x, y+1] = false;
                            }
                            //   0 0   //
                            // 1 1 1 1 //
                            //   0 0   //
                            else if(!q(x, y+1) && !q(x+1, y+1) && q(x-1, y) && q(x+1, y) && q(x+2, y) && !q(x, y-1) && !q(x+1, y-1)) {
                                if(r.Next(0, 100) < 80)
                                    ground[x  , y] = false;
                                if(r.Next(0, 100) < 80)
                                    ground[x+1, y] = false;
                            }
                        }
                    }
                }
                /*
                 * Delete long-ish thin connectors.
                 */
                for(int x = 1; x < Width-1; x++) {
                    for(int y = 1; y < Height-1; y++) {
                        if(q(x, y) && r.Next(0, 100) < 75) {
                            //   0   //
                            // 1 1 1 //
                            //   0   //
                            if(!q(x, y+1) && q(x-1, y) && q(x+1, y) && !q(x, y-1))
                                ground[x, y] = false;
                            //   1   //
                            // 0 1 0 //
                            //   1   //
                            else if(q(x, y+1) && !q(x-1, y) && !q(x+1, y) && q(x, y-1))
                                ground[x, y] = false;
                        }
                    }
                }
                bool q(int x, int y) => ground[x, y];
            }

            void slideSeperates() {
                var seperates = findSeperates();                // Find the separated chunks of the island.
                while(seperates.Count > 1) {                    // Loop until there is only one island:
                    ground = new bool[Width, Height];           // Clear the ground pixels,
                    foreach(var p in seperates.Skip(1).Union()) //     and populate them with
                        ground[p.X, p.Y] = true;                //         the seperate chunks, except the first one.
                                                                //
                    doSlide(seperates.First());                 // Slide the first separated chunk into the mainland.
                    seperates = findSeperates();                // Re-compute the seperated chunks of the island.
                }

                List<List<PointI>> findSeperates() {
                    var fragments = new HashSet<PointI>();
                    for(int x = 0; x < Width; x++) {      // For every point in the island:
                        for(int y = 0; y < Height; y++) { //
                            if(ground[x, y])              // If there is a ground pixel there,
                                fragments.Add((x, y));    //     add the point to the list.
                        }
                    }

                    var chunks = new List<List<PointI>>();
                    while(fragments.Count > 0) {
                        var frontier = new Queue<PointI>();     // Coordinates to be searched.
                            frontier.Enqueue(fragments.Last()); //
                        var known = new List<PointI>();         // Coordinates that have already been searched.

                        while(frontier.Count > 0) {       // While there are coordinates to be searched:
                            var c = frontier.Dequeue();   // Get a coordinate to search.
                            fragments.Remove(c);          // Remove it from the list of loose fragments,
                            known.Add(c);                 // and add it to the list of searched coordinates.
                            //
                            // Mark its neighbors to be searched, if they haven't been searched already.
                            peripheral(new PointI(c.X - 1, c.Y));
                            peripheral(new PointI(c.X, c.Y + 1));
                            peripheral(new PointI(c.X + 1, c.Y));
                            peripheral(new PointI(c.X, c.Y - 1));
                        }

                        chunks.Add(known);

                        void peripheral(PointI p) {
                            if(fragments.Contains(p)) // If the specified point is still loose,
                                frontier.Enqueue(p);  //     mark it to be searched later on.
                        }
                    }

                    return chunks.OrderBy(s => s.Count).ToList();
                }

                void doSlide(List<PointI> isolated) {
                    PointI? slide = null; // The direction and amount of sliding.

                    var order = from n in new[] { 0, 1, 2, 3 } orderby r.Next() select n;
                    foreach(int n in order) {
                        switch(n) {
                            // Try to slide leftwards.
                            case 0:
                                trySlide(findEdge((p) => p.Y, (p1, p2) => p1.X > p2.X), (-1, 0));
                                break;
                            // Try to slide downward.
                            case 1:
                                trySlide(findEdge((p) => p.X, (p1, p2) => p1.Y > p2.Y), (0, -1));
                                break;
                            // Try to slide rightward.
                            case 2:
                                trySlide(findEdge((p) => p.Y, (p1, p2) => p1.X < p2.X), (1, 0));
                                break;
                            // Try to slide upward.
                            case 3:
                                trySlide(findEdge((p) => p.X, (p1, p2) => p1.Y < p2.Y), (0, 1));
                                break;
                        }
                    }

                    if(slide is PointI s) {                       // If the isolated chunk can be slid,
                        for(int i = 0; i < isolated.Count; i++) { //     draw the chunk to the ground pixels,
                            var (x, y) = isolated[i] + s;         //         offset by the slide amount.
                            ground[x, y] = true;
                        }
                    }

                    IEnumerable<PointI> findEdge(Func<PointI, int> indexer, Func<PointI, PointI, bool> isFurther) {
                        var ed = new PointI?[Math.Max(Width, Height)]; // An array of edge blocks.
                        foreach(var p in isolated) {                   // For every block in the chunk:
                            ref var ep = ref ed[indexer(p)];           //
                            if(ep == null || isFurther(ep.Value, p))   // If it is the closest block to the edge in this axis,
                                ep = p;                                //     add it to the array of edges.
                        }
                        return from e in ed where e != null select e.Value; // Return an array of (initialized) edge blocks.
                    }

                    void trySlide(IEnumerable<PointI> edge, PointI dir) {
                        var box = new BoundingBox(0, 0, Width-1, Height-1);         // A box encompassing the island.
                        foreach(var e in edge) {                                    // For every edge block:
                            for(int i = 0; box.Contains(e + dir * i); i++) {        // Cast a ray from that block in the specified direction.
                                if(hasAdjacent(e.X + dir.X * i, e.Y + dir.Y * i)) { // If this point in the ray is touching the mainland,
                                    if((slide?.SqrMagnitude > i*i || slide == null) //     the point is closer than the previous candidate,
                                        && checkBounds(dir * i))                    //     and it is within the bounds of the island,
                                        slide = dir * i;                            //         set the best best candidate to be this point.
                                    break;                                          // Stop raycasting.
                                }
                            }
                        }
                    }

                    bool hasAdjacent(int x, int y) => (x > 0 && ground[x - 1, y]) || (y < Height - 1 && ground[x, y + 1]) ||
                                                      (x < Width - 1 && ground[x + 1, y]) || (y > 0 && ground[x, y - 1]);

                    bool checkBounds(PointI offset) {
                        foreach(var p in isolated) {                        // For every point in the isolated chunk:
                            var (x, y) = p + offset;                        //
                            if(x < 0 || x >= Width || y < 0 || y >= Height) // If it is beyond the bounds of the island,
                                return false;                               //     return false.
                        }                                                   // If we got this far without exiting the method,                                     
                        return true;                                        //     return true.
                    }
                }
            }

            void extraneous() {
                /*
                 * Delete parts of the island that are extruding too far.
                 */
                for(int x = 0; x < Width; x++) {
                    for(int y = 0; y < Height; y++) {
                        // 0 1 0 //
                        // 0 1 0 //
                        // 0 1 0 //
                        // 0 0 0 //
                        if(
                           !q(x-1, y+2) &&  q(x, y+2) && !q(x+1, y+2) &&
                           !q(x-1, y+1) &&  q(x, y+1) && !q(x+1, y+1) &&
                           !q(x-1, y  ) &&  q(x, y  ) && !q(x+1, y  ) &&
                           !q(x-1, y-1) && !q(x, y-1) && !q(x+1, y-1)
                          )
                            ground[x, y] = false;
                        // 0 0 0 0 //
                        // 0 1 1 1 //
                        // 0 0 0 0 //
                        else if(
                                !q(x-1, y+1) && !q(x, y+1) && !q(x+1, y+1) && !q(x+2, y+1) &&
                                !q(x-1, y  ) &&  q(x, y  ) &&  q(x+1, y  ) &&  q(x+2, y  ) &&
                                !q(x-1, y-1) && !q(x, y-1) && !q(x+1, y-1) && !q(x+2, y-1)
                               )
                            ground[x, y] = false;
                        // 0 0 0 //
                        // 0 1 0 //
                        // 0 1 0 //
                        // 0 1 0 //
                        else if(
                                !q(x-1, y+1) && !q(x, y+1) && !q(x+1, y+1) &&
                                !q(x-1, y  ) &&  q(x, y  ) && !q(x+1, y  ) &&
                                !q(x-1, y-1) &&  q(x, y-1) && !q(x+1, y-1) &&
                                !q(x-1, y-2) &&  q(x, y-2) && !q(x+1, y-2)
                               )
                            ground[x, y] = false;
                        // 0 0 0 0 //
                        // 1 1 1 0 //
                        // 0 0 0 0 //
                        else if(
                                !q(x-2, y+1) && !q(x-1, y+1) && !q(x, y+1) && !q(x+1, y+1) &&
                                 q(x-2, y  ) &&  q(x-1, y  ) &&  q(x, y  ) && !q(x+1, y  ) &&
                                !q(x-2, y-1) && !q(x-1, y-1) && !q(x, y-1) && !q(x+1, y-1)
                               )
                            ground[x, y] = false;
                    }
                }

                /*
                 * Have a chance to delete parts of the island that are extruding a bit.
                 */
                for(int x = 0; x < Width; x++) {
                    for(int y = 0; y < Height; y++) {
                        if(q(x, y) && r.Next(0, 100) < 75) {
                            // 0 1 0 //
                            // 0 1 0 //
                            // 0 0 0 //
                            if(
                               !q(x - 1, y + 1) && q(x, y + 1) && !q(x + 1, y + 1) &&
                               !q(x - 1, y) && !q(x + 1, y) &&
                               !q(x - 1, y - 1) && !q(x, y - 1) && !q(x + 1, y - 1)
                              )
                                ground[x, y] = false;
                            // 0 0 0 //
                            // 0 1 1 //
                            // 0 0 0 //
                            else if(
                                    !q(x-1, y+1) && !q(x, y+1) && !q(x+1, y+1) &&
                                    !q(x-1, y  ) &&                q(x+1, y  ) &&
                                    !q(x-1, y-1) && !q(x, y-1) && !q(x+1, y-1)
                                   )
                                ground[x, y] = false;
                            // 0 0 0 //
                            // 0 1 0 //
                            // 0 1 0 //
                            else if(
                                    !q(x-1, y+1) && !q(x, y+1) && !q(x+1, y+1) &&
                                    !q(x-1, y  ) &&               !q(x+1, y  ) &&
                                    !q(x-1, y-1) &&  q(x, y-1) && !q(x+1, y-1)
                                   )
                                ground[x, y] = false;
                            // 0 0 0 //
                            // 1 1 0 //
                            // 0 0 0 //
                            else if(
                                    !q(x-1, y+1) && !q(x, y+1) && !q(x+1, y+1) &&
                                     q(x-1, y  ) &&               !q(x+1, y  ) &&
                                    !q(x-1, y-1) && !q(x, y-1) && !q(x+1, y-1)
                                   )
                                ground[x, y] = false;
                        }
                    }
                }

                bool q(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height && ground[x, y];
            }
        }

        /// <summary>
        /// Variables for the marching squares algorithm, held by reference.
        /// </summary>
        private ref struct OutlineParams
        {
            public Span<PointF> verts;
            public int vCount;
            public Span<(int a, int b)> edges;
            public int eCount;
        }

        static (Vertex[], Edge[]) FindOutline(bool[,] shape, Random r) {
            int width = shape.GetLength(0), height = shape.GetLength(1);

            //
            // Create some reference fields to hold
            // values for a zero-alloc list pattern.
            var @params = new OutlineParams() {
                verts = stackalloc PointF[32],
                vCount = 0,
                edges = stackalloc (int, int)[32],
                eCount = 0
            };
            //
            // Run the Marching Squares algorithm on the pixels
            // to generate an initial outline surrounding the Island.
            // https://en.wikipedia.org/wiki/Marching_squares.
            {
                for(int x = -1; x < width; x++) {
                    for(int y = -1; y < height; y++) {
                        //
                        // If the array of edges is full,
                        // increase its size.
                        if(@params.eCount + 2 >= @params.edges.Length) {
                            Span<(int a, int b)> _edges = stackalloc (int, int)[@params.edges.Length * 2];
                            for(int i = 0; i < @params.edges.Length; i++)
                                _edges[i] = @params.edges[i];
                            @params.edges = _edges;
                        }
                        //
                        // If the array of vertices is full,
                        // increase its size.
                        if(@params.vCount + 4 > @params.verts.Length) {
                            Span<PointF> verts = stackalloc PointF[@params.verts.Length * 2];
                            for(int i = 0; i < @params.verts.Length; i++)
                                verts[i] = @params.verts[i];
                            @params.verts = verts;
                        }

                        var lookup = b(x, y + 1) << 3 | b(x + 1, y + 1) << 2 | b(x + 1, y) << 1 | b(x, y);

                        const float Whole = 1f;        // The width of a single cell.
                        const float Half = Whole / 2f; // Half the width of a cell.
                        const float Ro2 = 0.70710678118f;
                        switch(lookup) {
                            // 0 0 //
                            // 0 0 //
                            case 0:

                                break;
                            // 0 0 //
                            // 1 0 //
                            case 1:
                                addEdge((0, Half), (Half, 0), Ro2, Ro2, ref @params);
                                break;
                            // 0 0 //
                            // 0 1 //
                            case 2:
                                addEdge((Half, 0), (Whole, Half), -Ro2, Ro2, ref @params);
                                break;
                            // 0 0 //
                            // 1 1 //
                            case 3:
                                addEdge((0, Half), (Whole, Half), 0, 1, ref @params);
                                break;
                            // 0 1 //
                            // 0 0 //
                            case 4:
                                addEdge((Half, Whole), (Whole, Half), -Ro2, -Ro2, ref @params);
                                break;
                            // 0 1 //
                            // 1 0 //
                            case 5:
                                addEdge((0, Half), (Half, Whole), -Ro2, Ro2, ref @params);
                                addEdge((Half, 0), (Whole, Half), Ro2, -Ro2, ref @params);
                                break;
                            // 0 1 //
                            // 0 1 //
                            case 6:
                                addEdge((Half, 0), (Half, Whole), -1, 0, ref @params);
                                break;
                            // 0 1 //
                            // 1 1 //
                            case 7:
                                addEdge((0, Half), (Half, Whole), -Ro2, Ro2, ref @params);
                                break;
                            // 1 0 //
                            // 0 0 //
                            case 8:
                                addEdge((0, Half), (Half, Whole), Ro2, -Ro2, ref @params);
                                break;
                            // 1 0 //
                            // 1 0 //
                            case 9:
                                addEdge((Half, 0), (Half, Whole), 1, 0, ref @params);
                                break;
                            // 1 0 //
                            // 0 1 //
                            case 10:
                                addEdge((0, Half), (Half, 0), -Ro2, -Ro2, ref @params);
                                addEdge((Half, Whole), (Whole, Half), Ro2, Ro2, ref @params);
                                break;
                            // 1 0 //
                            // 1 1 //
                            case 11:
                                addEdge((Half, Whole), (Whole, Half), Ro2, Ro2, ref @params);
                                break;
                            // 1 1 //
                            // 0 0 //
                            case 12:
                                addEdge((0, Half), (Whole, Half), 0, -1, ref @params);
                                break;
                            // 1 1 //
                            // 1 0 //
                            case 13:
                                addEdge((Half, 0), (Whole, Half), Ro2, -Ro2, ref @params);
                                break;
                            // 1 1 //
                            // 0 1 //
                            case 14:
                                addEdge((0, Half), (Half, 0), -Ro2, -Ro2, ref @params);
                                break;
                            // 1 1 //
                            // 1 1 //
                            case 15:

                                break;
                        }


                        void addEdge(PointF alpha, PointF beta, float normalX, float normalY, ref OutlineParams @params) {
                            var indices = (makeIndex((x, y) + alpha, ref @params), makeIndex((x, y) + beta, ref @params));
                            @params.edges[@params.eCount] = indices;
                            @params.eCount++;

                            static int makeIndex(PointF v, ref OutlineParams @params) {
                                var i = @params.verts.IndexOf(v);      // Get the index of the specified vertex.
                                if(i >= 0) {                           // If the index exists,
                                    return i;                          //     return it.
                                }                                      //
                                else {                                 // If the index does NOT exist,
                                    @params.verts[@params.vCount] = v; //     add the specified vertex,
                                    return @params.vCount++;           // and return its index.
                                }
                            }
                        }

                        int b(int g, int h) => g >= 0 && g < width && h >= 0 && h < height && shape[g, h] ? 1 : 0;
                    }
                }

                //
                // Slice down the arrays of vertices and edges.
                @params.verts = @params.verts.Slice(0, @params.vCount);
                @params.edges = @params.edges.Slice(0, @params.eCount);
            }

            ref var vertices = ref @params.verts;
            ref var edges = ref @params.edges;
            smoothOutline(vertices, edges);               // Smooth the outline.
            scaleOutline(ref vertices, ref edges);        // Scale up the islands by a random amount.
            jitterOutline(vertices);                      // Roughen up the outline.
            superOutline(vertices);                       // Scale the island up by four.
            alignOutline(vertices);                       // Align the outline to the bottom left of this island.
            return compile(vertices, edges);
            
            /*
             * Local Methods.
             */
            static void smoothOutline(Span<PointF> vertices, Span<(int a, int b)> edges) {
                Span<PointF> verts = stackalloc PointF[vertices.Length];

                //
                // Apply Laplacian smoothing to the outline.
                // https://en.wikipedia.org/wiki/Laplacian_smoothing.
                //
                for(int i = 0; i < vertices.Length; i++) {
                    //
                    // Get each neighbor of the vertex:
                    // Each vertex that is connected to the current one by an Edge.
                    Span<PointF> ns = stackalloc PointF[4];
                    int count = 0;
                    foreach(var e in edges) {
                        if(e.a == i || e.b == i) {
                            if(count >= ns.Length)
                                ns = stackalloc PointF[ns.Length * 2];
                            ns[count] = vertices[e.a == i ? e.b : e.a];
                            count++;
                        }
                    }
                    //
                    // Find the smoothed position of the vertex by averaging its neighbors' positions.
                    var xi = PointF.Zero;
                    foreach(var n in ns) {
                        xi += n; 
                    }
                    xi /= count;
                    //
                    // The new pos of the vertex is the midpoitn of
                    // the smoothed and original positions.
                    verts[i] = PointF.Lerp(vertices[i], xi, 0.5f);
                }

                for(int i = 0; i < verts.Length; i++) {
                    vertices[i] = verts[i];
                }
            }

            static void alignOutline(Span<PointF> vertices) {
                float leftmost = float.MaxValue;
                float bottommost = float.MaxValue;
                //
                // Find the furthest left and furthest down positions of the vertices.
                foreach(var v in vertices) {
                    if(v.X < leftmost)
                        leftmost = v.X;
                    if(v.Y < bottommost)
                        bottommost = v.Y;
                }

                for(int i = 0; i < vertices.Length; i++) {     // For every vertex:
                    vertices[i] -= (leftmost-1, bottommost-1); //     slide it to be aligned against the bottom left corner.
                }
            }

            void scaleOutline(ref Span<PointF> vertices, ref Span<(int a, int b)> edges) {
                float scale = (float)r.NextDouble() + r.Next(1, 3); // Choose a random float between 1 and 3.

                for(int i = 0; i < vertices.Length; i++) { // For every vertex,
                    vertices[i] *= scale;                  //     multiply it by the scale.
                }

                if(scale > 1.5f) {                             // If the island was scaled up a lot,
                    subdivideOutline(ref vertices, ref edges); //     subdivide it,
                    smoothOutline(vertices, edges);            //     and then smooth it.
                }
            }

            static void subdivideOutline(ref Span<PointF> vertices, ref Span<(int a, int b)> edges) {
                //
                // Make new arrays of vertices and edges.
                // We know how big they should be, as the subdivision algorithm is deterministic.
                var _vertices = new PointF[vertices.Length + edges.Length];
                for(int i = 0; i < vertices.Length; i++)
                    _vertices[i] = vertices[i];
                var _edges = new (int a, int b)[edges.Length * 2];
                //
                // For every edge:
                for(int i = 0; i < edges.Length; i++) {
                    ref var e = ref edges[i];
                    var v = (vertices[e.a] + vertices[e.b]) / 2; // Make a new vertex in the middle of the edge.
                    var vi = vertices.Length + i;                // Figure out the vertex's index.
                    _vertices[vi] = v;                           //
                                                                 //
                    _edges[i * 2    ] = (e.a, vi);               // Make an edge between the 1st vertex and the new one.
                    _edges[i * 2 + 1] = (vi, e.b);               // Make an edge between the 2nd vertex and the new one.
                }

                vertices = _vertices;
                edges = _edges;
            }

            void jitterOutline(Span<PointF> vertices) {
                float s = 0;                 // The extents of this island.
                foreach(var v in vertices) { // For every vertex:
                    s = Math.Max(s, v.X);    //     Update the extents if the x coord of the vertex is larger.
                    s = Math.Max(s, v.Y);    //     Update the extents if the y coord of the vertex is larger.
                }
                s = (s + 25) / 2;

                for(int i = 0; i < vertices.Length; i++) { // For every vertex:
                    var jitter = new PointF(j(), j());     // A vector; each component is between -1 & +1.
                    if(jitter.SqrMagnitude > 1)            // If the vector is larger than 1,
                        jitter = jitter.Normalized;        //     normalize it.
                    vertices[i] += jitter * s / 100;       // Offset the vertex by the vector,
                                                           //     multiplied by the island extents,
                                                           //     and divided by 100.
                }
                float j() => (float)r.NextDouble() * (r.Next(0, 10) < 5 ? 1 : -1); // Get a # between -1 and +1.
            }

            static void superOutline(Span<PointF> vertices) {
                const float scale = 4f;
                for(int i = 0; i < vertices.Length; i++) { // For every vertex:
                    vertices[i] *= scale;                  // Scale it up 4 times.
                }
            }

            static (Vertex[], Edge[]) compile(Span<PointF> vertices, Span<(int a, int b)> edges) {
                //
                // Project each edge into a specialized struct.
                var finalEdges = new Edge[edges.Length];
                for(int i = 0; i < finalEdges.Length; i++) {
                    var (a, b) = edges[i];
                    finalEdges[i] = new Edge(a, b);
                }
                //
                // Project each vertex into a specialized struct.
                var finalVertices = new Vertex[vertices.Length];
                for(int i = 0; i < finalVertices.Length; i++) { // For each vertex:
                    var (x, y) = vertices[i];                   // Store the vertex's position.
                    finalVertices[i] = new Vertex(x, y);        // Create a Vertex struct with the info.
                }

                return (finalVertices, finalEdges);
            }
        }

        static IslandBlock?[,] FindBlocks(Island island, Vertex[] vertices, Edge[] edges) {
            //
            // Find the extents of the island.
            int width = 0, height = 0;
            foreach(var v in vertices) {
                if(v.x > width)
                    width = (int)Math.Ceiling(v.x);
                if(v.y > height)
                    height = (int)Math.Ceiling(v.y);
            }

            var blocks = new IslandBlock?[width, height];
            var def = IslandBlockDef.Get("sand");
            for(int x = 0; x < width; x++) {
                for(int y = 0; y < height; y++) {
                    bool tr = IsCollidingPrecise(vertices, edges, (x + 1, y + 1)),
                         br = IsCollidingPrecise(vertices, edges, (x + 1, y)),
                         bl = IsCollidingPrecise(vertices, edges, (x, y)),
                         tl = IsCollidingPrecise(vertices, edges, (x, y + 1));
                    if(tr && br && bl && tl)
                        block(IslandBlockShape.Solid);
                    else if(tl && br && bl)
                        block(IslandBlockShape.TopRight);
                    else if(tl && tr && bl)
                        block(IslandBlockShape.BottomRight);
                    else if(tl && tr && br)
                        block(IslandBlockShape.BottomLeft);
                    else if(tr && br && bl)
                        block(IslandBlockShape.TopLeft);

                    void block(IslandBlockShape shape) => blocks[x, y] = new IslandBlock(island, def, x, y, shape);
                }
            }

            return blocks;
        }
        #endregion

        #region Collision
        private static bool IsCollidingPrecise(Vertex[] vertices, Edge[] edges, (int x, int y) p) {
            int cn = 0;
            foreach(var E in edges) {
                var a = vertices[E.a];
                var b = vertices[E.b];
                if(a.y <= p.y && b.y > p.y
                || a.y > p.y && b.y <= p.y) {
                    float vt = (float)(p.y - a.y) / (b.y - a.y);
                    if(p.x < a.x + vt * (b.x - a.x))
                        ++cn;
                }
            }
            return (cn & 1) == 1;
        }

        /// <summary>
        /// Checks if the specified point in <see cref="Ocean.Sea"/> space collides with this <see cref="Island"/>.
        /// </summary>
        public bool Collides(PointF seaPoint) {
            var local = Transformer.PointTo(seaPoint);
            var (indX, indY) = ((int)local.X, (int)local.Y);
            return indX >= 0 && indX < Width
                && indY >= 0 && indY < Height
                && this.blocks[indX, indY] != null;
        }

        /// <summary>
        /// Checks if the described <see cref="Ocean.Sea"/>-space line segment intersects with this <see cref="Island"/>.
        /// </summary>
        public bool Intersects(PointF start, PointF end) {
            var localStart = (PointI)Transformer.PointTo(in start);
            var localEnd = (PointI)Transformer.PointTo(in end);
            //
            // Use a pixel-by-pixel line drawing algorithm to draw a line.
            // If any pixel collides with an island block,
            // that means that the line intersects with this island.
            bool intersects = false;
            Bresenham.Line(localStart, localEnd, step);
            void step(int x, int y) {
                if(x >= 0 && y >= 0 && x < Width && y < Height && this.blocks[x, y] != null)
                    intersects = true;
            }
            return intersects;
        }

        /// <summary>
        /// Checks if the described <see cref="Ocean.Sea"/>-space lien segment intersects with this <see cref="Island"/>.
        /// If it does, outputs the squared distance from <paramref name="start"/> to the intersection.
        /// </summary>
        /// <param name="sqrDistance">
        /// The squared distance from <paramref name="start"/> to the intersection.
        /// To get the euclidean distance, take the square root of this.</param>
        public bool Intersects(PointF start, PointF end, out float sqrDistance) {
            var localStart = (PointI)Transformer.PointTo(in start);
            var localEnd = (PointI)Transformer.PointTo(in end);
            //
            // Use a pixel-by-pixel line drawing algorithm to draw a line.
            // If any pixel collides with an island block,
            // that measn that the line intersects with this island.
            bool intersects = false;
            var sqrDist = float.MaxValue;
            Bresenham.Line(localStart, localEnd, step);
            void step(int x, int y) {
                if(x > 0 && y >= 0 && x < Width && y < Height && this.blocks[x, y] != null) {
                    intersects = true;
                    //
                    // Measure the squared distance from the start to the intersection.
                    // If it's smaller than the last smallest distance,
                    // set it as the new smallest distance.
                    var dist = PointF.SqrDistance(in start, Transformer.PointFrom(x, y));
                    if(dist < sqrDist)
                        sqrDist = dist;
                }
            }

            sqrDistance = sqrDist;
            return intersects;
        }
        #endregion

        #region IAgentContainer Implementation
        IslandBlock? IAgentContainer<Island, IslandBlock>.GetSpotOrNull(int x, int y) => GetBlockOrNull(x, y);
        #endregion

        #region IGraph Implementation
        IEnumerable<IslandBlock> IGraph<IslandBlock>.Nodes {
            get {
                for(int x = 0; x < Width; x++) {
                    for(int y = 0; y < Height; y++) {
                        if(this.blocks[x, y] is IslandBlock b)
                            yield return b;
                    }
                }
            }
        }
        #endregion

        #region ISpaceLocus Implementation
        ISpaceLocus? ISpaceLocus.Parent => Sea;
        ISpace ISpaceLocus.Transformer => Transformer;
        ISpace<Island> ISpaceLocus<Island>.Transformer => Transformer;
        #endregion

        #region IUpdatable Implementation
        void IUpdatable.Update(in UpdateParams @params)
        {
            //
            // Update every Agent on the Island.
            foreach(var a in this.agents)
                (a as IUpdatable).Update(@params);
        }
        #endregion

        #region IDrawable Implementation
        void IDrawable<Sea>.Draw<TSeaDrawer>(in TSeaDrawer seaDrawer) {
            var drawer = new SpaceDrawer<Island, IslandTransformer, TSeaDrawer, Sea>(seaDrawer, Transformer);
            //
            // Draw each Block.
            for(int x = 0; x < Width; x++) {
                for(int y = 0; y < Height; y++) {
                    (this.blocks[x, y] as IDrawable<Island>)?.Draw(drawer);
                }
            }
            //
            // Draw each Village.
            (Village as IDrawable<Island>)?.Draw(drawer);
            //
            // Draw each Stock.
            for(int x = 0; x < Width; x++) {
                for(int y = 0; y < Height; y++) {
                    (this.blocks[x, y]?.Stock as IDrawable<Island>)?.Draw(drawer);
                }
            }
            //
            // Draw each Job.
            foreach(var j in this.jobs) {
                (j as IDrawable<Island>).Draw(drawer);
            }
            //
            // Draw each agent.
            foreach(var a in this.agents) {
                (a as IDrawable<Island>).Draw(drawer);
            }
        }
        #endregion

        #region IFocusableParent Implementation
        List<IFocusable> IFocusableParent.GetFocusable(PointF seaPoint)
        {
            var focusable = new List<IFocusable>();

            var (indX, indY) = Transformer.PointToIndex(seaPoint);
            if(TryGetAgent(indX, indY, out var agent)) {
                focusable.Add(agent);
            }
            if(TryGetStock(indX, indY, out var stock)) {
                focusable.Add(stock);
            }

            return focusable;
        }
        #endregion

        protected sealed override IslandBlock?[,] GetBlockGrid() => this.blocks;

        readonly struct Edge
        {
            /// <summary>
            /// The index of the corresponding vertex of this edge, within its own list.
            /// </summary>
            public readonly int a, b;

            public Edge(int a, int b) => (this.a, this.b) = (a, b);
        }

        readonly struct Vertex
        {
            /// <summary>
            /// The value of the corresponding coordinate of this <see cref="Vertex"/>.
            /// </summary>
            public readonly float x, y;

            public Vertex(float x, float y) => (this.x, this.y) = (x, y);
        }
    }

    public readonly struct IslandTransformer : ITransformer<Island>
    {
        bool ITransformer<Island>.HasRotation => false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        PointF ITransformer<Island>.PointTo(Island island, in PointF parent) => parent - (island.Left, island.Bottom);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        PointF ITransformer<Island>.PointFrom(Island island, in PointF local) => local + (island.Left, island.Bottom);

        Angle ITransformer<Island>.AngleTo(Island space, in Angle parent) => parent;
        Angle ITransformer<Island>.AngleFrom(Island space, in Angle local) => local;

        float ITransformer<Island>.ScaleTo(Island space, float parent) => parent;
        float ITransformer<Island>.ScaleFrom(Island space, float local) => local;
    }
}

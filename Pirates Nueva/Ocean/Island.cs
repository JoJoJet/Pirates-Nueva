using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Pirates_Nueva.Ocean
{
    public sealed class Island : ISpaceLocus<Island>, IDrawable<Sea>
    {
        private readonly IslandBlock?[,] blocks;

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
        }

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

        static (Vertex[], Edge[]) FindOutline(bool[,] shape, Random r) {
            int width = shape.GetLength(0), height = shape.GetLength(1);

            var vertices = new List<PointF>();
            var edges = new List<(int a, int b)>();

            findOutline();   // Generate an outline surrounding the island.
            smoothOutline(); // Smooth the outline.
            scaleOutline();  // Scale up the islands by a random amount.
            jitterOutline(); // Roughen up the outline.
            superOutline();  // Scale the island up by four.
            alignOutline();  // Align the outline to the bottom left of this island.
            return compile();
            
            /*
             * Local Methods.
             */
            void findOutline() {
                //
                // Run the Marching Squares algorithm on the pixels.
                // https://en.wikipedia.org/wiki/Marching_squares.
                //
                for(int x = -1; x < width; x++) {
                    for(int y = -1; y < height; y++) {
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
                                addEdge((0, Half), (Half, 0), Ro2, Ro2);
                                break;
                            // 0 0 //
                            // 0 1 //
                            case 2:
                                addEdge((Half, 0), (Whole, Half), -Ro2, Ro2);
                                break;
                            // 0 0 //
                            // 1 1 //
                            case 3:
                                addEdge((0, Half), (Whole, Half), 0, 1);
                                break;
                            // 0 1 //
                            // 0 0 //
                            case 4:
                                addEdge((Half, Whole), (Whole, Half), -Ro2, -Ro2);
                                break;
                            // 0 1 //
                            // 1 0 //
                            case 5:
                                addEdge((0, Half), (Half, Whole), -Ro2, Ro2);
                                addEdge((Half, 0), (Whole, Half), Ro2, -Ro2);
                                break;
                            // 0 1 //
                            // 0 1 //
                            case 6:
                                addEdge((Half, 0), (Half, Whole), -1, 0);
                                break;
                            // 0 1 //
                            // 1 1 //
                            case 7:
                                addEdge((0, Half), (Half, Whole), -Ro2, Ro2);
                                break;
                            // 1 0 //
                            // 0 0 //
                            case 8:
                                addEdge((0, Half), (Half, Whole), Ro2, -Ro2);
                                break;
                            // 1 0 //
                            // 1 0 //
                            case 9:
                                addEdge((Half, 0), (Half, Whole), 1, 0);
                                break;
                            // 1 0 //
                            // 0 1 //
                            case 10:
                                addEdge((0, Half), (Half, 0), -Ro2, -Ro2);
                                addEdge((Half, Whole), (Whole, Half), Ro2, Ro2);
                                break;
                            // 1 0 //
                            // 1 1 //
                            case 11:
                                addEdge((Half, Whole), (Whole, Half), Ro2, Ro2);
                                break;
                            // 1 1 //
                            // 0 0 //
                            case 12:
                                addEdge((0, Half), (Whole, Half), 0, -1);
                                break;
                            // 1 1 //
                            // 1 0 //
                            case 13:
                                addEdge((Half, 0), (Whole, Half), Ro2, -Ro2);
                                break;
                            // 1 1 //
                            // 0 1 //
                            case 14:
                                addEdge((0, Half), (Half, 0), -Ro2, -Ro2);
                                break;
                            // 1 1 //
                            // 1 1 //
                            case 15:

                                break;
                        }

                        void addEdge(PointF alpha, PointF beta, float normalX, float normalY) {
                            var indices = (makeIndex((x, y) + alpha), makeIndex((x, y) + beta));
                            edges.Add(indices);

                            int makeIndex(PointF v) {
                                var i = vertices.IndexOf(v);   // Get the index of the specified vertex.
                                if(i >= 0) {                   // If the index exists,
                                    return i;                  //     return it.
                                }                              //
                                else {                         // If the index does NOT exist,
                                    vertices.Add(v);           //     add the specified vertex,
                                    return vertices.Count - 1; //     and return its index.
                                }
                            }
                        }

                        int b(int g, int h) => g >= 0 && g < width && h >= 0 && h < height && shape[g, h] ? 1 : 0;
                    }
                }
            }

            void smoothOutline() {
                Span<PointF> verts = stackalloc PointF[vertices.Count];

                //
                // Apply Laplacian smoothing to the outline.
                // https://en.wikipedia.org/wiki/Laplacian_smoothing.
                //
                for(int i = 0; i < vertices.Count; i++) {       // For every vertex in this island's outline:
                    var neighbors = from n in edges             // Get each neighbor of the vertex.
                                    where n.a == i || n.b == i  //
                                    select vertices[            //
                                        n.a == i ? n.b : n.a    //
                                        ];                      //
                                                                //
                    var xi = PointF.Zero;                       // The smoothed position of the vertex.
                    int count = 0;                              // How many neighbors it has.
                    foreach(var n in neighbors) {               // For every neighbor:
                        xi += n;                                //     Add its position to the smoothed position,
                        count++;                                //     and increment the neighbor count.
                    }                                           //
                    xi /= count;                                // Divide the smoothed position by the number of neighbors.
                                                                //
                    verts[i] = PointF.Lerp(                     // Set the new position of the vertex
                        vertices[i], xi, 0.5f                   //     as the midpoint between the smoothed
                        );                                      //     position & its original position.
                }

                for(int i = 0; i < verts.Length; i++) {
                    vertices[i] = verts[i];
                }
            }

            void alignOutline() {
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

                for(int i = 0; i < vertices.Count; i++) {      // For every vertex:
                    vertices[i] -= (leftmost-1, bottommost-1); //     slide it to be aligned against the bottom left corner.
                }
            }

            void scaleOutline() {
                float scale = (float)r.NextDouble() + r.Next(1, 3); // Choose a random float between 1 and 3.

                for(int i = 0; i < vertices.Count; i++) { // For every vertex,
                    vertices[i] *= scale;                 //     multiply it by the scale.
                }

                if(scale > 1.5f) {      // If the island was scaled up a lot,
                    subdivideOutline(); //     subdivide it,
                    smoothOutline();    //     and then smooth it.
                }
            }

            void subdivideOutline() {
                var _vertices = new List<PointF>(vertices);
                var _edges = new List<(int a, int b)>(edges.Count * 2);
                foreach(var e in edges) {                        // For every edge:
                    var v = (vertices[e.a] + vertices[e.b]) / 2; //
                                                                 //
                    _vertices.Add(v);                            // Add that vertex to the list of vertices.
                    var i = _vertices.Count - 1;                 //
                                                                 //
                    _edges.Add((e.a, i));                        // Make an edge between the 1st vertex and the new one.
                    _edges.Add((i, e.b));                        // Make an edge between the 2nd vertex and the new one.
                }

                vertices = _vertices;
                edges = _edges;
            }

            void jitterOutline() {
                float s = 0;                 // The extents of this island.
                foreach(var v in vertices) { // For every vertex:
                    s = Math.Max(s, v.X);    //     Update the extents if the x coord of the vertex is larger.
                    s = Math.Max(s, v.X);    //     Update the extents if the y coord of the vertex is larger.
                }
                s = (s + 25) / 2;

                for(int i = 0; i < vertices.Count; i++) {  // For every vertex:
                    var jitter = new PointF(j(), j());     // A vector; each component is between -1 & +1.
                    if(jitter.SqrMagnitude > 1)            // If the vector is larger than 1,
                        jitter = jitter.Normalized;        //     normalize it.
                    vertices[i] += jitter * s / 100;       // Offset the vertex by the vector,
                                                           //     multiplied by the island extents,
                                                           //     and divided by 100.
                }
                float j() => (float)r.NextDouble() * (r.Next(0, 10) < 5 ? 1 : -1); // Get a # between -1 and +1.
            }

            void superOutline() {
                const float scale = 4f;
                for(int i = 0; i < vertices.Count; i++) { // For every vertex:
                    vertices[i] *= scale;                 // Scale it up 4 times.
                }
            }

            (Vertex[], Edge[]) compile() {
                //
                // Project each edge into a specialized struct.
                var finalEdges = new Edge[edges.Count];
                for(int i = 0; i < finalEdges.Length; i++) {
                    var (a, b) = edges[i];
                    finalEdges[i] = new Edge(a, b);
                }
                //
                // Project each vertex into a specialized struct.
                var finalVertices = new Vertex[vertices.Count];
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

        private static bool IsCollidingPrecise(Vertex[] vertices, Edge[] edges, PointF p) {
            int cn = 0;
            foreach(var E in edges) {
                var a = vertices[E.a];
                var b = vertices[E.b];
                if(a.y <= p.Y && b.y > p.Y
                || a.y > p.Y && b.y <= p.Y) {
                    float vt = (float)(p.Y - a.y) / (b.y - a.y);
                    if(p.X < a.x + vt * (b.x - a.x))
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
        
        #region ISpaceLocus Implementation
        ISpaceLocus? ISpaceLocus.Parent => Sea;
        ISpace ISpaceLocus.Transformer => Transformer;
        ISpace<Island> ISpaceLocus<Island>.Transformer => Transformer;
        #endregion

        #region IDrawable Implementation
        void IDrawable<Sea>.Draw(ILocalDrawer<Sea> drawer) {
            //
            // Draw the blocks.
            if(this.blocks is null)
                return;
            var localDrawer = new SpaceDrawer<Island, IslandTransformer, Sea>(drawer, Transformer);
            foreach(var block in this.blocks) {
                (block as IDrawable<Island>)?.Draw(localDrawer);
            }
        }
        #endregion

        readonly struct Edge
        {
            /// <summary>
            /// The index of the corresponding vertex of this edge, within its own list.
            /// </summary>
            public readonly int a, b;

            public Edge(int a, int b) {
                this.a = a; this.b = b;
            }
        }

        readonly struct Vertex
        {
            /// <summary>
            /// The value of the corresponding coordinate of this <see cref="Vertex"/>.
            /// </summary>
            public readonly float x, y;

            public Vertex(float x, float y) {
                this.x = x; this.y = y;
            }
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

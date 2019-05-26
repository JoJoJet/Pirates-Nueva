using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pirates_Nueva.Ocean
{
    public class Island : IDrawable
    {
        /// <summary> The outline of this island. </summary>
        private Vertex[] vertices;
        /// <summary> Each element contains the indices of two connected vertices. </summary>
        private Edge[] edges;

        private UI.Texture tex;

        public Sea Sea { get; }

        /// <summary> The left edge of this <see cref="Island"/>, in <see cref="Ocean.Sea"/>-space. </summary>
        public float Left { get; }
        /// <summary> The bottom edge of this <see cref="Island"/>, in <see cref="Ocean.Sea"/>-space. </summary>
        public float Bottom { get; }

        public Island(Sea sea, int left, int bottom) {
            Sea = sea;
            Left = left;
            Bottom = bottom;
        }

        /// <summary> The number of texture pixels per whole number. </summary>
        const int PPU = 16;

        const int _width = 15, _height = 15;
        public async Task GenerateAsync(int seed, Master master) {
            Random r = new Random(seed);

            var shape = await generateShape(r);

            await Task.Run(() => FindOutline(shape, r));

            tex = await Task.Run(() => CreateTexture(master, this.vertices, this.edges));
        }

        static async Task<bool[,]> generateShape(Random r) {
            const int Width = _width, Height = _height;

            var ground = new bool[Width, Height];

            await Task.Run(() => placeBlobs());    // Scatter shapes around the canvas.
            await Task.Run(() => connectEdges());  // Connect separated but close blocks.
            await floodFill();                     // Fill in the entire terrain.

            await Task.Run(() => decimate());      // Randomly kill 20% of the blocks.
            await floodFill();                     // Fill in the terrain.

            await Task.Run(() => breakNecks());    // Break any thin connectors.
            await slideSeperates();                // Combine any stray islands into one shape.

            await Task.Run(() => connectEdges());  // Connnect separated but close blocks.
            await floodFill();                     // Fill in the terrain.

            await Task.Run(() => extraneous());    // Delete unnatural extrusions.
            await Task.Run(() => breakNecks());    // Break any thin connectors.
            await slideSeperates();                // Combine any stray shapes into one.

            await Task.Run(() => connectEdges());  // Connect separated but close blocks.
            await floodFill();                     // Fill in the terrain.

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

            async Task floodFill() {
                var fill = new bool[Width, Height];   // An empty 2D array.
                for(int x = 0; x < Width; x++) {      // For every point in the array:
                    for(int y = 0; y < Height; y++) { //
                        fill[x, y] = true;            // Default its to be filled in.
                    }
                }
                await Task.Run(() => doFloodFill(ground, (0, Height-1), (x, y) => fill[x, y] = false));
                ground = fill;

                void doFloodFill(bool[,] canvas, PointI start, Action<int, int> paint) {
                    var box = new BoundingBox(0, 0, Width-1, Height-1);

                    var frontier = new Stack<PointI>(new[] { start }); // List of pixels to be searched.
                    var known = new List<PointI>();                    // Pixels that have already been searched.

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
                        if(box.Contains(x, y) && !ground[x, y] && !known.Contains((x, y))) // If the point is empty & not yet searched,
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

            async Task slideSeperates() {
                var seperates = await Task.Run(() => findSeperates()); // Find the separated chunks of the island.
                while(seperates.Count > 1) {                           // Loop until there is only one island:
                    ground = new bool[Width, Height];                  // Clear the ground pixels,
                    foreach(var p in seperates.Skip(1).Union())        //     and populate them with
                        ground[p.X, p.Y] = true;                       //         the seperate chunks, except the first one.
                                                                       //
                    await Task.Run(() => doSlide(seperates.First()));  // Slide the first separated chunk into the mainland.
                    seperates = await Task.Run(() => findSeperates()); // Re-compute the seperated chunks of the island.
                }

                List<List<PointI>> findSeperates() {
                    var fragments = new List<PointI>();   // A list of points corresponding to ground pixels.
                    for(int x = 0; x < Width; x++) {      // For every point in the island:
                        for(int y = 0; y < Height; y++) { //
                            if(ground[x, y])              // If there is a ground pixel there,
                                fragments.Add((x, y));    //     add the point to the list.
                        }
                    }

                    var chunks = new List<List<PointI>>();
                    while(fragments.Count > 0) {
                        var frontier = new Queue<PointI>(new[] { fragments.Last() }); // Coordinates to be searched.
                        var known = new List<PointI>();                               // Coordinates that have already been searched.

                        while(frontier.Count > 0) {          // While there are coordinates to be searched:
                            var (x, y) = frontier.Dequeue(); // Get a coordinate to search, /x/, /y/.
                            fragments.Remove((x, y));        // Remove it from the list of loose fragments,
                            known.Add((x, y));               // and add it to the list of searched coordinates.
                                                             //
                            peripheral(x - 1, y);            // Mark its left adjacent neighbor to be searched, if it wansn't already.
                            peripheral(x, y + 1);            // Do the same for its upward neighbor,
                            peripheral(x + 1, y);            // its rightward neighbor,
                            peripheral(x, y - 1);            // and its downward neighbor.
                        }

                        chunks.Add(known);

                        void peripheral(int x, int y) {
                            if(fragments.Contains((x, y))) // If the specified point is still loose,
                                frontier.Enqueue((x, y));  //     mark it to be searched later on.
                        }
                    }

                    return (from s in chunks
                            orderby s.Count ascending
                            select s).ToList();
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
                        var ed = new PointI?[Math.Max(Width, Height)];      // An array of edge blocks.
                        foreach(var p in isolated) {                        // For every block in the chunk:
                            int i = indexer(p);                             //
                            if(ed[i] == null || isFurther(ed[i].Value, p))  // If it is the closest block to the edge in this axis,
                                ed[i] = p;                                  //     add it to the array of edges.
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

        void FindOutline(bool[,] shape, Random r) {
            const int Width = _width, Height = _height;

            var vertices = new List<PointF>();
            var edges = new List<(int a, int b)>();

            findOutline();   // Generate an outline surrounding the island.
            smoothOutline(); // Smooth the outline.
            scaleOutline();  // Scale up the islands by a random amount.
            jitterOutline(); // Roughen up the outline.
            superOutline();  // Scale the island up by four.
            alignOutline();  // Align the outline to the bottom left of this island.
            compile();
            
            /*
             * Local Methods.
             */
            void findOutline() {
                //
                // Run the Marching Squares algorithm on the pixels.
                // https://en.wikipedia.org/wiki/Marching_squares.
                //
                for(int x = -1; x < Width; x++) {
                    for(int y = -1; y < Height; y++) {
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

                        int b(int g, int h) => g >= 0 && g < Width && h >= 0 && h < Height && shape[g, h] ? 1 : 0;
                    }
                }
            }

            void smoothOutline() {
                PointF[] verts = new PointF[vertices.Count];

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
                    int length = 0;                             // How many neighbors it has.
                    foreach(var n in neighbors) {               // For every neighbor:
                        xi += n;                                //     Add its position to the smoothed position,
                        length++;                               //     and increment the neighbor count.
                    }                                           //
                    xi /= length;                               // Divide the smoothed position by the number of neighbors.
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
                float leftmost = float.MaxValue;   // The furthest left position of any vertex.
                float bottommost = float.MaxValue; // The lowest position of any vertex.
                foreach(var v in vertices) {                // For every vertex:
                    leftmost =   Math.Min(leftmost, v.X);   //     Set its x coord as the leftmost value if it's smaller than the current.
                    bottommost = Math.Min(bottommost, v.Y); //     Set its y coord as the bottommost value if it's smaller than the current.
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

            void compile() {
                this.vertices = vertices.Select(v => new Vertex() { x = v.X, y = v.Y }).ToArray();
                this.edges = edges.Select(e => new Edge() { a = e.a, b = e.b }).ToArray();

                // Calculate normals for the edges.
                for(int i = 0; i < this.edges.Length; i++) { // For every edge:
                    var a = this.vertices[edges[i].a];       // The first vertex of this edge.
                    var b = this.vertices[edges[i].b];       // The second vertex of this edge.
                                                             //
                    var center = (a.Pos + b.Pos) / 2;        // The midpoint of both vertices.
                                                             //
                    var dx = b.x - a.x;                      // Slope, rise.
                    var dy = b.y - a.y;                      // Slope, run.
                                                             //
                    var v1 = new PointF(-dy, dx).Normalized; // A vector perpendicular to this edge.
                                                             //
                    if(IsCollidingPrecise(center + v1/10f))  // If the vector points inwards,
                        this.edges[i].normal = -v1;          //     set the edge's normal as the opposite of that vector.
                    else                                     // If the vector points outwards,
                        this.edges[i].normal = v1;           //     set the edge's normal as that vector.
                }

                // Calculate normals for the vertices.
                for(int i = 0; i < this.vertices.Length; i++) { // For each vertex:
                    var es = from n in this.edges               // Find the edges connecting to the vertex.
                             where n.a == i || n.b == i         //
                             select n;                          //
                    var a = es.First();                         // The first edge connected to this vertex.
                    var b = es.Last();                          // The second edge connected to this vertex.
                                                                //
                    this.vertices[i].normal =                   // Set the vertex's normal as
                        ((a.normal + b.normal) / 2).Normalized; //     the medium of each edge's normal.
                }
            }
        }

        static UI.Texture CreateTexture(Master master, Vertex[] vertices, Edge[] edges) {

            (int w, int h) = findExtents();

            var pixels = new UI.Color[w * h]; // An array of colors

            scanlineFill(vertices, edges, UI.Color.DarkLime);

            drawShore();

            return master.Renderer.CreateTexture(w, h, pixels); // Create a texture using the array of colors we just made.

            /*
             * Local Methods
             */
            void paint(int x, int y, UI.Color color) => pixels[(h - y - 1) * w + x] = color;
            UI.Color get(int x, int y) => pixels[(h - y - 1) * w + x];

            (int, int) findExtents() {
                float rightmost = 0; // The rightmost edge of this island.
                float topmost = 0;   // The topmost edge of this island.
                foreach(var v in vertices) {              // For every vertex:
                    rightmost = Math.Max(rightmost, v.x); // Update the rightmost extent if the vertex if further right.
                    topmost = Math.Max(topmost, v.y);     // Update the topmost extent if the vertex if further up.
                }

                var wi = (int)Math.Floor((rightmost + 1) * PPU); // Width of the texture.
                var he = (int)Math.Floor((topmost + 1) * PPU); // Height of the texture.
                return (wi, he);
            }

            void scanlineFill(IList<Vertex> _verts, IEnumerable<Edge> _edges, UI.Color fillColor) {
                //
                // Fills the island using the scanline algorithm described here:
                // https://www.tutorialspoint.com/computer_graphics/polygon_filling_algorithm.htm
                //
                for(int y = 0; y < h; y++) {
                    var iss = getIntersections(new PointF(0, (float)y / PPU));
                    for(int i = 0; i < iss.Count - 1; i += 2) {
                        var a = iss[i] * PPU;
                        var b = iss[i + 1] * PPU;
                        for(int x = (int)a.X; x <= b.X; x++)
                            paint(x, y, fillColor);
                    }
                }

                List<PointF> getIntersections(PointF rayOrigin) {
                    return getInters().OrderBy(p => p.X).ToList();

                    IEnumerable<PointF> getInters() {
                        PointF end = rayOrigin + new PointF(w + 100, 0);
                        (float x, float y) i;
                        foreach(var e in _edges) {
                            if(GetLineIntersection(rayOrigin, end, _verts[e.a].Pos, _verts[e.b].Pos, out i))
                                yield return i;
                        }
                    }
                }
            }

            void drawShore() {

                var verts = new List<Vertex>(vertices.Length * 2);
                    verts.AddRange(vertices);
                var edgs = new List<Edge>(edges.Length * 2);
                    edgs.AddRange(edges);

                foreach(var v in vertices) {
                    verts.Add(new Vertex(v.x - v.normal.X*3, v.y - v.normal.Y*3, v.normal));
                }
                foreach(var e in edges) {
                    edgs.Add(new Edge(e.a + vertices.Length, e.b + vertices.Length, e.normal));
                }

                scanlineFill(verts, edgs, UI.Color.PaleYellow);
            }
        }

        private bool IsCollidingPrecise(PointF p) {
            int cn = 0;
            foreach(var E in this.edges) {
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
        
        static bool GetLineIntersection(PointF a, PointF b, PointF c, PointF d, out (float x, float y) i) {
            return get_line_intersection(a.X, a.Y, b.X, b.Y, c.X, c.Y, d.X, d.Y, out i.x, out i.y) == '1';
        }
        // Returns 1 if the lines intersect, otherwise 0. In addition, if the lines 
        // intersect the intersection point may be stored in the floats i_x and i_y.
        static char get_line_intersection(float p0_x, float p0_y, float p1_x, float p1_y,
            float p2_x, float p2_y, float p3_x, float p3_y, out float i_x, out float i_y) {
            float s1_x, s1_y, s2_x, s2_y;
            s1_x = p1_x - p0_x; s1_y = p1_y - p0_y;
            s2_x = p3_x - p2_x; s2_y = p3_y - p2_y;

            float s, t;
            s = (-s1_y * (p0_x - p2_x) + s1_x * (p0_y - p2_y)) / (-s2_x * s1_y + s1_x * s2_y);
            t = (s2_x * (p0_y - p2_y) - s2_y * (p0_x - p2_x)) / (-s2_x * s1_y + s1_x * s2_y);

            if(s >= 0 && s <= 1 && t >= 0 && t <= 1) {
                // Collision detected
                i_x = p0_x + (t * s1_x);
                i_y = p0_y + (t * s1_y);
                return '1';
            }
            else {
                i_x = i_y = 0;
                return '0'; // No collision
            }
        }

        #region IDrawable Implementation
        void IDrawable.Draw(Master master) {
            //drawOutline();

            if(tex == null)
                return;
            var (sx, sy) = Sea.SeaPointToScreen(Left, Bottom);
            var (w, h) = (PointI)(tex.Width * Sea.PPU, tex.Height * Sea.PPU) / PPU;
            master.Renderer.Draw(this.tex, sx, sy - h, w, h);

            void drawOutline() {
                //
                // Return early if the edges have not generated yet.
                if(this.edges == null)
                    return;

                foreach(var l in this.edges) {
                    //
                    // Draw each edge.
                    var a = Sea.SeaPointToScreen((Left, Bottom) + vertices[l.a].Pos);
                    var b = Sea.SeaPointToScreen((Left, Bottom) + vertices[l.b].Pos);
                    master.Renderer.DrawLine(a, b);
                    //
                    // Draw the normal of each EDGE.
                    var lCenter = (Left, Bottom) + (vertices[l.a].Pos + vertices[l.b].Pos) / 2;
                    var lEnd = lCenter + l.normal;
                    master.Renderer.DrawLine(Sea.SeaPointToScreen(lCenter), Sea.SeaPointToScreen(lEnd));
                }
                //
                // Draw the normal of each VERTEX.
                foreach(var v in this.vertices) {
                    var vCenter = (Left, Bottom) + v.Pos;
                    var nEnd = vCenter + v.normal;
                    master.Renderer.DrawLine(Sea.SeaPointToScreen(vCenter), Sea.SeaPointToScreen(nEnd), UI.Color.Black);
                }
            }
        }
        #endregion

        struct Edge
        {
            /// <summary>
            /// The index of the corresponding vertex of this edge, within its own list.
            /// </summary>
            public int a, b;
            public PointF normal;

            public Edge(int a, int b, PointF normal) => this = new Edge() { a = a, b = b, normal = normal };
        }

        struct Vertex
        {
            /// <summary>
            /// The value of the corresponding coordinate of this <see cref="Vertex"/>.
            /// </summary>
            public float x, y;
            public PointF normal;

            public PointF Pos {
                get => new PointF(x, y);
                set => (x, y) = value;
            }

            public Vertex(float x, float y, PointF normal) => this = new Vertex() { x = x, y = y, normal = normal };
        }
    }
}

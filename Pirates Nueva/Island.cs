using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pirates_Nueva
{
    public class Island : IDrawable
    {
        private bool[,] ground;
        private List<List<PointI>> seperates;

        public Island() { }

        public async Task Generate(int seed, Master master) {
            Random r = new Random(seed);

            const int Width = 15;
            const int Height = 15;
            ground = new bool[Width, Height];

            // Scatter shapes around the canvas.
            await placeBlobs();
            await waitForClick();
            
            // Connect separated but close blocks.
            await Task.Run(() => connectEdges());
            await waitForClick();

            // Fill in the entire terrain.
            await floodFill();
            await waitForClick();

            // Randomly kill 20% of the blocks.
            await Task.Run(() => decimate());
            await waitForClick();

            // Fill in the terrain.
            await floodFill();
            await waitForClick();

            await slideSeperates();

            async Task waitForClick() {
                await Task.Run(() => doWait());
                void doWait() {
                    while(!master.Input.MouseLeft.IsDown) ;
                    System.Threading.Thread.Sleep(100);
                }
            }

            async Task placeBlobs() {
                const int Radius = 3;
                PointI[] shape = {
                                   (0,  2),
                         (-1,  1), (0,  1), (1,  1),
                (-2, 0), (-1,  0), (0,  0), (1,  0), (2, 0),
                         (-1, -1), (0, -1), (1, -1),
                                   (0, -2)
                };

                const int BlobCount = 6;
                for(int i = 0; i < BlobCount; i++) {
                    await waitForClick();
                    var (x, y) = (r.Next(Radius, Width-Radius), r.Next(Radius, Height-Radius));
                    foreach(var s in shape)
                        ground[x+s.X, y+s.Y] = true;
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
                var fill = new bool[Width, Height];
                for(int x = 0; x < Width; x++) {
                    for(int y = 0; y < Height; y++) {
                        fill[x, y] = true;
                    }
                }
                await Task.Run(() => doFloodFill(ground, (0, Height-1), (x, y) => fill[x, y] = false));
                ground = fill;

                void doFloodFill(bool[,] canvas, PointI start, Action<int, int> paint) {
                    var box = new BoundingBox(0, 0, Width-1, Height-1);

                    var frontier = new List<PointI>() { start };
                    var known = new List<PointI>();

                    while(frontier.Count > 0) {
                        var (x, y) = frontier[frontier.Count - 1];
                        frontier.RemoveAt(frontier.Count - 1);
                        known.Add((x, y));

                        paint(x, y);

                        neighbor(x - 1, y);
                        neighbor(x, y + 1);
                        neighbor(x + 1, y);
                        neighbor(x, y - 1);
                    }
                    void neighbor(int x, int y) {
                        if(box.Contains(x, y) && !ground[x, y] && !known.Contains((x, y)))
                            frontier.Add((x, y));
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

            void findSeperates() {
                var fragments = new List<PointI>();   // A list of points corresponding to ground pixels.
                for(int x = 0; x < Width; x++) {      // For every point in the island:
                    for(int y = 0; y < Height; y++) { //
                        if(ground[x, y])              // If there is a ground pixel there,
                            fragments.Add((x, y));    //     add the point to the list.
                    }
                }

                var seperates = new List<List<PointI>>();
                while(fragments.Count > 0) {
                    var frontier = new Queue<PointI>(new[] { fragments.Last() }); // Coordinates to be searched.
                    var known = new List<PointI>();                               // Coordinates that have already been searched.

                    while(frontier.Count > 0) {          // While there are coordinates to be searched:
                        var (x, y) = frontier.Dequeue(); // Get a coordinate to search, /x/, /y/.
                        fragments.Remove((x, y));        // Remove it from the list of loose fragments,
                        known.Add((x, y));               // and add it to the list of searched coordinates.

                        peripheral(x - 1, y);            // Mark its left adjacent neighbor to be searched, if it wansn't already.
                        peripheral(x, y + 1);            // Do the same for its upward neighbor,
                        peripheral(x + 1, y);            // its rightward neighbor,
                        peripheral(x, y - 1);            // and its downward neighbor.
                    }
                    void peripheral(int x, int y) {
                        if(fragments.Contains((x, y))) // If the specified point is still loose,
                            frontier.Enqueue((x, y));  //     mark it to be searched later on.
                    }

                    seperates.Add(known);
                }

                this.seperates = (from s in seperates
                                  orderby s.Count descending
                                  select s).ToList();
            }

            async Task slideSeperates() {
                await Task.Run(() => findSeperates()); // Find the separated chunks of the island.
                while(seperates.Count > 1) {
                    int c = seperates.Count;

                    ground = new bool[Width, Height];              // Clear the ground pixels,
                    foreach(var p in seperates.Take(c-1).Union())  //     and populate them with
                        ground[p.X, p.Y] = true;                   //         the seperate chunks, except the final one.

                    await Task.Run(() => doSlide(seperates[c-1])); // Slide the last separated chunk into the mainland.
                    await Task.Run(() => findSeperates());         // Re-compute the seperated chunks of the island.
                    
                    await waitForClick();                          // Wait for the user to click.
                }

                void doSlide(List<PointI> isolated) {
                    PointI? slide = null; // The direction and amount of sliding.
                    
                    trySlide( // Try to slide leftward.
                        findEdge((p) => p.Y, (p1, p2) => p1.X > p2.X),
                        (-1, 0)
                        );

                    trySlide( // Try to slide downward.
                        findEdge((p) => p.X, (p1, p2) => p1.Y > p2.Y),
                        (0, -1)
                        );

                    trySlide( // Try to slide rightward.
                        findEdge((p) => p.Y, (p1, p2) => p1.X < p2.X),
                        (1, 0)
                        );

                    trySlide( // Try to slide upward.
                        findEdge((p) => p.X, (p1, p2) => p1.Y < p2.Y),
                        (0, 1)
                        );
                    
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

                    void trySlide(IEnumerable<PointI> ed, PointI dir) {
                        var bounds = new BoundingBox(0, 0, Width - 1, Height - 1);
                        foreach(var e in ed) {
                            for(int i = 0; bounds.Contains(e + dir * i); i++) {
                                if(hasAdjacent(e.X + dir.X * i, e.Y + dir.Y * i)) {
                                    if((slide?.SqrMagnitude > i*i || slide == null) && checkBounds(dir * i))
                                        slide = dir * i;
                                    break;
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
        }

        void IDrawable.Draw(Master master) {
            const int Pixels = 32;

            var tex = master.Resources.LoadTexture("woodBlock");

            for(int x = 0; x < ground.GetLength(0); x++) {
                for(int y = 0; y < ground.GetLength(1); y++) {
                    if(ground[x, y])
                        master.Renderer.Draw(tex, x * Pixels, master.GUI.ScreenHeight - y * Pixels, Pixels, Pixels);
                }
            }
        }
    }
}

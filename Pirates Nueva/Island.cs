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
        private List<PointI>[] seperates;

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

            await floodFill();
            await waitForClick();

            await Task.Run(() => findSeperates());

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
                    var w = canvas.GetLength(0);
                    var h = canvas.GetLength(1);

                    var frontier = new List<PointI>() { start };
                    var known = new List<PointI>();

                    while(frontier.Count > 0) {
                        var (x, y) = frontier[frontier.Count - 1];
                        frontier.RemoveAt(frontier.Count - 1);
                        known.Add((x, y));

                        paint(x, y);

                        if(x > 0 && canvas[x - 1, y] == false && !known.Contains((x - 1, y)))
                            frontier.Add((x - 1, y));
                        if(y < h - 1 && canvas[x, y + 1] == false && !known.Contains((x, y + 1)))
                            frontier.Add((x, y + 1));
                        if(x < w - 1 && canvas[x + 1, y] == false && !known.Contains((x + 1, y)))
                            frontier.Add((x + 1, y));
                        if(y > 0 && canvas[x, y - 1] == false && !known.Contains((x, y - 1)))
                            frontier.Add((x, y - 1));
                    }
                }
            }

            void decimate() {
                for(int x = 0; x < Width; x++) {
                    for(int y = 0; y < Height; y++) {
                        if(ground[x, y] && r.Next(0, 100) < 20)
                            ground[x, y] = false;
                    }
                }
            }

            void findSeperates() {
                var fragments = new List<PointI>();
                for(int x = 0; x < Width; x++) {
                    for(int y = 0; y < Height; y++) {
                        if(ground[x, y])
                            fragments.Add((x, y));
                    }
                }

                var seperates = new List<List<PointI>>();
                while(fragments.Count > 0) {
                    var frontier = new List<PointI>() { fragments[fragments.Count - 1] };
                    var known = new List<PointI>();

                    while(frontier.Count > 0) {
                        var (x, y) = frontier[frontier.Count - 1];
                        frontier.RemoveAt(frontier.Count - 1);
                        fragments.Remove((x, y));
                        known.Add((x, y));

                        peripheral(x - 1, y);
                        peripheral(x, y + 1);
                        peripheral(x + 1, y);
                        peripheral(x, y - 1);

                        void peripheral(int px, int py) {
                            if(fragments.Contains((px, py)))
                                frontier.Add((px, py));
                        }
                    }

                    seperates.Add(known);
                }

                this.seperates = (from s in seperates
                                  orderby s.Count descending
                                  select s).ToArray();
            }
        }

        void IDrawable.Draw(Master master) {
            const int Pixels = 32;

            var tex = master.Resources.LoadTexture("woodBlock");

            /*
             * If isolated chunks have seperated, display them.
             */
            if(this.seperates != null) {
                foreach(var s in this.seperates) {
                    var r = new Random(s.GetHashCode());
                    var color = new Microsoft.Xna.Framework.Color(r.Next(0, 256), r.Next(0, 256), r.Next(0, 256));

                    foreach(var p in s) {
                        master.Renderer.Draw(tex, p.X * Pixels, master.GUI.ScreenHeight - p.Y * Pixels, Pixels, Pixels, color);
                    }
                }
            }
            /*
             * If isolated chunks have NOT been separated, display the whole ground.
             */
            else {
                for(int x = 0; x < ground.GetLength(0); x++) {
                    for(int y = 0; y < ground.GetLength(1); y++) {
                        if(ground[x, y])
                            master.Renderer.Draw(tex, x * Pixels, master.GUI.ScreenHeight - y * Pixels, Pixels, Pixels);
                    }
                }
            }
        }
    }
}

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

        public Island() { }

        public void Generate(int seed, Master master) {
            Random r = new Random(seed);

            const int Width = 15;
            const int Height = 15;
            ground = new bool[Width, Height];

            // Scatter shapes around the canvas.
            placeBlobs();

            void placeBlobs() {
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
                    pause();
                    var (x, y) = (r.Next(Radius, Width-Radius), r.Next(Radius, Height-Radius));
                    foreach(var s in shape)
                        ground[x+s.X, y+s.Y] = true;
                }
            }
        }

        void IDrawable.Draw(Master master) {
            const int Pixels = 32;

            var tex = master.Resources.LoadTexture("woodBlock");

            for(int x = 0; x < ground.GetLength(0); x++) {
                for(int y = 0; y < ground.GetLength(1); y++) {
                    if(ground[x, y])
                        master.Renderer.Draw(tex, x*Pixels, master.GUI.ScreenHeight - y*Pixels, Pixels, Pixels);
                }
            }
        }
    }
}

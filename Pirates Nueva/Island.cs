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

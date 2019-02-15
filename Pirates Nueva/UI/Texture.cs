using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pirates_Nueva.UI
{
    /// <summary>
    /// A 2D image texture.
    /// </summary>
    public class Texture
    {
        /// <summary> The width of this <see cref="Texture"/>. </summary>
        public virtual int Width => Drawable.Width;
        /// <summary> The height of this <see cref="Texture"/>. </summary>
        public virtual int Height => Drawable.Height;

        public virtual Texture2D Drawable { get; }

        /// <summary>
        /// Create a new <see cref="Texture"/> from a MonoGame <see cref="Texture2D"/>.
        /// </summary>
        public Texture(Texture2D inner) {
            Drawable = inner;
        }
        /// <summary>
        /// Create a blank <see cref="Texture2D"/>; only usable from derived classes.
        /// </summary>
        protected Texture() { }
    }

    /// <summary>
    /// A 9-sliced <see cref="Texture"/>.
    /// </summary>
    public class NineSlice : Texture
    {
        private static readonly Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();

        public override Texture2D Drawable { get; }

        public NineSlice(SliceDef def, int width, int height, Master master) : base() {

            // Create a nine-sliced texture from the source and paramaters,
            // OR: fetch the cached one if we have previously created one with these parameters.
            if(textures.TryGetValue($"{def.ID}_{width}_{height}", out var tex) == false) {
                tex = CreateTex(def, width, height, master);
            }
            Drawable = tex;
        }

        private static Texture2D CreateTex(SliceDef def, int width, int height, Master master) {
            var source = master.Resources.LoadTexture(def.Texture);

            // Fetch the colors of the source texture for use in the /readInner()/ method below.
            var innerData = new Color[source.Width * source.Height];
            source.GetData(innerData);

            var (sourceWidth, sourceHeight) = (source.Width, source.Height); // The width and height of the source texture.
            int midWidth = sourceWidth - def.Slices.left - def.Slices.right; // The width of the middle slice.
            int midHeight = sourceHeight - def.Slices.bottom - def.Slices.top; // The height of the middle slice.
            int rightIndex = sourceWidth - def.Slices.right; // The index where the rightmost slice begins.
            int topIndex = sourceHeight - def.Slices.top; // The index where the topmost slice begins.

            var newData = new Color[width * height];
            for(int x = 0; x < width; x++) {
                for(int y = 0; y < height; y++) {
                    int tx, ty; // This point's corresponding index on the source texture, depending on slice positions.

                    if(x < def.Slices.left)                                      // If this point is on the left side of the texture, 
                        tx = x;                                                  //     its source index is in the leftmost set of slices.
                    else if(x >= width - def.Slices.right)                       // If this point is on the right side of the texture,
                        tx = rightIndex + x - (width - def.Slices.right);        //     its source index is in the rightmost set of slices.
                    else                                                         // If this point is (horizontally) in the middle,
                        tx = def.Slices.left + (x - def.Slices.left) % midWidth; //     its source index is in the middle set of slices.

                    if(y < def.Slices.bottom)                                        // If this point is on the bottom side of the texture,
                        ty = y;                                                      //     its source index is in the bottommost set of slices.
                    else if(y >= height - def.Slices.top)                            // If this point is on the top side of the texture,
                        ty = topIndex + y - (height - def.Slices.top);               //     its source index is in the topmost set of slices.
                    else                                                             // If this point is (vertically) in the middle,
                        ty = def.Slices.bottom + (y - def.Slices.bottom) % midHeight;//     its source index is in the middle set of slices.

                    newData[y * width + x] = readInner(tx, ty);
                }
            }
            var tex = new Texture2D(master.GraphicsDevice, width, height);
            tex.SetData(newData);

            textures[$"{def.ID}_{width}_{height}"] = tex; // Cache the texture we just made.

            return tex;

            // Read the specified color pixel from the inner texture.
            Color readInner(int x, int y) => innerData[y * sourceWidth + x];
        }
    }
}

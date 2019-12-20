using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Pirates_Nueva.UI
{
    /// <summary>
    /// A 2D image sprite.
    /// </summary>
    public class Sprite
    {
        private readonly Texture2D? source;

        /// <summary> The left edge of this <see cref="Sprite"/> on its source texture. </summary>
        protected internal int Left { get; }
        /// <summary> The top edge of this <see cref="Sprite"/> on its source texture. </summary>
        protected internal int Top { get; }

        /// <summary> The width of this <see cref="Sprite"/>. </summary>
        public int Width { get; }
        /// <summary> The height of this <see cref="Sprite"/>. </summary>
        public int Height { get; }

        protected internal virtual Texture2D Source => this.source ?? NullableUtil.ThrowNotInitialized<Texture2D>(nameof(Sprite));

        /// <summary>
        /// Creates a new <see cref="Sprite"/> from a MonoGame <see cref="Texture2D"/>.
        /// </summary>
        public Sprite(Texture2D inner, int left, int top, int width, int height) {
            this.source = inner;

            (Left, Top) = (left, top);
            (Width, Height) = (width, height);
        }
        internal Sprite(Texture2D inner, SpriteDef def)
            : this(inner, def.FromLeft, inner.Height - def.FromBottom - def.Height, def.Width, def.Height) { }
        /// <summary>
        /// Creates a blank <see cref="Sprite"/>; only usable from derived classes.
        /// </summary>
        protected Sprite(int width, int height)
            => (Width, Height) = (width, height);
    }

    /// <summary>
    /// A 9-sliced <see cref="Sprite"/>.
    /// </summary>
    public class NineSlice : Sprite
    {
        private static readonly Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();

        protected internal override Texture2D Source { get; }

        public NineSlice(SliceDef def, int width, int height, Master master) : base(width, height) {

            // Create a nine-sliced texture from the source and paramaters,
            // OR: fetch the cached one if we have previously created one with these parameters.
            if(!textures.TryGetValue($"{def.ID}_{width}_{height}", out var tex)) {
                tex = CreateTex(def, width, height, master);
            }
            Source = tex;
        }

        private static Texture2D CreateTex(SliceDef def, int width, int height, Master master) {
            Texture2D source = Resources.LoadSprite(def.SpriteID).Source;

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

                    newData[flatten(x, y, width)] = innerData[flatten(tx, ty, sourceWidth)];
                }
            }
            var tex = new Texture2D(master.GraphicsDevice, width, height);
            tex.SetData(newData);

            textures[$"{def.ID}_{width}_{height}"] = tex; // Cache the texture we just made.

            return tex;

            // Flattens the specified coordinates into a single line, using the specified width value.
            static int flatten(int x, int y, int width) => y * width + x;
        }
    }
}

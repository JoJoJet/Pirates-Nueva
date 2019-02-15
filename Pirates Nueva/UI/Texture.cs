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
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Pirates_Nueva.Ocean
{
    /// <summary>
    /// An implementation of <see cref="ILocalDrawer{T}"/> that depends on an instance of <see cref="Space{TSelf, TTransformer}"/>.
    /// </summary>
    /// <typeparam name="TLocus">The locus for this drawer's coordinate system.</typeparam>
    /// <typeparam name="TTransformer">The type that will perform transformation for this Drawer.</typeparam>
    /// <typeparam name="TParent">The type representing the parent coordinate system.</typeparam>
    public sealed class SpaceDrawer<TLocus, TTransformer, TParent> : ILocalDrawer<TLocus>
        where TLocus : ISpaceLocus<TLocus>
        where TTransformer : struct, ITransformer<TLocus>
    {
        private ILocalDrawer<TParent> ParentDrawer { get; }
        private Space<TLocus, TTransformer> Transformer { get; }

        public SpaceDrawer(ILocalDrawer<TParent> parentDrawer, Space<TLocus, TTransformer> space)
            => (ParentDrawer, Transformer) = (parentDrawer, space);

        public void DrawCornerAt<T>(UI.Sprite sprite, float left, float top, float width, float height, in UI.Color tint) {
            //
            // If we're drawing at the current space.
            if(typeof(T) == typeof(TLocus)) {
                //
                // Procedure for transformers with rotation.
                if(default(TTransformer).HasRotation) {
                    DrawAt<T>(sprite, left, top, width, height, Angle.Right, (0f, 1f), in tint);
                }
                //
                // Procedure for transformers without rotation.
                else {
                    var (parentX, parentY) = Transformer.PointFrom(left, top);
                    var (screenW, screenH) = (Transformer.ScaleFrom(width), Transformer.ScaleFrom(height));
                    ParentDrawer.DrawCorner(sprite, parentX, parentY, screenW, screenH, in tint);
                }
            }
            //
            // If we're drawing at a parent space.
            else {
                ParentDrawer.DrawCornerAt<T>(sprite, left, top, width, height, in tint);
            }
        }
        public void DrawAt<T>(UI.Sprite sprite, float x, float y, float width, float height,
                     in Angle angle, in PointF origin, in UI.Color tint)
        {
            //
            // If we're drawing at the current space.
            if(typeof(T) == typeof(TLocus)) {
                var (screenW, screenH) = (Transformer.ScaleFrom(width), Transformer.ScaleFrom(height));
                //
                // Procedure for transformers with rotation.
                if(default(TTransformer).HasRotation) {
                    var texOffset = (1, 1) - origin;
                    texOffset = (texOffset.X * width, texOffset.Y * height);
                    texOffset += PointF.Rotate((-0.5f, 0.5f), in angle);

                    var (parentX, parentY) = Transformer.PointFrom(x + texOffset.X, y + texOffset.Y);

                    ParentDrawer.Draw(sprite, parentX, parentY, screenW, screenH, -Transformer.AngleFrom(in angle), (0, 0), in tint);
                }
                //
                // Procedure for transformers without rotation.
                else {
                    var (parentX, parentY) = Transformer.PointFrom(x, y);
                    ParentDrawer.Draw(sprite, parentX, parentY, screenW, screenH, in angle, in origin, in tint);
                }
            }
            //
            // If we're drawing at a parent space.
            else {
                ParentDrawer.DrawAt<T>(sprite, x, y, width, height, in angle, in origin, in tint);
            }
        }

        public void DrawLineAt<T>(PointF start, PointF end, in UI.Color color) {
            if(typeof(T) == typeof(TLocus))
                ParentDrawer.DrawLine(Transformer.PointFrom(in start), Transformer.PointFrom(in end), in color);
            else
                ParentDrawer.DrawLineAt<T>(start, end, in color);
        }
        public void DrawString(UI.Font font, string text, float left, float top, in UI.Color color)
            => throw new NotImplementedException();
    }
}

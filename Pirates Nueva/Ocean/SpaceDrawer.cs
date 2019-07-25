﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Pirates_Nueva.Ocean
{
    /// <summary>
    /// An implementation of <see cref="ILocalDrawer{T}"/> that depends on an instance of <see cref="Space{TSelf, TTransformer}"/>.
    /// </summary>
    /// <typeparam name="TParent">The type representing the parent coordinate system.</typeparam>
    /// <typeparam name="TSpace">The type representing the local coordinate system.</typeparam>
    /// <typeparam name="TTransformer">The type that will perform transformation for this Drawer.</typeparam>
    public sealed class SpaceDrawer<TParent, TSpace, TTransformer> : ILocalDrawer<TSpace>
        where TTransformer : struct, ITransformer<TSpace>
    {
        private ILocalDrawer<TParent> Drawer { get; }
        private Space<TSpace, TTransformer> Transformer { get; }

        public SpaceDrawer(ILocalDrawer<TParent> parentDrawer, Space<TSpace, TTransformer> space)
            => (Drawer, Transformer) = (parentDrawer, space);

        public void DrawCorner(UI.Sprite sprite, float left, float top, float width, float height, in UI.Color tint) {
            //
            // Procedure for transformers with rotation.
            if(default(TTransformer).HasRotation) {
                Draw(sprite, left, top, width, height, Angle.Right, (0f, 1f), in tint);
            }
            //
            // Procedure for transformers without rotation.
            else {
                var (parentX, parentY) = Transformer.PointFrom(left, top);
                width = Transformer.ScaleFrom(width);
                height = Transformer.ScaleFrom(height);
                Drawer.DrawCorner(sprite, parentX, parentY, width, height, in tint);
            }
        }
        public void Draw(UI.Sprite sprite, float x, float y, float width, float height,
                     in Angle angle, in PointF origin, in UI.Color tint)
        {
            width = Transformer.ScaleFrom(width);
            height = Transformer.ScaleFrom(height);
            //
            // Procedure for transformers with rotation.
            if(default(TTransformer).HasRotation) {
                var texOffset = (1, 1) - origin;
                texOffset = (texOffset.X * width, texOffset.Y * height);
                texOffset += PointF.Rotate((-0.5f, 0.5f), in angle);

                var (parentX, parentY) = Transformer.PointFrom(x + texOffset.X, y + texOffset.Y);

                Drawer.Draw(sprite, parentX, parentY, width, height, -Transformer.AngleFrom(in angle), (0, 0), in tint);
            }
            //
            // Procedure for transformers without rotation.
            else {
                var (parentX, parentY) = Transformer.PointFrom(x, y);
                Drawer.Draw(sprite, parentX, parentY, width, height, in angle, in origin, in tint);
            }
        }

        public void DrawLine(PointF start, PointF end, in UI.Color color)
            => Drawer.DrawLine(Transformer.PointFrom(in start), Transformer.PointFrom(in end), in color);
        public void DrawString(UI.Font font, string text, float left, float top, in UI.Color color)
            => throw new NotImplementedException();
    }
}
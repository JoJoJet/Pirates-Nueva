﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Pirates_Nueva.Ocean
{
    /// <summary>
    /// The base type for <see cref="ISpaceLocus{TSelf}"/>.
    /// </summary>
    public interface ISpaceLocus
    {
        /// <summary>
        /// The coordinate system containing the current one.
        /// If null, that means the current system is the root.
        /// </summary>
        ISpaceLocus? Parent { get; }

        /// <summary> The object that handles transformation for this Locus. </summary>
        ISpace Transformer { get; }
    }
    /// <summary>
    /// An object that represents the central point in a local coordinate system.
    /// </summary>
    /// <typeparam name="TSelf">The type that is implementing this interface.</typeparam>
    public interface ISpaceLocus<TSelf> : ISpaceLocus
        where TSelf : ISpaceLocus<TSelf>
    {
        /// <summary> The object that handles transformation for this Locus. </summary>
        new ISpace<TSelf> Transformer { get; }
    }

    /// <summary>
    /// An object that performs transformation for coordinate system with specified Locus.
    /// </summary>
    /// <typeparam name="TLocus">The locus for this coordinate system.</typeparam>
    public interface ITransformer<TLocus>
        where TLocus : ISpaceLocus
    {
        /// <summary> Whether or not this type of Transformer might include rotation. Should be constant. </summary>
        bool HasRotation { get; }

        /// <summary> Transforms a point from parent space to local space. </summary>
        /// <param name="space">The instance around which to transform.</param>
        PointF PointTo(TLocus space, in PointF parent);
        /// <summary> Transforms a point from local space to parent space. </summary>
        /// <param name="space">The instance around which to transform.</param>
        PointF PointFrom(TLocus space, in PointF local);

        /// <summary> Transforms an angle from parent space to local space. </summary>
        /// <param name="space">The instance around which to transform.</param>
        Angle AngleTo(TLocus space, in Angle parent);
        /// <summary> Transforms an angle from local space to parent space. </summary>
        /// <param name="space">The instance around which to transform.</param>
        Angle AngleFrom(TLocus space, in Angle local);

        /// <summary> Scales a scalar value from parent space to local space. </summary>
        /// <param name="space">The instance around which to scale.</param>
        float ScaleTo(TLocus space, float parent);
        /// <summary> Scales a scalar value from local space to parent space. </summary>
        /// <param name="space">The instance around which to scale.</param>
        float ScaleFrom(TLocus space, float local);
    }
    /// <summary>
    /// The base type of <see cref="Space{TSpace, TTransformer}"/>.
    /// </summary>
    public interface ISpace
    {
        /// <summary> Transforms a point from parent space to local space. </summary>
        PointF PointTo(in PointF parentPoint);
        /// <summary> Transforms a point from local space to parent space. </summary>
        PointF PointFrom(in PointF localPoint);

        /// <summary> Transforms a point from root space to local space. </summary>
        PointF PointFromRoot(in PointF rootPoint);
        /// <summary> Transforms a point from local space to root space. </summary>
        PointF PointToRoot(in PointF localPoint);

        /// <summary> Transforms an <see cref="Angle"/> from parent space to local space. </summary>
        Angle AngleTo(in Angle parentAngle);
        /// <summary> Transforms an <see cref="Angle"/> from local space to parent space. </summary>
        Angle AngleFrom(in Angle localAngle);

        /// <summary> Scales a scalar value from parent space to local space. </summary>
        float ScaleTo(float parentScalar);
        /// <summary> Scales a scalar value from local space to parent space. </summary>
        float ScaleFrom(float localScalar);
    }
    /// <summary>
    /// The base type of <see cref="Space{TLocus, TTransformer}"/>.
    /// </summary>
    public interface ISpace<TLocus> : ISpace
        where TLocus : ISpaceLocus
    {  }
    /// <summary>
    /// An object representing a local system of coordinates.
    /// </summary>
    /// <typeparam name="TLocus">The type around which this coordinate system is centered.</typeparam>
    /// <typeparam name="TTransformer">The type that will perform transformation for this Space.</typeparam>
    public sealed class Space<TLocus, TTransformer> : ISpace<TLocus>
        where TLocus : ISpaceLocus<TLocus>
        where TTransformer : struct, ITransformer<TLocus>
    {
        private readonly TLocus locus;

        /// <summary>
        /// Creates a new Space instance centered around the specified object.
        /// </summary>
        public Space(TLocus locus) => this.locus = locus;

        /// <summary> Transforms a point from parent space to local space. </summary>
        /// <param name="parentPoint">The point in parent space.</param>
        public PointF PointTo(in PointF parentPoint) => default(TTransformer).PointTo(this.locus, in parentPoint);
        /// <summary> Transforms a point from parent space to local space. </summary>
        /// <param name="parentX">The x coordinate of the point in parent space.</param>
        /// <param name="parentY">The y coordinate of the point in parent space.</param>
        public PointF PointTo(float parentX, float parentY) => default(TTransformer).PointTo(this.locus, new PointF(parentX, parentY));

        /// <summary> Transforms a point from local space to parent space. </summary>
        /// <param name="localPoint">The point in local space.</param>
        public PointF PointFrom(in PointF localPoint) => default(TTransformer).PointFrom(this.locus, in localPoint);
        /// <summary> Transforms a point from local space to parent space. </summary>
        /// <param name="localX">The x coordinate of the point in local space.</param>
        /// <param name="localY">The y coordinate of the point in local space.</param>
        public PointF PointFrom(float localX, float localY) => default(TTransformer).PointFrom(this.locus, new PointF(localX, localY));

        /// <summary> Transforms a point from root space to local space. </summary>
        /// <param name="rootPoint">The point in root space.</param>
        public PointF PointFromRoot(in PointF rootPoint)
            => PointTo(this.locus.Parent?.Transformer.PointFromRoot(in rootPoint) ?? rootPoint);
        /// <summary> Transforms a point from root space to local space. </summary>
        /// <param name="rootX">The x coordinate of the point in root space.</param>
        /// <param name="rootY">The y coordinate of the point in root space.</param>
        public PointF PointFromRoot(float rootX, float rootY) => PointFromRoot(new PointF(rootX, rootY));

        /// <summary> Transforms a point from local space to root space. </summary>
        /// <param name="localPoint">The point in local space.</param>
        public PointF PointToRoot(in PointF localPoint) {
            var parentPoint = PointFrom(in localPoint);
            return this.locus.Parent?.Transformer.PointToRoot(in parentPoint) ?? parentPoint;
        }
        /// <summary> Transforms a point from local space to root space. </summary>
        /// <param name="localX">The x coordinate of the point in local space.</param>
        /// <param name="localY">The y coordinate of the point in local space.</param>
        public PointF PointToRoot(float localX, float localY) => PointToRoot(new PointF(localX, localY));


        /// <summary> Transforms an <see cref="Angle"/> from parent space to local space. </summary>
        /// <param name="parentAngle">The <see cref="Angle"/> in parent space.</param>
        public Angle AngleTo(in Angle parentAngle) => default(TTransformer).AngleTo(this.locus, in parentAngle);
        /// <summary> Transforms an <see cref="Angle"/> from local space to parent sapce. </summary>
        /// <param name="localAngle">The <see cref="Angle"/> in local space.</param>
        public Angle AngleFrom(in Angle localAngle) => default(TTransformer).AngleFrom(this.locus, in localAngle);


        /// <summary> Scales a scalar value from parent space to local space. </summary>
        /// <param name="parentScalar">The scalar value in parent space.</param>
        public float ScaleTo(float parentScalar) => default(TTransformer).ScaleTo(this.locus, parentScalar);
        /// <summary> Scales a scalar value from local space to parent space. </summary>
        /// <param name="localScalar">The scalar value in local space.</param>
        public float ScaleFrom(float localScalar) => default(TTransformer).ScaleFrom(this.locus, localScalar);
    }
}

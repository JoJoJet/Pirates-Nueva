using System;
using System.Collections.Generic;
using System.Text;

namespace Pirates_Nueva.Ocean
{
    /// <summary>
    /// An object that performs transformation for <see cref="Space{T, T}"/> of specified type.
    /// </summary>
    /// <typeparam name="TSpace">The type of space around which to transform.</typeparam>
    public interface ITransformer<TSpace>
    {
        /// <summary> Whether or not this type of Transformer might include rotation. Should be constant. </summary>
        bool HasRotation { get; }

        /// <summary> Transforms a point from parent space to local space. </summary>
        /// <param name="space">The instance around which to transform.</param>
        PointF PointTo(TSpace space, in PointF parent);
        /// <summary> Transforms a point from local space to parent space. </summary>
        /// <param name="space">The instance around which to transform.</param>
        PointF PointFrom(TSpace space, in PointF local);

        /// <summary> Transforms an angle from parent space to local space. </summary>
        /// <param name="space">The instance around which to transform.</param>
        Angle AngleTo(TSpace space, in Angle parent);
        /// <summary> Transforms an angle from local space to parent space. </summary>
        /// <param name="space">The instance around which to transform.</param>
        Angle AngleFrom(TSpace space, in Angle local);

        /// <summary> Scales a scalar value from parent space to local space. </summary>
        /// <param name="space">The instance around which to scale.</param>
        float ScaleTo(TSpace space, float parent);
        /// <summary> Scales a scalar value from local space to parent space. </summary>
        /// <param name="space">The instance around which to scale.</param>
        float ScaleFrom(TSpace space, float local);
    }
    /// <summary>
    /// An object representing a local system of coordinates.
    /// </summary>
    /// <typeparam name="TSelf">The type around which this coordinate system is centered.</typeparam>
    /// <typeparam name="TTransformer">The type that will perform transformation for this Space.</typeparam>
    public sealed class Space<TSelf, TTransformer>
        where TTransformer : struct, ITransformer<TSelf>
    {
        private readonly TSelf self;

        /// <summary>
        /// Creates a new Space instance centered around the specified object.
        /// </summary>
        public Space(TSelf self) => this.self = self;

        /// <summary> Transforms a point from parent space to local space. </summary>
        /// <param name="parentPoint">The point in parent space.</param>
        public PointF PointTo(in PointF parentPoint) => default(TTransformer).PointTo(this.self, in parentPoint);
        /// <summary> Transforms a point from parent space to local space. </summary>
        /// <param name="parentX">The x coordinate of the point in parent space.</param>
        /// <param name="parentY">The y coordinate of the point in parent space.</param>
        public PointF PointTo(float parentX, float parentY) => default(TTransformer).PointTo(this.self, new PointF(parentX, parentY));

        /// <summary> Transforms a point from local space to parent space. </summary>
        /// <param name="localPoint">The point in local space.</param>
        public PointF PointFrom(in PointF localPoint) => default(TTransformer).PointFrom(this.self, in localPoint);
        /// <summary> Transforms a point from local space to parent space. </summary>
        /// <param name="localX">The x coordinate of the point in local space.</param>
        /// <param name="localY">The y coordinate of the point in local space.</param>
        public PointF PointFrom(float localX, float localY) => default(TTransformer).PointFrom(this.self, new PointF(localX, localY));

        /// <summary> Transforms an <see cref="Angle"/> from parent space to local space. </summary>
        /// <param name="parentAngle">The <see cref="Angle"/> in parent space.</param>
        public Angle AngleTo(in Angle parentAngle) => default(TTransformer).AngleTo(this.self, in parentAngle);
        /// <summary> Transforms an <see cref="Angle"/> from local space to parent sapce. </summary>
        /// <param name="localAngle">The <see cref="Angle"/> in local space.</param>
        public Angle AngleFrom(in Angle localAngle) => default(TTransformer).AngleFrom(this.self, in localAngle);

        /// <summary> Scales a scalar value from parent space to local space. </summary>
        /// <param name="parentScalar">The scalar value in parent space.</param>
        public float ScaleTo(float parentScalar) => default(TTransformer).ScaleTo(this.self, parentScalar);
        /// <summary> Scales a scalar value from local space to parent space. </summary>
        /// <param name="localScalar">The scalar value in local space.</param>
        public float ScaleFrom(float localScalar) => default(TTransformer).ScaleFrom(this.self, localScalar);
    }
}

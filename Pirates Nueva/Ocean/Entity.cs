namespace Pirates_Nueva.Ocean
{
    /// <summary>
    /// An entity existing within a <see cref="Sea"/>.
    /// </summary>
    public abstract class Entity
    {
        /// <summary>
        /// Whether or not the input point is colliding with this <see cref="Entity"/>.
        /// </summary>
        public virtual bool IsColliding(PointF point) => GetBounds().Contains(point) && IsCollidingPrecise(point);
        /// <summary>
        /// A box drawn around this <see cref="Entity"/>, used for approximating collision.
        /// </summary>
        protected abstract BoundingBox GetBounds();
        /// <summary>
        /// Whether or not the specified point (<see cref="Sea"/>-space) is colliding with this <see cref="Entity"/>.
        /// </summary>
        protected abstract bool IsCollidingPrecise(PointF point);
    }
}

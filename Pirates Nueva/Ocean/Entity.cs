﻿namespace Pirates_Nueva.Ocean
{
    /// <summary>
    /// An entity existing within a <see cref="Sea"/>.
    /// </summary>
    public abstract class Entity
    {
        /// <summary> The <see cref="Ocean.Sea"/> containing this <see cref="Entity"/>. </summary>
        public Sea Sea { get; }

        /// <summary> The X coordinate of the center of this <see cref="Entity"/> within its <see cref="Ocean.Sea"/>. </summary>
        public float CenterX { get; protected set; }
        /// <summary> The Y coordinate of the center of this <see cref="Entity"/> within its <see cref="Ocean.Sea"/>. </summary>
        public float CenterY { get; protected set; }
        /// <summary> The center of this <see cref="Entity"/> within its <see cref="Ocean.Sea"/>. </summary>
        public PointF Center {
            get => (CenterX, CenterY);
            protected set => (CenterX, CenterY) = value;
        }

        public Entity(Sea sea) => Sea = sea;

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

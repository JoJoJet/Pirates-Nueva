﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pirates_Nueva
{
    /// <summary>
    /// An entity existing within a <see cref="Sea"/>.
    /// </summary>
    public abstract class Entity
    {
        /// <summary>
        /// A box drawn around this <see cref="Entity"/>, used for approximating collision.
        /// </summary>
        protected abstract BoundingBox Bounds { get; }
        /// <summary>
        /// Whether or not the specified point (<see cref="Sea"/>-space) is colliding with this <see cref="Entity"/>.
        /// </summary>
        protected abstract bool IsCollidingPrecise(PointF point);
    }
}

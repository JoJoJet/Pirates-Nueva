using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pirates_Nueva
{
    public struct BoundingBox
    {
        /// <summary> The left edge of this <see cref="BoundingBox"/>, in <see cref="Sea"/>-space. </summary>
        public float Left { get; }
        /// <summary> The bottom edge of this <see cref="BoundingBox"/>, in <see cref="Sea"/>-space. </summary>
        public float Bottom { get; }
        /// <summary> The top edge of this <see cref="BoundingBox"/>, in <see cref="Sea"/>-space. </summary>
        public float Top { get; }
        /// <summary> The right edge of this <see cref="BoundingBox"/>, in <see cref="Sea"/>-space. </summary>
        public float Right { get; }

        public float Width => Right - Left;
        public float Height => Top - Bottom;

        /// <summary> The bottom-left corner of this <see cref="BoundingBox"/>, in <see cref="Sea"/>-space. </summary>
        public PointF BottomLeft => (Left, Bottom);
        /// <summary> The top-right corner of this <see cref="BoundingBox"/>, in <see cref="Sea"/>-space. </summary>
        public PointF TopRight => (Right, Top);

        public BoundingBox(float left, float bottom, float top, float right) {
            Left = left;
            Bottom = bottom;
            Top = top;
            Right = right;
        }
        public BoundingBox(PointF bottomLeft, PointF topRight) : this(bottomLeft.X, bottomLeft.Y, topRight.Y, topRight.X) { }

        /// <summary> Whether or not the specified point falls within this <see cref="BoundingBox"/>. </summary>
        public bool Contains(PointF point) => Contains(point.X, point.Y);
        /// <summary> Whether or not the specified coordinates fall within this <see cref="BoundingBox"/>. </summary>
        public bool Contains(float x, float y) => x >= Left && x <= Right && y >= Bottom && y <= Top;
    }
}

using System;

namespace Pirates_Nueva.UI
{
    /// <summary>
    /// A pair of potentially moving coordinates, in screen space.
    /// </summary>
    public interface IScreenSpaceTarget
    {
        int X { get; }
        int Y { get; }
    }

    public enum Corner { TopLeft, TopRight, BottomRight, BottomLeft };
    /// <summary>
    /// A floating menu that follows a <see cref="IScreenSpaceTarget"/>
    /// </summary>
    public class FloatingMenu : GUI.Menu
    {
        /// <summary> The point that this <see cref="FloatingMenu"/> should follow. </summary>
        public IScreenSpaceTarget Target { get; }
        /// <summary>
        /// How much this <see cref="FloatingMenu"/> should be offset from /Target/, in proportion to the size of the screen.
        /// </summary>
        public PointF Offset { get; }
        /// <summary> Which corner of the menu will be aligned with <see cref="Target"/>. </summary>
        public Corner Corner { get; }

        public FloatingMenu(IScreenSpaceTarget target, PointF offset, Corner corner, params GUI.Element<GUI.Menu>[] elements)
            : base(elements)
        {
            Target = target;
            Offset = offset;
            Corner = corner;
        }

        protected override void Draw(Master master) {
            // Find the extents of the menu.
            var (rightBound, bottomBound) = (0, 0);
            foreach(var el in Elements) {
                rightBound  = Math.Max(rightBound, el.Left + el.WidthPixels  + Padding);
                bottomBound = Math.Max(bottomBound, el.Top + el.HeightPixels + Padding);
            }

            // Offset the Menu by a different amount depending on which Corner we are pinning against.
            PointI offset = ((int)(Offset.X * master.GUI.ScreenWidth), (int)(Offset.Y * master.GUI.ScreenHeight));
            if(Corner == Corner.TopLeft)
                offset += (Target.X, Target.Y);
            if(Corner == Corner.TopRight)
                offset += (Target.X - rightBound, Target.Y);
            else if(Corner == Corner.BottomRight)
                offset += (Target.X - rightBound, Target.Y - bottomBound);
            else if(Corner == Corner.BottomLeft)
                offset += (Target.X, Target.Y - bottomBound);

            foreach(var el in Elements) {
                DrawElement(master, el, offset.X, offset.Y);
            }
        }
    }
}

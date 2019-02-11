using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// <summary> Which corner of the menu will be aligned with <see cref="Target"/>. </summary>
        public Corner Corner { get; }

        public FloatingMenu(IScreenSpaceTarget target, Corner corner, params GUI.MenuElement[] elements) : base(elements) {
            Target = target;
            Corner = corner;
        }

        protected override void Draw(Master master) {
            // Find the extents of the menu.
            var (rightBound, bottomBound) = (0, 0);
            foreach(GUI.MenuElement el in Elements) {
                rightBound  = Math.Max(rightBound, el.Left + el.WidthPixels  + Padding);
                bottomBound = Math.Max(bottomBound, el.Top + el.HeightPixels + Padding);
            }

            var offset = (Target.X, Target.Y);
            if(Corner == Corner.TopRight)
                offset = (Target.X - rightBound, Target.Y);
            else if(Corner == Corner.BottomRight)
                offset = (Target.X - rightBound, Target.Y - bottomBound);
            else if(Corner == Corner.BottomLeft)
                offset = (Target.X, Target.Y - bottomBound);

            foreach(GUI.MenuElement el in Elements) {
                var local = (el.Left, el.Top);
                DrawElement(master, el, local.Left + offset.X, local.Top + offset.Y);
            }
        }
    }
}

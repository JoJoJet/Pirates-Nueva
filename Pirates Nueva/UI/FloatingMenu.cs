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
        private PointF? resolvedOffset;

        /// <summary> The point that this <see cref="FloatingMenu"/> should follow. </summary>
        public IScreenSpaceTarget Target { get; }
        /// <summary>
        /// How much this <see cref="FloatingMenu"/> should be offset from /Target/, in proportion to the size of the screen.
        /// </summary>
        public PointF Offset { get; }
        /// <summary> Which corner of the menu will be aligned with <see cref="Target"/>. </summary>
        public Corner Corner { get; }

        private PointF ResolvedOffset => this.resolvedOffset ?? NullableUtil.ThrowNotInitialized<PointF>();

        public FloatingMenu(IScreenSpaceTarget target, PointF offset, Corner corner, params GUI.Element<GUI.Menu>[] elements)
            : base(elements)
        {
            Target = target;
            Offset = offset;
            Corner = corner;
        }

        protected override void Draw<TDrawer>(in TDrawer drawer, Master master) {
            CalcOffset();
            var localDrawer = new FloatingMenuDrawer<TDrawer>(this, in drawer);

            foreach(var el in Elements) {
                DrawElement(el, in localDrawer, master);
            }
        }

        protected override PointF ScreenPointToMenu(int screenX, int screenY)
            => new PointF(screenX - ResolvedOffset.X, screenY - ResolvedOffset.Y);

        private void CalcOffset() {
            // Find the extents of the menu.
            var (rightBound, bottomBound) = (0, 0);
            foreach(var el in Elements) {
                rightBound = Math.Max(rightBound, el.Left + el.Width + Padding);
                bottomBound = Math.Max(bottomBound, el.Top + el.Height + Padding);
            }

            // Offset the Menu by a different amount depending on which Corner we are pinning against.
            PointI offset = ((int)(Offset.X * Screen.Width), (int)(Offset.Y * Screen.Height));
            if(Corner == Corner.TopLeft)
                offset += (Target.X, Target.Y);
            else if(Corner == Corner.TopRight)
                offset += (Target.X - rightBound, Target.Y);
            else if(Corner == Corner.BottomRight)
                offset += (Target.X - rightBound, Target.Y - bottomBound);
            else if(Corner == Corner.BottomLeft)
                offset += (Target.X, Target.Y - bottomBound);

            this.resolvedOffset = offset;
        }

        private readonly struct FloatingMenuDrawer<TScreenDrawer> : ILocalDrawer<GUI.Menu>
            where TScreenDrawer : ILocalDrawer<Screen>
        {
            public FloatingMenu Menu { get; }
            public TScreenDrawer Drawer { get; }

            public FloatingMenuDrawer(FloatingMenu menu, in TScreenDrawer drawer) {
                Menu = menu;
                Drawer = drawer;
            }

            public void DrawCornerAt<T>(Sprite sprite, float left, float top, float width, float height, in Color tint) {
                if(typeof(T) == typeof(GUI.Menu))
                    Drawer.DrawCorner(sprite, left + Menu.ResolvedOffset.X, top + Menu.ResolvedOffset.Y, width, height, in tint);
                else
                    Drawer.DrawCornerAt<T>(sprite, left, top, width, height, in tint);
            }
            public void DrawAt<T>(Sprite sprite, float x, float y, float width, float height,
                                  in Angle angle, in PointF origin, in Color tint) {
                if(typeof(T) == typeof(GUI.Menu))
                    Drawer.Draw(sprite, x + Menu.ResolvedOffset.X, y + Menu.ResolvedOffset.Y, width, height, in angle, in origin, in tint);
                else
                    Drawer.DrawAt<T>(sprite, x, y, width, height, in angle, in origin, in tint);
            }
            public void DrawLineAt<T>(in PointF start, in PointF end, in Color color) {
                if(typeof(T) == typeof(GUI.Menu))
                    Drawer.DrawLine(start + Menu.ResolvedOffset, end + Menu.ResolvedOffset, in color);
                else
                    Drawer.DrawLineAt<T>(start, end, in color);
            }
            public void DrawString(Font font, string text, float left, float top, in Color color)
                => Drawer.DrawString(font, text, left + Menu.ResolvedOffset.X, top + Menu.ResolvedOffset.Y, in color);
        }
    }
}

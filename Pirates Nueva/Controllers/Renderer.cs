using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pirates_Nueva
{
    /// <summary>
    /// An object that draws things to the screen.
    /// </summary>
    public interface IDrawer
    {
        /// <summary>
        /// Submits a <see cref="UI.Sprite"/> to be drawn this frame, drawing from the top left corner.
        /// </summary>
        /// <param name="left">The local coordinate of the <see cref="UI.Sprite"/>'s left edge.</param>
        /// <param name="top">The local coordinate of the <see cref="UI.Sprite"/>'s top edge.</param>
        /// <param name="width">The width of the <see cref="UI.Sprite"/>, in local units.</param>
        /// <param name="height">The height of the <see cref="UI.Sprite"/>, in local units.</param>
        void DrawCorner(UI.Sprite sprite, float left, float top, float width, float height, in UI.Color tint);
        /// <summary>
        /// Submits a rotated <see cref="UI.Sprite"/> to be drawn this frame.
        /// </summary>
        /// <param name="x">The local x coordinate at which to draw the <see cref="UI.Sprite"/>.</param>
        /// <param name="y">The local y coordinate at which to draw the <see cref="UI.Sprite"/>.</param>
        /// <param name="width">The width of the <see cref="UI.Sprite"/>, in local units.</param>
        /// <param name="height">The height of the <see cref="UI.Sprite"/>, in local units.</param>
        /// <param name="angle">The <see cref="Angle"/> at which to draw the <see cref="UI.Sprite"/>.</param>
        /// <param name="origin">The point, relative to the <see cref="UI.Sprite"/>,
        ///                      from which to draw it. Range: [0, 1].</param>
        void Draw(UI.Sprite sprite, float x, float y, float width, float height,
                  in Angle angle, in PointF origin, in UI.Color tint);

        /// <summary>
        /// Draws a line with specified color.
        /// </summary>
        /// <param name="start">The local position of the line's starting point.</param>
        /// <param name="end">The local position of the line's ending point.</param>
        void DrawLine(PointF start, PointF end, in UI.Color color);

        /// <summary>
        /// Submits a <see cref="string"/> to be drawn this frame.
        /// </summary>
        /// <param name="left">The coordinate of the <see cref="string"/>'s left edge.</param>
        /// <param name="top">The coordinate of the <see cref="string"/>'s top edge.</param>
        void DrawString(UI.Font font, string text, float left, float top, in UI.Color color);
    }

    /// <summary>
    /// An object that transforms and draws things to the screen.
    /// </summary>
    /// <typeparam name="T">The type of object around which things will be drawn.</typeparam>
    public interface ILocalDrawer<T> : IDrawer
    {
        void IDrawer.DrawCorner(UI.Sprite sprite, float left, float top, float width, float height, in UI.Color tint)
            => DrawCornerAt<T>(sprite, left, top, width, height, in tint);
        void IDrawer.Draw(UI.Sprite sprite, float x, float y, float width, float height,
                          in Angle angle, in PointF origin, in UI.Color tint)
            => DrawAt<T>(sprite, x, y, width, height, in angle, in origin, in tint);

        void IDrawer.DrawLine(PointF start, PointF end, in UI.Color color)
            => DrawLineAt<T>(start, end, in color);

        /// <summary>
        /// Submits a <see cref="UI.Sprite"/> to be drawn at the specified space, drawing from the top left corner.
        /// </summary>
        /// <typeparam name="U">The space around which the sprite will be drawn.</typeparam>
        /// <param name="left">The local coordinate of the <see cref="UI.Sprite"/>'s left edge.</param>
        /// <param name="top">The local coordinate of the <see cref="UI.Sprite"/>'s top edge.</param>
        /// <param name="width">The width of the <see cref="UI.Sprite"/>, in local units.</param>
        /// <param name="height">The height of the <see cref="UI.Sprite"/>, in local units.</param>
        void DrawCornerAt<U>(UI.Sprite sprite, float left, float top, float width, float height, in UI.Color tint);
        /// <summary>
        /// Submits a rotated <see cref="UI.Sprite"/> to be drawn at the specified space.
        /// </summary>
        /// <typeparam name="U">The space around which the sprite will be drawn.</typeparam>
        /// <param name="x">The local x coordinate at which to draw the <see cref="UI.Sprite"/>.</param>
        /// <param name="y">The local y coordinate at which to draw the <see cref="UI.Sprite"/>.</param>
        /// <param name="width">The width of the <see cref="UI.Sprite"/>, in local units.</param>
        /// <param name="height">The height of the <see cref="UI.Sprite"/>, in local units.</param>
        /// <param name="angle">The <see cref="Angle"/> at which to draw the <see cref="UI.Sprite"/>.</param>
        /// <param name="origin">The point, relative to the <see cref="UI.Sprite"/>,
        ///                      from which to draw it. Range: [0, 1].</param>
        void DrawAt<U>(UI.Sprite sprite, float x, float y, float width, float height, in Angle angle, in PointF origin, in UI.Color tint);

        /// <summary>
        /// Draws a line with specified color at the specified space.
        /// </summary>
        /// <typeparam name="U">The space around which the sprite will be drawn.</typeparam>
        /// <param name="start">The local position of the line's starting point.</param>
        /// <param name="end">The local position of the line's ending point.</param>
        void DrawLineAt<U>(PointF start, PointF end, in UI.Color color);
    }
    public static class DrawerExt
    {
        /// <summary>
        /// Submits a <see cref="UI.Sprite"/> to be drawn this frame, drawing from the center of the sprite.
        /// </summary>
        /// <param name="centerX">The local x coordinate of the <see cref="UI.Sprite"/>'s center.</param>
        /// <param name="centerY">The local y coordinate of the <see cref="UI.Sprite"/>'s center.</param>
        /// <param name="width">The width of the <see cref="UI.Sprite"/>, in local units.</param>
        /// <param name="height">The height of the <see cref="UI.Sprite"/>, in local units.</param>
        public static void DrawCenter<T>(this ILocalDrawer<T> drawer, UI.Sprite sprite,
                                         float centerX, float centerY, float width, float height, in UI.Color tint)
            => drawer.DrawCorner(sprite, centerX - width / 2, centerY + width / 2, width, height, in tint);


        /// <summary>
        /// Submits a <see cref="UI.Sprite"/> to be drawn this frame, drawing from the center of the sprite.
        /// </summary>
        /// <param name="centerX">The local x coordinate of the <see cref="UI.Sprite"/>'s center.</param>
        /// <param name="centerY">The local y coordinate of the <see cref="UI.Sprite"/>'s center.</param>
        /// <param name="width">The width of the <see cref="UI.Sprite"/>, in local units.</param>
        /// <param name="height">The height of the <see cref="UI.Sprite"/>, in local units.</param>
        public static void DrawCenter<T>(this ILocalDrawer<T> drawer, UI.Sprite sprite,
                                         float centerX, float centerY, float width, float height)
            => drawer.DrawCenter(sprite, centerX, centerY, width, height, in UI.Color.White);
        /// <summary>
        /// Submits a <see cref="UI.Sprite"/> to be drawn this frame, drawing from the top left corner.
        /// </summary>
        /// <param name="left">The local coordinate of the <see cref="UI.Sprite"/>'s left edge.</param>
        /// <param name="top">The local coordinate of the <see cref="UI.Sprite"/>'s top edge.</param>
        /// <param name="width">The width of the <see cref="UI.Sprite"/>, in local units.</param>
        /// <param name="height">The height of the <see cref="UI.Sprite"/>, in local units.</param>
        public static void DrawCorner<T>(this ILocalDrawer<T> drawer, UI.Sprite sprite,
                                         float left, float top, float width, float height)
            => drawer.DrawCorner(sprite, left, top, width, height, in UI.Color.White);
        /// <summary>
        /// Submits a rotated <see cref="UI.Sprite"/> to be drawn this frame.
        /// </summary>
        /// <param name="x">The local x coordinate at which to draw the <see cref="UI.Sprite"/>.</param>
        /// <param name="y">The local y coordinate at which to draw the <see cref="UI.Sprite"/>.</param>
        /// <param name="width">The width of the <see cref="UI.Sprite"/>, in local units.</param>
        /// <param name="height">The height of the <see cref="UI.Sprite"/>, in local units.</param>
        /// <param name="angle">The <see cref="Angle"/> at which to draw the <see cref="UI.Sprite"/>.</param>
        /// <param name="origin">The point, relative to the <see cref="UI.Sprite"/>,
        ///                      from which to draw it. Range: [0, 1].</param>
        public static void Draw<T>(this ILocalDrawer<T> drawer, UI.Sprite sprite,
                                   float x, float y, float width, float height,
                                   in Angle angle, in PointF origin)
            => drawer.Draw(sprite, x, y, width, height, in angle, in origin, in UI.Color.White);
    }

    /// <summary>
    /// An instance that can be locally drawn around an object.
    /// </summary>
    /// <typeparam name="T">The type of object around which the instance will be drawn.</typeparam>
    internal interface IDrawable<T>
    {
        /// <summary>
        /// Draws this object around its parent.
        /// </summary>
        /// <param name="drawer">The object on which to draw.</param>
        void Draw(ILocalDrawer<T> drawer);
    }


    /// <summary>
    /// Controls rendering for <see cref="Pirates_Nueva"/>.
    /// </summary>
    internal class Renderer : ILocalDrawer<Screen>, ILocalDrawer<UI.Edge>
    {
        private Lazy<UI.Sprite> pixelLazy;

        private Master Master { get; }
        private SpriteBatch SpriteBatch { get; }

        private UI.Sprite Pixel => this.pixelLazy.Value;

        internal Renderer(Master master, SpriteBatch spriteBatch) {
            Master = master;
            SpriteBatch = spriteBatch;

            this.pixelLazy = new Lazy<UI.Sprite>(() => Master.CreateSprite(1, 1, UI.Color.White));
        }


        public void DrawCornerAt<T>(UI.Sprite sprite, float left, float top, float width, float height, in UI.Color tint) {
            if(typeof(T) == typeof(Screen) || typeof(T) == typeof(UI.Edge))
                SpriteBatch.Draw(sprite.Source, new Rectangle((int)left, (int)top, (int)width, (int)height),
                                 new Rectangle(sprite.Left, sprite.Top, sprite.Width, sprite.Height), tint);
            else
                ThrowInvalidType<T>(nameof(DrawCornerAt));
        }
        public void DrawAt<T>(UI.Sprite sprite, float x, float y, float width, float height,
                         in Angle angle, in PointF origin, in UI.Color tint) {
            if(typeof(T) == typeof(Screen) || typeof(T) == typeof(UI.Edge))
                SpriteBatch.Draw(sprite.Source, new Rectangle((int)x, (int)y, (int)width, (int)height),
                                 new Rectangle(sprite.Left, sprite.Top, sprite.Width, sprite.Height),
                                 tint, angle, origin, SpriteEffects.None, 0);
            else
                ThrowInvalidType<T>(nameof(DrawAt));
        }

        public void DrawLineAt<T>(PointF start, PointF end, in UI.Color color) {
            if(typeof(T) == typeof(Screen) || typeof(T) == typeof(UI.Edge)) {
                var edge = end - start;
                var angle = Angle.FromRadians(MathF.Atan2(edge.Y, edge.X));

                DrawAt<Screen>(Pixel, start.X, start.Y, edge.Magnitude, 1, angle, (0, 0), in color);
            }
            else
                ThrowInvalidType<T>(nameof(DrawLineAt));
        }

        public void DrawString(UI.Font font, string text, float left, float top, in UI.Color color)
            => SpriteBatch.DrawString(font, text, new PointF(left, top), color);

        public void DrawCorner(UI.Sprite sprite, float left, float top, float width, float height, in UI.Color tint)
            => DrawCornerAt<Screen>(sprite, left, top, width, height, in tint);
        public void Draw(UI.Sprite sprite, float x, float y, float width, float height,
                         in Angle angle, in PointF origin, in UI.Color tint)
            => DrawAt<Screen>(sprite, x, y, width, height, in angle, in origin, in tint);

        public void DrawLine(PointF start, PointF end, in UI.Color color)
            => DrawLineAt<Screen>(start, end, in color);

        private static void ThrowInvalidType<T>(string callingMethod)
            => throw new ArgumentException($"{nameof(Renderer)}.{callingMethod}<{typeof(T).Name}>(): Type parameter is not valid!");
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Pirates_Nueva.Ocean
{
    public sealed class Cannonball : Entity, IUpdatable, IDrawable<Sea>
    {
        public Sea Sea { get; }

        public float CenterX { get; private set; }
        public float CenterY { get; private set; }
        public PointF Center {
            get => (CenterX, CenterY);
            private set => (CenterX, CenterY) = value;
        }

        public Vector Velocity { get; private set; }

        public Cannonball(Sea sea, float centerX, float centerY, Vector velocity) {
            Sea = sea;
            (CenterX, CenterY) = (centerX, centerY);
            Velocity = velocity;
        }

        protected override BoundingBox GetBounds()
            => new BoundingBox(CenterX - 0.5f, CenterY - 0.5f, CenterX + 0.5f, CenterY + 0.5f);
        protected override bool IsCollidingPrecise(PointF point) => true;

        void IUpdatable.Update(Master master, Time delta) {
            Center += Velocity * delta * 10;
        }

        void IDrawable<Sea>.Draw(ILocalDrawer<Sea> drawer) {
            var sprite = Resources.LoadSprite("cannonball");
            drawer.DrawCenter(sprite, CenterX, CenterY, 1, 1);
        }
    }
}

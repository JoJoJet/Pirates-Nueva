using System;
using System.Collections.Generic;
using System.Text;

namespace Pirates_Nueva.Ocean
{
    public sealed class Cannonball : Entity, IUpdatable, IDrawable<Sea>
    {
        float distanceTraveled;

        public Vector Velocity { get; private set; }

        public Cannonball(Sea sea, float centerX, float centerY, Vector velocity) : base(sea) {
            (CenterX, CenterY) = (centerX, centerY);
            Velocity = velocity;
        }

        protected override BoundingBox GetBounds()
            => new BoundingBox(CenterX - 0.5f, CenterY - 0.5f, CenterX + 0.5f, CenterY + 0.5f);
        protected override bool IsCollidingPrecise(PointF point) => true;

        void IUpdatable.Update(in UpdateParams @params) {
            //
            // Move the shot according to its velocity.
            var move = Velocity * @params.Delta * 30;
            Center += move;
            //
            // Kill the shot if it travels too far.
            this.distanceTraveled += move.Magnitude;
            if(this.distanceTraveled > 30)
                Sea.RemoveEntity(this);
        }

        void IDrawable<Sea>.Draw(ILocalDrawer<Sea> drawer) {
            var sprite = Resources.LoadSprite("cannonball");
            drawer.DrawCenter(sprite, CenterX, CenterY, 1, 1);
        }
    }
}

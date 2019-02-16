using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pirates_Nueva
{
    public class Agent : IDrawable
    {
        /// <summary> The <see cref="Pirates_Nueva.Ship"/> that contains this <see cref="Agent"/>. </summary>
        public Ship Ship { get; }

        /// <summary> The <see cref="Block"/> that this <see cref="Agent"/> is standing on or moving from. </summary>
        public Block CurrentBlock { get; protected set; }
        /// <summary> The <see cref="Block"/> that this <see cref="Agent"/> is moving to. </summary>
        public Block NextBlock { get; protected set; }
        /// <summary>
        /// This <see cref="Agent"/>'s progress in moving between <see cref="CurrentBlock"/> and <see cref="NextBlock"/>.
        /// </summary>
        public float MoveProgress { get; protected set; }
        
        /// <summary> The X coordinate of this <see cref="Agent"/>, local to its <see cref="Pirates_Nueva.Ship"/>. </summary>
        public float X => Lerp(CurrentBlock.X, (NextBlock??CurrentBlock).X, MoveProgress);
        /// <summary> The Y coordinate of this <see cref="Agent"/>, local to its <see cref="Pirates_Nueva.Ship"/>. </summary>
        public float Y => Lerp(CurrentBlock.Y, (NextBlock??CurrentBlock).Y, MoveProgress);

        /// <summary> Linearly interpolate between two values, by amount /f/. </summary>
        private float Lerp(float a, float b, float f) => a * (1 - f) + b * f;

        public Agent(Ship ship, Block floor) {
            Ship = ship;
            CurrentBlock = floor;
        }

        #region IDrawable Implementation
        void IDrawable.Draw(Master master) {
            var tex = master.Resources.LoadTexture("agent");

            (float seaX, float seaY) = Ship.ShipPointToSea(X, Y+1);
            (int screenX, int screenY) = Ship.Sea.SeaPointToScreen(seaX, seaY);
            master.Renderer.DrawRotated(tex, screenX, screenY, Ship.Part.Pixels, Ship.Part.Pixels, -Ship.Angle, (0, 0));
        }
        #endregion
    }
}

using System;

namespace Pirates_Nueva
{
    public class Camera
    {
        private readonly Master master;
        private readonly Sea sea;
        private float _zoom = 32;

        /// <summary> The left edge of this camera, in <see cref="Sea"/>-space. </summary>
        public float Left { get; internal set; }
        /// <summary> The bottom edge of this camera, in <see cref="Sea"/>-space. </summary>
        public float Bottom { get; internal set; }

        /// <summary> How much this <see cref="Camera"/> is zoomed. </summary>
        public float Zoom {
            get => this._zoom;
            internal set {
                if(this._zoom != value) {
                    var first = sea.ScreenPointToSea(master.Input.MousePosition);   // Store the mouse position in the sea.
                    this._zoom = Math.Max(value, 1);                                // Zoom the camera.
                    var current = sea.ScreenPointToSea(master.Input.MousePosition); // Find the new mouse position in the sea.
                                                                                    //
                    var (deltaX, deltaY) = first - current;                         // Find how much the mouse-sea position moved.
                                                                                    //
                    Left += deltaX;                                                 // Re-center the camera around the mouse.
                    Bottom += deltaY;                                               //
                }
            }
        }

        public Camera(Master master, Sea sea) {
            this.master = master;
            this.sea = sea;
        }
    }
}

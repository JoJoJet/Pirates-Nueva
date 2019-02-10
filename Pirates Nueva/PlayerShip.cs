using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pirates_Nueva
{
    public class PlayerShip : Ship
    {
        public PlayerShip(Sea sea, int width, int height) : base(sea, width, height) {  }
        
        public override void Update(Master master) {
            // Get a PointF containing the direction of the user's arrow keys or WASD.
            PointF inputAxes = new PointF(master.Input.Horizontal, master.Input.Vertical).Normalized;

            // Do ship movement if arrow keys or WASD are held.
            if(inputAxes.SqrMagnitude > 0) {
                float deltaTime = master.FrameTime.DeltaSeconds();

                // Slowly rotate the ship to point at the input axes.
                Angle inputAngle = PointF.Angle((1, 0), inputAxes);
                this.Angle = Angle.MoveTowards(this.Angle, inputAngle, deltaTime);

                // Slowly move the ship in the direction of its right edge.
                Center += Right * deltaTime * 3;
            }

            // If the user left clicks, place a block.
            if(master.Input.MouseLeft.IsDown) {
                var (shipX, shipY) = mouseToShip();

                // If the place that the user clicked is within this ship, and that spot is not occupied.
                if(isValidIndex(shipX, shipY) && HasBlock(shipX, shipY) == false)
                    PlaceBlock("wood", shipX, shipY);
            }
            // If the user right clicks, remove a block.
            else if(master.Input.MouseRight.IsDown) {
                var (shipX, shipY) = mouseToShip();

                // If the place that the user clicked is within this ship, that spot has a block, and that block is not the Root.
                if(isValidIndex(shipX, shipY) && GetBlock(shipX, shipY) is Block b && b.ID != RootID)
                    RemoveBlock(shipX, shipY);
            }

            // Get the mouse cursor's positioned, tranformed to an index within this Ship.
            (int, int) mouseToShip() {
                var (x, y) = Sea.ScreenPointToSea(master.Input.MousePosition);
                return SeaPointToShip(x, y);
            }

            // Check if the input indices are within the bounds of this ship.
            bool isValidIndex(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;
        }
    }
}

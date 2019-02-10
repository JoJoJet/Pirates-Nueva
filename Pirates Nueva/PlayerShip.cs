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
            UpdateMovement(master);

            UpdateEditing(master);
        }

        /// <summary>
        /// Allows the user to control movement of this <see cref="PlayerShip"/>.
        /// </summary>
        void UpdateMovement(Master master) {
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
        }

        /// <summary>
        /// Allows the user to place and remove blocks in this <see cref="PlayerShip"/>.
        /// </summary>
        void UpdateEditing(Master master) {
            // If the user left clicks, place a Block or Furniture.
            if(master.Input.MouseLeft.IsDown) {
                var (shipX, shipY) = mouseToShip();

                // If the user is holding left shift, try to place a furniture.
                if(master.Input.LShift.IsPressed) {
                    // If the place the user clicked is within this ship, and that spot has a Block but no Furniture.
                    if(isValidIndex(shipX, shipY) && HasBlock(shipX, shipY) && HasFurniture(shipX, shipY) == false)
                        PlaceFurniture(FurnitureDef.Get("cannon"), shipX, shipY);
                }
                // If the user is not holding left shift, try to place a block.
                else {
                    // If the place that the user clicked is within this ship, and that spot is not occupied.
                    if(isValidIndex(shipX, shipY) && HasBlock(shipX, shipY) == false)
                        PlaceBlock("wood", shipX, shipY);
                }
            }
            // If the user right clicks, remove a Block or Furniture.
            else if(master.Input.MouseRight.IsDown) {
                var (shipX, shipY) = mouseToShip();

                // If the user is holding left shift, try to remove a Furniture.
                if(master.Input.LShift.IsPressed) {
                    if(isValidIndex(shipX, shipY) && HasFurniture(shipX, shipY))
                        RemoveFurniture(shipX, shipY);
                }
                // If the user is not holding left shift, try to remove a Block.
                else {
                    // If the place that the user clicked is within this ship, that spot has a block, and that block is not the Root.
                    if(isValidIndex(shipX, shipY) && GetBlock(shipX, shipY) is Block b && b.ID != RootID)
                        RemoveBlock(shipX, shipY);
                }
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

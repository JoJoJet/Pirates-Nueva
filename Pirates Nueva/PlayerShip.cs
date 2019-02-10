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
            UpdateEditing(master);

            UpdateMovement(master);
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
            if(master.Input.MouseLeft.IsDown && isMouseValid(out int shipX, out int shipY)) {

                // If the user is holding left shift, try to place a furniture.
                if(master.Input.LShift.IsPressed) {
                    // If the place the user clicked has a Block but no Furniture.
                    if(HasBlock(shipX, shipY) && HasFurniture(shipX, shipY) == false)
                        PlaceFurniture(FurnitureDef.Get("cannon"), shipX, shipY);
                }
                // If the user is not holding left shift, try to place a block.
                else {
                    // If the place that the user clicked is not occupied.
                    if(HasBlock(shipX, shipY) == false)
                        PlaceBlock("wood", shipX, shipY);
                }
            }
            // If the user right clicks, remove a Block or Furniture.
            if(master.Input.MouseRight.IsDown && isMouseValid(out shipX, out shipY)) {

                // If the user is holding left shift, try to remove a Furniture.
                if(master.Input.LShift.IsPressed) {
                    if(HasFurniture(shipX, shipY))
                        RemoveFurniture(shipX, shipY);
                }
                // If the user is not holding left shift, try to remove a Block.
                else {
                    // If the place that the user clicked has a block, and that block is not the Root.
                    if(GetBlock(shipX, shipY) is Block b && b.ID != RootID)
                        RemoveBlock(shipX, shipY);
                }
            }
            
            // Return whether or not the user clicked within the ship, and give
            // the position (local to the ship) as out paremters /x/ and /y/.
            bool isMouseValid(out int x, out int y) {
                var(seaX, seaY) = Sea.ScreenPointToSea(master.Input.MousePosition);
                (x, y) = SeaPointToShip(seaX, seaY);

                return x >= 0 && x < Width && y >= 0 && y < Height;
            }
        }
    }
}

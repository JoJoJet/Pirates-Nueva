using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pirates_Nueva
{
    public class PlayerShip : Ship
    {
        enum ShipMode { None, Movement, Editing };
        enum PlaceMode { Block, Furniture };

        private ShipMode mode;
        private PlaceMode placeMode;

        public PlayerShip(Sea sea, int width, int height) : base(sea, width, height) {  }
        
        public override void Update(Master master) {
            const string noneKey = "playershipmode_none";
            const string editKey = "playershipmode_edit";
            const string moveKey = "playershipmode_move";

            // If there is no floating menu for the ship onscreen, put one up.
            if(master.GUI.HasEdge(noneKey) == false) {
                master.GUI.AddEdge(noneKey, new EdgeButton("None", master.Font, () => mode = ShipMode.None, GUI.Edge.Bottom, GUI.Direction.Right));
                master.GUI.AddEdge(editKey, new EdgeButton("Edit", master.Font, () => mode = ShipMode.Editing, GUI.Edge.Bottom, GUI.Direction.Right));
                master.GUI.AddEdge(moveKey, new EdgeButton("Move", master.Font, () => mode = ShipMode.Movement, GUI.Edge.Bottom, GUI.Direction.Right));
            }
            
            if(mode == ShipMode.Editing) {
                updateEditing();
            }
            // If the mode is not 'Editing', remove the associated menu.
            else if(master.GUI.HasEdge("shipediting_block")) {
                master.GUI.RemoveEdge("shipediting_block");
                master.GUI.RemoveEdge("shipediting_furniture");
            }

            if(mode == ShipMode.Movement) {
                updateMovement();
            }
            
            void updateEditing() {
                if(master.GUI.HasEdge("shipediting_block") == false) {
                    master.GUI.AddEdge("shipediting_block", new GUI.EdgeButton("Block", () => placeMode = PlaceMode.Block, GUI.Edge.Bottom, GUI.Direction.Left));
                    master.GUI.AddEdge("shipediting_furniture", new GUI.EdgeButton("Furniture", () => placeMode = PlaceMode.Furniture, GUI.Edge.Bottom, GUI.Direction.Left));
                }

                // If the user left clicks, place a Block or Furniture.
                if(master.Input.MouseLeft.IsDown && isMouseValid(out int shipX, out int shipY)) {

                    // If the place mode is 'Furniture', try to place a furniture.
                    if(placeMode == PlaceMode.Furniture) {
                        // If the place the user clicked has a Block but no Furniture.
                        if(HasBlock(shipX, shipY) && HasFurniture(shipX, shipY) == false)
                            PlaceFurniture(FurnitureDef.Get("cannon"), shipX, shipY);
                    }
                    // If the place mode is 'Block', try to place a block.
                    if(placeMode == PlaceMode.Block) {
                        // If the place that the user clicked is not occupied.
                        if(HasBlock(shipX, shipY) == false)
                            PlaceBlock("wood", shipX, shipY);
                    }
                }
                // If the user right clicks, remove a Block or Furniture.
                if(master.Input.MouseRight.IsDown && isMouseValid(out shipX, out shipY)) {

                    // If the place mode is 'Furniture', try to remove a Furniture.
                    if(placeMode == PlaceMode.Furniture) {
                        if(HasFurniture(shipX, shipY))
                            RemoveFurniture(shipX, shipY);
                    }
                    // If the place mode is 'Block', try to remove a Block.
                    if(placeMode == PlaceMode.Block) {
                        // If the place that the user clicked has a block, and that block is not the Root.
                        if(GetBlock(shipX, shipY) is Block b && b.ID != RootID)
                            RemoveBlock(shipX, shipY);
                    }
                }

                // Return whether or not the user clicked within the ship, and give
                // the position (local to the ship) as out paremters /x/ and /y/.
                bool isMouseValid(out int x, out int y) {
                    var (seaX, seaY) = Sea.ScreenPointToSea(master.Input.MousePosition);
                    (x, y) = SeaPointToShip(seaX, seaY);

                    return x >= 0 && x < Width && y >= 0 && y < Height;
                }
            }

            void updateMovement() {
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
        }

        /// <summary>
        /// Allows the user to control movement of this <see cref="PlayerShip"/>.
        /// </summary>

        /// <summary>
        /// Allows the user to place and remove blocks in this <see cref="PlayerShip"/>.
        /// </summary>
    }
}

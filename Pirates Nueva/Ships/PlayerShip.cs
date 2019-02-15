using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pirates_Nueva
{
    public class PlayerShip : Ship, IFocusable
    {
        enum FocusOption { None, Movement, Editing };
        enum PlaceMode { Block, Furniture };

        private FocusOption focusOption;
        private PlaceMode placeMode;
        private Dir placeDir;

        public PlayerShip(Sea sea, int width, int height) : base(sea, width, height) {  }

        #region IFocusable Implementation
        private bool IsFocusLocked { get; set; }
        bool IFocusable.IsLocked => IsFocusLocked;

        const string FocusMenuID = "playershipfloating";
        void IFocusable.StartFocus(Master master) {
            if(master.GUI.HasMenu(FocusMenuID) == false) { // If there is NOT a floating menu for this ship,
                master.GUI.AddMenu(                        //     create one.
                    FocusMenuID,
                    new UI.FloatingMenu(
                        this, (0, -0.025f), UI.Corner.BottomLeft,
                        new UI.MenuButton("Edit", master.Font, () => focusOption = FocusOption.Editing),
                        new UI.MenuButton("Move", master.Font, () => focusOption = FocusOption.Movement)
                        )
                    );
            }
        }

        void IFocusable.Focus(Master master) {
            if(master.GUI.TryGetMenu(FocusMenuID, out var menu)) { // If there is an options menu:
                if(focusOption == FocusOption.None)                // If an option is NOT selected,
                    menu.Unhide();                                 //     show the menu.
                else                                               // If an option IS selected,
                    menu.Hide();                                   //     hide the menu.
            }

            if(focusOption == FocusOption.Editing) {
                // If there is no ship editing menu, add one.
                if(master.GUI.HasEdge("shipediting_block") == false) {
                    master.GUI.AddEdge("shipediting_quit", new UI.EdgeButton("Quit", master.Font, () => focusOption = FocusOption.None, GUI.Edge.Bottom, GUI.Direction.Left));
                    master.GUI.AddEdge("shipediting_block", new UI.EdgeButton("Block", master.Font, () => placeMode = PlaceMode.Block, GUI.Edge.Bottom, GUI.Direction.Left));
                    master.GUI.AddEdge("shipediting_furniture", new UI.EdgeButton("Furniture", master.Font, () => placeMode = PlaceMode.Furniture, GUI.Edge.Bottom, GUI.Direction.Left));
                }
                updateEditing();
                IsFocusLocked = true; // Lock focus onto this object.
            }
            // If the mode is not 'Editing', remove the associated menu.
            else if(master.GUI.HasEdge("shipediting_block")) {
                master.GUI.RemoveEdges("shipediting_quit", "shipediting_block", "shipediting_furniture");

                IsFocusLocked = false; // Release focus from this object.
            }

            if(focusOption == FocusOption.Movement) {
                IsFocusLocked = true;                             // Lock focus onto this object.
                master.GUI.Tooltip = "Click the new destination"; // Set a tooltip telling the user what to do.

                if(master.Input.MouseLeft.IsDown && !master.GUI.IsMouseOverGUI) {   // When the user clicks:
                    Destination = Sea.ScreenPointToSea(master.Input.MousePosition); //     Set the destination as the click point,
                    focusOption = FocusOption.None;                                 //     unset the focus option,
                    IsFocusLocked = false;                                          //     release focus from this object,
                    master.GUI.Tooltip = "";                                        //     and unset the tooltip.
                }
            }

            void updateEditing() {
                placeDir = (Dir)(((int)placeDir + (int)master.Input.Horizontal.Down + 4) % 4); // Cycle through place directions.

                // If the user left clicks, place a Block or Furniture.
                if(master.Input.MouseLeft.IsDown && isMouseValid(out int shipX, out int shipY)) {

                    // If the place mode is 'Furniture', try to place a furniture.
                    if(placeMode == PlaceMode.Furniture) {
                        // If the place the user clicked has a Block but no Furniture.
                        if(HasBlock(shipX, shipY) && HasFurniture(shipX, shipY) == false)
                            PlaceFurniture(FurnitureDef.Get("cannon"), shipX, shipY, placeDir);
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
                // Also: If the user clicked a GUI element, definitely return false.
                bool isMouseValid(out int x, out int y) {
                    var (seaX, seaY) = Sea.ScreenPointToSea(master.Input.MousePosition);
                    (x, y) = SeaPointToShip(seaX, seaY);

                    return x >= 0 && x < Width && y >= 0 && y < Height && !master.GUI.IsMouseOverGUI;
                }
            }
        }

        void IFocusable.Unfocus(Master master) {
            if(master.GUI.HasMenu(FocusMenuID)) {   // If there IS a floating menu for this ship,
                master.GUI.RemoveMenu(FocusMenuID); //     remove it.
            }
        }
        #endregion
    }
}

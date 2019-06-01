using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pirates_Nueva.Ocean.Agents;

namespace Pirates_Nueva.Ocean
{
    using Toil = Job<Ship, Block>.Toil;
    public class PlayerShip : Ship, IFocusable
    {
        public PlayerShip(Sea sea, int width, int height) : base(sea, width, height) {  }

        protected override void Draw(Master master) {
            base.Draw(master);
            //
            // If we're being focused on,
            // draw a line to the destination.
            if(IsFocused && Destination is PointF dest) {
                var screenCenter = Sea.SeaPointToScreen(Center);
                var screenDest = Sea.SeaPointToScreen(dest);

                master.Renderer.DrawLine(screenCenter, screenDest, UI.Color.Black);
            }
        }

        #region IFocusable Implementation
        protected bool IsFocused { get; private set; }
        bool IFocusable.IsFocused { set => IsFocused = value; }
        IFocusMenuProvider IFocusable.GetProvider() => new FocusProvider(this);

        private sealed class FocusProvider : IFocusMenuProvider
        {
            enum FocusState { None, Movement, Editing };
            enum PlaceMode { None, Block, Furniture, Gunpowder };

            const string MenuID = "playerShipFloating";

            private FocusState state;
            private PlaceMode placeMode;
            private Dir placeDir;

            public bool IsLocked { get; private set; }
            public PlayerShip Ship { get; }

            private UI.FloatingMenu Menu { get; set; }

            public FocusProvider(PlayerShip ship) {
                Ship = ship;
            }
            public void Start(Master master) {
                //
                // Create a GUI menu.
                master.GUI.AddMenu(
                    MenuID,
                    Menu = new UI.FloatingMenu(
                        Ship, (0, -0.025f), UI.Corner.BottomLeft,
                        new UI.MenuButton("Edit", master.Font, () => SetState(FocusState.Editing)),
                        new UI.MenuButton("Move", master.Font, () => SetState(FocusState.Movement))
                        )
                    );
            }
            const string QuitID = "playerShipEditing_quit",
                         BlockID = "playerShipEditing_placeBlock",
                         FurnID = "playerShipEditing_placeFurniture",
                         GunpID = "playerShipEditing_placeGunpowder";
            public void Update(Master master) {
                switch(this.state) {
                    //
                    // Editing the ship's layout.
                    case FocusState.Editing:
                        //
                        // If there's no ship editing menu, add one.
                        if(!master.GUI.HasEdge(QuitID)) {
                            master.GUI.AddEdge(QuitID, new UI.EdgeButton("Quit", master.Font, () => unsetEditing(), UI.Edge.Bottom, UI.Direction.Left));
                            master.GUI.AddEdge(BlockID, new UI.EdgeButton("Block", master.Font, () => placeMode = PlaceMode.Block, UI.Edge.Bottom, UI.Direction.Left));
                            master.GUI.AddEdge(FurnID, new UI.EdgeButton("Furniture", master.Font, () => placeMode = PlaceMode.Furniture, UI.Edge.Bottom, UI.Direction.Left));
                            master.GUI.AddEdge(GunpID, new UI.EdgeButton("Gunpowder", master.Font, () => placeMode = PlaceMode.Gunpowder, UI.Edge.Bottom, UI.Direction.Left));
                        }
                        //
                        // Lock focus & call the editing method.
                        IsLocked = true;
                        updateEditing();

                        void unsetEditing() {
                            //
                            // Remove the ship editing menu.
                            if(master.GUI.HasEdge(QuitID))
                                master.GUI.RemoveEdges(QuitID, BlockID, FurnID, GunpID);
                            //
                            // Release the lock.
                            this.placeMode = PlaceMode.None;
                            IsLocked = false;
                            SetState(FocusState.None);
                        }
                        break;
                    //
                    // Sailing.
                    case FocusState.Movement:
                        IsLocked = true;                             // Lock focus onto this object.
                        master.GUI.Tooltip = "Click the new destination"; // Set a tooltip telling the user what to do.

                        if(master.Input.MouseLeft.IsDown && !master.GUI.IsMouseOverGUI) { // When the user clicks:
                            Ship.Destination = Ship.Sea.MousePosition;                    //     Set the destination as the click point,
                            SetState(FocusState.None);                                    //     unset the focus option,
                            IsLocked = false;                                             //     release focus from this object,
                            master.GUI.Tooltip = "";                                      //     and unset the tooltip.
                        }
                        break;
                }


                void updateEditing() {
                    placeDir = (Dir)(((int)placeDir + (int)master.Input.Horizontal.Down + 4) % 4); // Cycle through place directions.

                    // If the user left clicks, place a Block or Furniture.
                    if(master.Input.MouseLeft.IsDown && isMouseValid(out int shipX, out int shipY)) {

                        // If the place mode is 'Furniture', try to place a furniture.
                        if(placeMode == PlaceMode.Furniture) {
                            // If the place the user clicked has a Block but no Furniture.
                            if(Ship.HasBlock(shipX, shipY) && !Ship.HasFurniture(shipX, shipY))
                                Ship.PlaceFurniture(FurnitureDef.Get("cannon"), shipX, shipY, placeDir);
                        }
                        // If the place mode is 'Block', try to place a block.
                        if(placeMode == PlaceMode.Block) {
                            // If the place that the user clicked is not occupied.
                            if(Ship.HasBlock(shipX, shipY) == false)
                                Ship.CreateJob(
                                    shipX, shipY,
                                    new Toil(
                                        //
                                        // Place a block if next to the job.
                                        action: new PlaceBlock("wood"),
                                        new IsAdjacentTo<Ship, Block>(
                                            new Toil(
                                                //
                                                // Path to the job if it's accessible.
                                                action: new PathToAdjacent<Ship, Block>(),
                                                new IsAccessibleAdj<Ship, Block>()
                                                )
                                            )
                                        )
                                    );
                        }
                        if(placeMode == PlaceMode.Gunpowder) {
                            if(Ship.GetBlockOrNull(shipX, shipY) is Block b && b.Stock is null)
                                Ship.PlaceStock(ItemDef.Get("gunpowder"), shipX, shipY);
                        }
                    }
                    // If the user right clicks, remove a Block or Furniture.
                    if(master.Input.MouseRight.IsDown && isMouseValid(out shipX, out shipY)) {

                        // If the place mode is 'Furniture', try to remove a Furniture.
                        if(placeMode == PlaceMode.Furniture) {
                            if(Ship.HasFurniture(shipX, shipY))
                                Ship.RemoveFurniture(shipX, shipY);
                        }
                        // If the place mode is 'Block', try to remove a Block.
                        if(placeMode == PlaceMode.Block) {
                            // If the place that the user clicked has a block, and that block is not the Root.
                            if(Ship.GetBlockOrNull(shipX, shipY) is Block b && b.ID != RootID)
                                Ship.RemoveBlock(shipX, shipY);
                        }
                    }

                    // Return whether or not the user clicked within the ship, and give
                    // the position (local to the ship) as out paremters /x/ and /y/.
                    // Also: If the user clicked a GUI element, definitely return false.
                    bool isMouseValid(out int x, out int y) {
                        var (seaX, seaY) = Ship.Sea.MousePosition;
                        (x, y) = Ship.SeaPointToShip(seaX, seaY);

                        return Ship.AreIndicesValid(x, y) && !master.GUI.IsMouseOverGUI;
                    }
                }
            }
            public void Close(Master master)
                => master.GUI.RemoveMenu(MenuID);

            /// <summary> Sets the current state to the specified value. </summary>
            private void SetState(FocusState state) {
                this.state = state;
                if(state == FocusState.None)
                    Menu.Unhide();
                else
                    Menu.Hide();
            }
        }
        #endregion
    }
}

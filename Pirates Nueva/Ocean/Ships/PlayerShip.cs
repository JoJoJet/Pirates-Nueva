using Pirates_Nueva.Ocean.Agents;
using Pirates_Nueva.UI;

namespace Pirates_Nueva.Ocean
{
    using Toil = Job<Ship, Block>.Toil;
    public class PlayerShip : Ship, IFocusable
    {
        public PlayerShip(Sea sea, ShipDef def) : base(sea, def) {  }

        protected override void Draw(ILocalDrawer<Sea> drawer) {
            base.Draw(drawer);
            //
            // If we're being focused on,
            // draw a line to the destination.
            if(IsFocused && Destination is PointF dest) {
                drawer.DrawLine(Center, dest, in UI.Color.Black);
            }
        }

        #region IFocusable Implementation
        protected bool IsFocused { get; private set; }
        bool IFocusable.IsFocused { set => IsFocused = value; }
        IFocusMenuProvider IFocusable.GetProvider(Master master) => new FocusProvider(this, master);

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

            private UI.FloatingMenu Menu { get; }

            public FocusProvider(PlayerShip ship, Master master) {
                Ship = ship;
                //
                // Create a GUI menu.
                master.GUI.AddMenu(
                    MenuID,
                    Menu = new FloatingMenu(
                        Ship, (0, -0.025f), Corner.BottomLeft,
                        new Button<GUI.Menu>("Edit", master.Font, () => SetState(FocusState.Editing)),
                        new Button<GUI.Menu>("Move", master.Font, () => SetState(FocusState.Movement))
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
                            master.GUI.AddEdge(QuitID, Edge.Bottom, Direction.Left, new Button<Edge>("Quit", master.Font, () => unsetEditing() ));
                            master.GUI.AddEdge(BlockID, Edge.Bottom, Direction.Left, new Button<Edge>("Block", master.Font, () => placeMode = PlaceMode.Block));
                            master.GUI.AddEdge(FurnID, Edge.Bottom, Direction.Left, new Button<Edge>("Furniture", master.Font, () => placeMode = PlaceMode.Furniture));
                            master.GUI.AddEdge(GunpID, Edge.Bottom, Direction.Left, new Button<Edge>("Gunpowder", master.Font, () => placeMode = PlaceMode.Gunpowder));
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
                                        new IsAdjacentToToil<Ship, Block>(
                                            new Toil(
                                                //
                                                // Path to the job if it's accessible.
                                                action: new PathToToilAdjacent<Ship, Block>(),
                                                new IsAccessibleToToilAdj<Ship, Block>()
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
                                Ship.DestroyBlock(shipX, shipY);
                        }
                    }

                    // Return whether or not the user clicked within the ship, and give
                    // the position (local to the ship) as out paremters /x/ and /y/.
                    // Also: If the user clicked a GUI element, definitely return false.
                    bool isMouseValid(out int x, out int y) {
                        var (seaX, seaY) = Ship.Sea.MousePosition;
                        (x, y) = Ship.Transformer.PointToIndex(seaX, seaY);

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

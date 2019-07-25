using Pirates_Nueva.Ocean.Agents;

namespace Pirates_Nueva.Ocean
{
    /// <summary>
    /// A character that exists on a <see cref="Ocean.Ship"/> and can complete jobs.
    /// </summary>
    internal class ShipAgent : Agent<Ship, Block>
    {
        /// <summary>
        /// The <see cref="Ocean.Ship"/> that contains this <see cref="ShipAgent"/>.
        /// </summary>
        public Ship Ship => Container;

        protected override PointI ScreenTarget => Ship.Sea.SeaPointToScreen(Ship.Transformer.PointFrom(X + 0.5f, Y + 0.5f));

        public ShipAgent(Ship ship, Block floor) : base(ship, floor) {  }

        protected override IFocusMenuProvider GetFocusProvider(Master master)
            => new ShipAgentFocusMenuProvider(this, master);
    }

    internal sealed class ShipAgentFocusMenuProvider : IFocusMenuProvider
    {
        const string MenuID = "agentFocusFloating";

        enum FocusState { None, ChoosePath };

        private FocusState state;

        public bool IsLocked { get; private set; }
        public ShipAgent Agent { get; }
        private UI.FloatingMenu Menu { get; set; }

        public ShipAgentFocusMenuProvider(ShipAgent agent, Master master) {
            Agent = agent;
            master.GUI.AddMenu(
                MenuID,
                Menu = new UI.FloatingMenu(
                    Agent, (0, -0.05f), UI.Corner.BottomLeft,
                    new UI.Text<UI.GUI.Menu>("Agent", master.Font),
                    new UI.Button<UI.GUI.Menu>("Path", master.Font, () => this.state = FocusState.ChoosePath)
                    )
                );
        }

        public void Update(Master master) {
            //
            // Toggle the menu on/off depending on the current state.
            if(this.state == FocusState.None)
                Menu.Unhide();
            else
                Menu.Hide();

            if(this.state == FocusState.ChoosePath) { // If we are currently choosing a path,
                IsLocked = true;                     //     lock the focus onto this agent.

                if(master.Input.MouseLeft.IsDown && !master.GUI.IsMouseOverGUI) {         // If the user clicked, but not on GUI:
                    var (seaX, seaY) = Agent.Ship.Sea.MousePosition;                      //
                    var (shipX, shipY) = Agent.Ship.Transformer.PointToIndex(seaX, seaY); // The point the user clicked,
                                                                                          //     local to the ship.
                    if(Agent.Ship.GetBlockOrNull(shipX, shipY) is Block target) {         // If the spot has a block,
                        Agent.PathTo(target);                                             //     have the agent path to it.
                    }

                    this.state = FocusState.None; // Unset the focus mode,
                    IsLocked = false;            // and release the focus.
                }
            }
        }
        public void Close(Master master)
            => master.GUI.RemoveMenu(MenuID);
    }
}

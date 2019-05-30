using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public Ship Ship => base.Container;

        protected override PointI ScreenTarget => Ship.Sea.SeaPointToScreen(Ship.ShipPointToSea(X + 0.5f, Y + 0.5f));

        public ShipAgent(Ship ship, Block floor) : base(ship, floor) {  }
        
        protected override void Draw(Master master) {
            var tex = master.Resources.LoadTexture("agent");

            (float seaX, float seaY) = Ship.ShipPointToSea(X, Y + 1);
            (int screenX, int screenY) = Ship.Sea.SeaPointToScreen(seaX, seaY);
            master.Renderer.DrawRotated(tex, screenX, screenY, Ship.Sea.PPU, Ship.Sea.PPU, -Ship.Angle, (0, 0));

            (Holding as IDrawable)?.Draw(master);
        }

        protected override IFocusMenuProvider GetFocusProvider() => new ShipAgentFocusMenuProvider(this);
    }

    internal sealed class ShipAgentFocusMenuProvider : IFocusMenuProvider
    {
        const string MenuID = "agentFocusFloating";

        enum FocusState { None, ChoosePath };

        private FocusState state;

        public bool IsLocked { get; private set; }
        public ShipAgent Agent { get; }
        private UI.FloatingMenu Menu { get; set; }

        public ShipAgentFocusMenuProvider(ShipAgent agent) => Agent = agent;

        public void Start(Master master) {
            master.GUI.AddMenu(
                MenuID,
                Menu = new UI.FloatingMenu(
                    Agent, (0, -0.05f), UI.Corner.BottomLeft,
                    new UI.MenuText("Agent", master.Font),
                    new UI.MenuButton("Path", master.Font, () => this.state = FocusState.ChoosePath)
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

                if(master.Input.MouseLeft.IsDown && !master.GUI.IsMouseOverGUI) { // If the user clicked, but not on GUI:
                    var (seaX, seaY) = Agent.Ship.Sea.MousePosition;              //
                    var (shipX, shipY) = Agent.Ship.SeaPointToShip(seaX, seaY);   // The point the user clicked,
                                                                                  //     local to the ship.
                    if(Agent.Ship.GetBlockOrNull(shipX, shipY) is Block target) { // If the spot has a block,
                        Agent.PathTo(target);                                     //     have the agent path to it.
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

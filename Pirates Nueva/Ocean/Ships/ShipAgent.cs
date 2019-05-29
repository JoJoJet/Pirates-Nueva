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
        }

        #region IFocusable Implementation
        enum FocusOption { None, ChoosePath };
        private FocusOption focusMode;

        bool _isFocusLocked;
        protected override bool IsFocusLocked => _isFocusLocked;

        const string FocusMenuID = "agentfocusfloating";
        protected override void StartFocus(Master master) {
            if(master.GUI.HasMenu(FocusMenuID) == false) // If there is no GUI menu for this Agent,
                master.GUI.AddMenu(                      //     create one.
                    FocusMenuID,
                    new UI.FloatingMenu(
                        this, (0, -0.05f), UI.Corner.BottomLeft,
                        new UI.MenuText("Agent", master.Font),
                        new UI.MenuButton("Path", master.Font, () => this.focusMode = FocusOption.ChoosePath)
                    )
                );
        }
        protected override void Focus(Master master) {
            if(master.GUI.TryGetMenu(FocusMenuID, out var menu)) { // If there is a menu:
                if(focusMode == FocusOption.None)                  //     If the focus mode is unset,
                    menu.Unhide();                                 //         unhide the menu.
                else                                               //     If the focus mode IS set.
                    menu.Hide();                                   //         hide the menu.
            }

            if(this.focusMode == FocusOption.ChoosePath) { // If we are currently choosing a path,
                _isFocusLocked = true;                     //     lock the focus onto this agent.

                if(master.Input.MouseLeft.IsDown && !master.GUI.IsMouseOverGUI) { // If the user clicked, but not on GUI:
                    var (seaX, seaY) = Ship.Sea.MousePosition;                    //
                    var (shipX, shipY) = Ship.SeaPointToShip(seaX, seaY);         // The point the user clicked,
                                                                                  //     local to the ship.
                    if(Ship.GetBlockOrNull(shipX, shipY) is Block target) {       // If the spot has a block,
                        PathTo(target);                                           //     have the agent path to it.
                    }

                    this.focusMode = FocusOption.None; // Unset the focus mode,
                    _isFocusLocked = false;            // and release the focus.
                }
            }
        }
        protected override void Unfocus(Master master) {
            if(master.GUI.HasMenu(FocusMenuID))     // If there is a GUI menu for this block,
                master.GUI.RemoveMenu(FocusMenuID); //     remove it.
        }
        #endregion
    }
}

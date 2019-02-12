using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Pirates_Nueva
{
    public class Input : IUpdatable
    {
        private readonly Dictionary<Keys, Button> _keys = new Dictionary<Keys, Button>();

        public Master Master { get; }

        public Point MousePosition { get; private set; }
        public Button MouseLeft { get; private set; } = new Button();
        public Button MouseRight { get; private set; } = new Button();

        /// <summary> The Horizontal input axis. </summary>
        public float Horizontal => (AKey.IsPressed || LeftKey.IsPressed ? -1 : 0) + (DKey.IsPressed || RightKey.IsPressed ? 1 : 0);
        /// <summary> The Vertical input axis. </summary>
        public float Vertical => (WKey.IsPressed || UpKey.IsPressed ? 1 : 0) + (SKey.IsPressed || DownKey.IsPressed ? -1 : 0);

        public Button WKey => GetKey(Keys.W);
        public Button AKey => GetKey(Keys.A);
        public Button SKey => GetKey(Keys.S);
        public Button DKey => GetKey(Keys.D);

        public Button UpKey => GetKey(Keys.Up);
        public Button LeftKey => GetKey(Keys.Left);
        public Button DownKey => GetKey(Keys.Down);
        public Button RightKey => GetKey(Keys.Right);

        public Button LShift => GetKey(Keys.LeftShift);

        internal Input(Master master) {
            Master = master;
        }

        /// <summary>
        /// Get the button associated with the specified key.
        /// </summary>
        public Button GetKey(Keys which) {
            if(this._keys.TryGetValue(which, out Button b)) {
                return b;
            }
            else {
                return new Button();
            }
        }

        void IUpdatable.Update(Master master) {
            var mouse = Mouse.GetState();

            MousePosition = mouse.Position;
            
            MouseLeft = updateButton(MouseLeft, mouse.LeftButton == ButtonState.Pressed);
            MouseRight = updateButton(MouseRight, mouse.RightButton == ButtonState.Pressed);

            var keyboard = Keyboard.GetState();

            // Enter any never-before-pressed keys into the dictionary of pressed keys.
            foreach(var key in keyboard.GetPressedKeys()) {
                if(this._keys.ContainsKey(key) == false)
                    this._keys[key] = new Button();
            }

            // For every key that has previously been pressed (or queried), update its current status.
            var allKeys = this._keys.Keys.ToArray();
            foreach(var key in allKeys) {
                this._keys[key] = updateButton(this._keys[key], keyboard.IsKeyDown(key));
            }

            Button updateButton(Button old, bool isPressed) {
                var newb = new Button();

                (newb as IContract).OldIsPressed = old.IsPressed;
                (newb as IContract).IsPressed = isPressed;

                return newb;
            }
        }

        private interface IContract
        {
            bool IsPressed { set; }
            bool OldIsPressed { set; }
        }

        /// <summary>
        /// An object representing the status of a button or key during this frame.
        /// </summary>
        public class Button : IContract
        {
            /// <summary> Whether or not this <see cref="Button"/> is being pressed this frame. </summary>
            public bool IsPressed { get; private set; }
            /// <summary> Whether or not this is the first frame that this <see cref="Button"/> is being pressed. </summary>
            public bool IsDown => !OldIsPressed && IsPressed;
            /// <summary> Whether or not this is the first frame that this <see cref="Button"/> was released. </summary>
            public bool IsUp => OldIsPressed && !IsPressed;

            private bool OldIsPressed { get; set; }

            bool IContract.IsPressed { set => IsPressed = value; }
            bool IContract.OldIsPressed { set => OldIsPressed = value; }

            internal Button() {  }
        }
    }
}

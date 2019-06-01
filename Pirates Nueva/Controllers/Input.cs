using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
#nullable enable

namespace Pirates_Nueva
{
    public class Input : IUpdatable
    {
        private readonly Dictionary<Keys, Button> _keys = new Dictionary<Keys, Button>();

        public Master Master { get; }

        public PointI MousePosition { get; private set; }
        public Button MouseLeft { get; private set; } = new Button();
        public Button MouseRight { get; private set; } = new Button();
        public ScrollWheel MouseWheel { get; private set; } = new ScrollWheel();

        /// <summary> The Horizontal input axis. </summary>
        public Axis Horizontal => new Axis(AKey, DKey);
        /// <summary> The Vertical input axis. </summary>
        public Axis Vertical => new Axis(WKey, SKey);

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

        void IUpdatable.Update(Master master, Time delta) {
            var mouse = Mouse.GetState();

            MousePosition = mouse.Position;
            
            // Update mouse buttons.
            MouseLeft = updateButton(MouseLeft, mouse.LeftButton == ButtonState.Pressed);
            MouseRight = updateButton(MouseRight, mouse.RightButton == ButtonState.Pressed);
            MouseWheel = updateWheel(MouseWheel, mouse.MiddleButton == ButtonState.Pressed, mouse);

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


            static TButton updateBase<TButton>(TButton old, TButton @new, bool isPressed)
                where TButton : Button, IButtonContract
            {
                @new.SetOldIsPressed(old.IsPressed);
                @new.SetIsPressed(isPressed);

                return @new;
            }

            static Button updateButton(Button old, bool isPressed) => updateBase(old, new Button(), isPressed);

            static ScrollWheel updateWheel(ScrollWheel old, bool isPressed, MouseState mouse) {
                var @new = updateBase(old, new ScrollWheel(), isPressed);

                var con = @new as IScrollContract;
                con.SetOldScrollCumulative(old.ScrollCumulative);
                con.SetScrollCumulative(mouse.ScrollWheelValue);
                
                return @new;
            }
        }

        private interface IButtonContract
        {
            void SetIsPressed(bool value);
            void SetOldIsPressed(bool value);
        }

        /// <summary>
        /// An object representing the status of a button or key during this frame.
        /// </summary>
        public class Button : IButtonContract
        {
            /// <summary> Whether or not this <see cref="Button"/> is being pressed this frame. </summary>
            public bool IsPressed { get; private set; }
            /// <summary> Whether or not this is the first frame that this <see cref="Button"/> is being pressed. </summary>
            public bool IsDown => !OldIsPressed && IsPressed;
            /// <summary> Whether or not this is the first frame that this <see cref="Button"/> was released. </summary>
            public bool IsUp => OldIsPressed && !IsPressed;

            private bool OldIsPressed { get; set; }

            void IButtonContract.SetIsPressed(bool value) => IsPressed = value;
            void IButtonContract.SetOldIsPressed(bool value) => OldIsPressed = value;

            internal Button() {  }
        }

        private interface IScrollContract {
            void SetOldScrollCumulative(float value);
            void SetScrollCumulative(float value);
        }
        /// <summary>
        /// An object representing the status of the mouse scrollwheel during this frame.
        /// </summary>
        public class ScrollWheel : Button, IScrollContract
        {
            /// <summary> Scrolling done during this frame. </summary>
            public float Scroll => ScrollCumulative - OldScrollCumulative;
            /// <summary> Scrolling since the start of the game. </summary>
            public float ScrollCumulative { get; private set; }

            private float OldScrollCumulative { get; set; }

            void IScrollContract.SetOldScrollCumulative(float value) => OldScrollCumulative = value;
            void IScrollContract.SetScrollCumulative(float value) => ScrollCumulative = value;

            internal ScrollWheel() {  }
        }

        /// <summary>
        /// An ojbect representing the status of an input axis during this frame.
        /// </summary>
        public struct Axis
        {
            private Button negative, positive;

            /// <summary> Get the value of this axis for the current frame. </summary>
            public float Value => (negative.IsPressed ? -1 : 0) + (positive.IsPressed ? 1 : 0);
            /// <summary>
            /// Get the value of this axis for the current frame. See <see cref="Button.IsDown"/> documentation.
            /// </summary>
            public float Down => (negative.IsDown ? -1 : 0) + (positive.IsDown ? 1 : 0);

            internal Axis(Button negative, Button positive) {
                this.negative = negative;
                this.positive = positive;
            }

            public static implicit operator float (Axis box) => box.Value;
        }
    }
}

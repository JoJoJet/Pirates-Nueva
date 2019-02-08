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
        public Master Master { get; }

        public Point MousePosition { get; private set; }

        internal Input(Master master) {
            Master = master;
        }

        void IUpdatable.Update(Master master) {
            var mouse = Mouse.GetState();

            MousePosition = mouse.Position;
            }
        }

        private interface IContract
        {
            void SetIsPressed(bool newValue);
            void SetOldIsPressed(bool newValue);
        }

        /// <summary>
        /// An object representing the status of a button or key this frame.
        /// </summary>
        public class Button : IContract
        {
            private bool _isPressed;
            private bool _oldIsPressed;

            /// <summary> Whether or not this <see cref="Button"/> is being pressed this frame. </summary>
            public bool IsPressed => _isPressed;
            /// <summary> Whether or not this is the first frame that this <see cref="Button"/> is being pressed. </summary>
            public bool IsDown => !_oldIsPressed && _isPressed;
            /// <summary> Whether or not this is the first frame that this <see cref="Button"/> was released. </summary>
            public bool IsUp => _oldIsPressed && !_isPressed;

            internal Button() {  }

            void IContract.SetIsPressed(bool newValue) => _isPressed = newValue;
            void IContract.SetOldIsPressed(bool newValue) => _oldIsPressed = newValue;
        }
    }
}

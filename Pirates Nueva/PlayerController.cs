using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pirates_Nueva
{
    /// <summary>
    /// A game object that can be <see cref="Focus(Master)"/>'ed on by <see cref="PlayerController"/>.
    /// </summary>
    public interface IFocusable
    {
        /// <summary> Called when <see cref="PlayerController"/> is focusing on this object. </summary>
        void Focus(Master master);
        /// <summary> Called when <see cref="PlayerController"/> stops focusing on this object. </summary>
        void Unfocus(Master master);
    }
    public class PlayerController : IUpdatable
    {
        public Master Master { get; }

        private Sea Sea { get; }

        internal PlayerController(Master master, Sea sea) {
            Master = master;
            Sea = sea;
        }

        void IUpdatable.Update(Master master) {

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pirates_Nueva
{
    public class Ship : IUpdatable, IDrawable
    {
        private readonly Block[,] blocks;

        public Sea Sea { get; }

        /// <summary> The horizontal length of this <see cref="Ship"/>. </summary>
        public int Width => this.blocks.GetLength(0);
        /// <summary> The vertical length of this <see cref="Ship"/>. </summary>
        public int Height => this.blocks.GetLength(1);

        /// <summary>
        /// Create a ship with specified /width/ and /height/.
        /// </summary>
        public Ship(Sea parent, int width, int height) {
            Sea = parent;

            this.blocks = new Block[width, height];

            // Place the root block.
            // It should be in the exact middle of the Ship.
            PlaceBlock("root", Width/2, Height/2);
        }

        /// <summary>
        /// Get the block at position (/x/, /y/).
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if either index exceeds the bounds of this <see cref="Ship"/>.</exception>
        public Block this[int x, int y] => GetBlock(x, y);

        public void Update(Master master) {
            // If the user clicks either mouse button.
            if(master.Input.MouseLeft.IsDown || master.Input.MouseRight.IsDown) {
                // Find the index of the location that the user clicked.
                var (seaX, seaY) = Sea.ScreenPointToSea(master.Input.MousePosition);
                var (shipX, shipY) = ((int)Math.Floor(seaX), (int)Math.Floor(seaY));

                // If the place the user clicked was within the bounds of the ship.
                if(shipX >= 0 && shipX < Width && shipY >= 0 && shipY < Height) {
                    // If the left mouse button was clicked, place a block.
                    if(master.Input.MouseLeft.IsDown && HasBlock(shipX, shipY) == false)
                        PlaceBlock("wood", shipX, shipY);
                    // If the right mouse button was clicked, remove a block.
                    else if(master.Input.MouseRight.IsDown && HasBlock(shipX, shipY))
                        RemoveBlock(shipX, shipY);
                }
            }
        }

        /// <summary>
        /// Draw this <see cref="Ship"/> onscreen.
        /// </summary>
        public void Draw(Master master) {
            for(int x = 0; x < Width; x++) {
                for(int y = 0; y < Height; y++) {
                    if(this.blocks[x, y] != null) {
                        this[x, y].Draw(master);
                    }
                }
            }
        }

        #region Block Accessor Methods
        /// <summary>
        /// Get the block at position (/x/, /y/).
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if either index exceeds the bounds of this <see cref="Ship"/>.</exception>
        public Block GetBlock(int x, int y) {
            try {
                ValidateIndices($"{nameof(Ship)}.{nameof(GetBlock)}()", x, y);
            }
            catch(ArgumentOutOfRangeException) {
                throw;
            }
            
            return UnsafeGetBlock(x, y);
        }
        /// <summary>
        /// Place a block of type /id/ at position (/x/, /y/).
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if either index exceeds the bounds of this <see cref="Ship"/>.</exception>
        /// <exception cref="InvalidOperationException">Thrown if there is already a <see cref="Block"/> at /x/, /y/.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if there is no <see cref="BlockDef"/> identified by /id/.</exception>
        /// <exception cref="InvalidCastException">Thrown if the <see cref="Def"/> identified by /id/ is not a <see cref="BlockDef"/>.</exception>
        public Block PlaceBlock(string id, int x, int y) {
            try {
                ValidateIndices($"{nameof(Ship)}.{nameof(PlaceBlock)}()", x, y);
            }
            catch(ArgumentOutOfRangeException) {
                throw;
            }
            
            if(UnsafeGetBlock(x, y) == null)
                return this.blocks[x, y] = new Block(this, BlockDef.Get(id), x, y);
            else
                throw new InvalidOperationException(
                    $"{nameof(Ship)}.{nameof(PlaceBlock)}(): There is already a {nameof(Block)} at position ({x}, {y})!"
                    );
        }

        /// <summary>
        /// Whether or not there is a block at position (/x/, /y/).
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if either index exceeds the bounds of this <see cref="Ship"/>.</exception>
        public bool HasBlock(int x, int y) {
            try {
                ValidateIndices($"{nameof(Ship)}.{nameof(HasBlock)}", x, y);
            }
            catch(ArgumentOutOfRangeException) {
                throw;
            }
            
            return UnsafeGetBlock(x, y) != null;
        }

        /// <summary>
        /// Remove the block at position (/x/, /y/).
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if either index exceeds the bounds of this <see cref="Ship"/>.</exception>
        /// <exception cref="InvalidOperationException">Thrown if there is no <see cref="Block"/> at /x/, /y/.</exception>
        public Block RemoveBlock(int x, int y) {
            try {
                ValidateIndices($"{nameof(Ship)}.{nameof(RemoveBlock)}()", x, y);
            }
            catch(ArgumentOutOfRangeException) {
                throw;
            }
            
            if(UnsafeGetBlock(x, y) is Block b) {
                this.blocks[x, y] = null;
                return b;
            }
            else {
                throw new InvalidOperationException(
                    $"{nameof(Ship)}.{nameof(RemoveBlock)}(): There is no {nameof(Block)} at position ({x}, {y})!"
                    );
            }
        }

        /// <summary> Get the <see cref="Block"/> at position (/x/, /y/), without checking the indices. </summary>
        private Block UnsafeGetBlock(int x, int y) => this.blocks[x, y];

        /// <summary> Throw an exception if either index is out of range. </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if either index exceeds the bounds of this <see cref="Ship"/>.</exception>
        private void ValidateIndices(string methodName, int x, int y) {
            if(x < 0 || x >= Width)
                throw new ArgumentOutOfRangeException(
                    nameof(x),
                    $@"{methodName}: Argument must be on the interval [0, {Width}). Its value is ""{x}""!"
                    );
            if(y < 0 || y >= Height)
                throw new ArgumentOutOfRangeException(
                    nameof(y),
                    $@"{methodName}: Argument must be on the interval [0, {Height}). Its value is ""{y}""!"
                    );
        }
        #endregion
    }
}

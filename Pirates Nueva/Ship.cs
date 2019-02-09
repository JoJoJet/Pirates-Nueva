using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pirates_Nueva
{
    public class Ship : IUpdatable, IDrawable
    {
        protected const string RootID = "root";

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
            PlaceBlock(RootID, Width/2, Height/2);
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
                var (shipX, shipY) = SeaPointToShip(seaX, seaY);

                // If the place the user clicked was within the bounds of the ship.
                if(shipX >= 0 && shipX < Width && shipY >= 0 && shipY < Height) {
                    // If the left mouse button was clicked, place a block.
                    if(master.Input.MouseLeft.IsDown && HasBlock(shipX, shipY) == false)
                        PlaceBlock("wood", shipX, shipY);
                    // If the right mouse button was clicked, remove a block, unless it is the root block.
                    else if(master.Input.MouseRight.IsDown && GetBlock(shipX, shipY) is Block b && b.ID != RootID)
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

        #region Space Transformation
        /// <summary>
        /// Transform the input <see cref="PointF"/> from <see cref="Pirates_Nueva.Sea"/> space
        /// to a <see cref="PointI"/> representing indices within this <see cref="Ship"/>.
        /// </summary>
        /// <param name="seaPoint">A pair of coordinates local to the <see cref="Pirates_Nueva.Sea"/></param>
        public PointI SeaPointToShip(PointF seaPoint) => SeaPointToShip(seaPoint.X, seaPoint.Y);
        /// <summary>
        /// Transform the input coordinates from <see cref="Pirates_Nueva.Sea"/>
        /// space to a pair of indices within to this <see cref="Ship"/>
        /// </summary>
        /// <param name="x">The x coordinate local to the <see cref="Pirates_Nueva.Sea"/>.</param>
        /// <param name="y">The y coordinate local to the <see cref="Pirates_Nueva.Sea"/>.</param>
        internal (int x, int y) SeaPointToShip(float x, float y) => ((int)Math.Floor(x), (int)Math.Floor(y));

        /// <summary>
        /// Transform the input coordinates from a <see cref="PointI"/> local to this <see cref="Ship"/>
        /// into a <see cref="PointF"/> local to the <see cref="Pirates_Nueva.Sea"/>.
        /// <para />
        /// NOTE: Is not necessarily the exact inverse of <see cref="SeaPointToShip(PointF)"/>, as that method
        /// has an element of rounding.
        /// </summary>
        /// <param name="shipPoint">A pair of indices within this <see cref="Ship"/>.</param>
        public PointF ShipPointToSea(PointI shipPoint) => ShipPointToSea(shipPoint.X, shipPoint.Y);
        /// <summary>
        /// Transform the input coordinates from indices local to this <see cref="Ship"/> into
        /// a pair of coordinates local to the <see cref="Pirates_Nueva.Sea"/>.
        /// <para />
        /// NOTE: Is not necessarily the exact inverse of <see cref="SeaPointToShip(float, float)"/>, as that method
        /// has an element of rounding.
        /// </summary>
        /// <param name="x">The x index within this <see cref="Ship"/>.</param>
        /// <param name="y">The y index within this <see cref="Ship"/>.</param>
        internal (float x, float y) ShipPointToSea(int x, int y) => (x + 0.5f, y + 0.5f);
        #endregion

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
        /// <exception cref="InvalidOperationException">Thrown if there is no <see cref="Block"/> at /x/, /y/, or if that block is the root.</exception>
        public Block RemoveBlock(int x, int y) {
            try {
                ValidateIndices($"{nameof(Ship)}.{nameof(RemoveBlock)}()", x, y);
            }
            catch(ArgumentOutOfRangeException) {
                throw;
            }
            
            if(UnsafeGetBlock(x, y) is Block b) {
                if(b.ID != RootID) {
                    this.blocks[x, y] = null;
                    return b;
                }
                else {
                    throw new InvalidOperationException($"{nameof(Ship)}.{nameof(RemoveBlock)}(): You can't remove the Root block! at ({x}, {y})");
                }
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

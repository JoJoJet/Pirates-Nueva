using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pirates_Nueva
{
    public abstract class Ship : IUpdatable, IDrawable
    {
        protected const string RootID = "root";

        private readonly Block[,] blocks;

        /// <summary>
        /// A delegate that allows this class to set the <see cref="Block.Furniture"/> property, even though that is a private property.
        /// </summary>
        internal static Func<Block, Furniture, Furniture> SetBlockFurniture { private protected get; set; }

        public Sea Sea { get; }

        /// <summary> The horizontal length of this <see cref="Ship"/>. </summary>
        public int Width => this.blocks.GetLength(0);
        /// <summary> The vertical length of this <see cref="Ship"/>. </summary>
        public int Height => this.blocks.GetLength(1);
        
        /// <summary> The X coordinate of the <see cref="Sea"/>-space center of this <see cref="Ship"/>. </summary>
        public float CenterX { get; protected set; }
        /// <summary> The Y coordinate of the <see cref="Sea"/>-space center of this <see cref="Ship"/>. </summary>
        public float CenterY { get; protected set; }
        /// <summary>
        /// The <see cref="Pirates_Nueva.Sea"/>-space center of this <see cref="Ship"/>.
        /// </summary>
        public PointF Center {
            get => (CenterX, CenterY);
            protected set => (CenterX, CenterY) = value;
        }

        /// <summary>
        /// This <see cref="Ship"/>'s rotation. 0 is pointing directly rightwards, rotation is counter-clockwise.
        /// </summary>
        public Angle Angle { get; protected set; }

        /// <summary>
        /// The direction from this <see cref="Ship"/>'s center to its right edge, <see cref="Pirates_Nueva.Sea"/>-space.
        /// </summary>
        public PointF Right => PointF.Rotate((1, 0), Angle);

        /// <summary> The X index of this <see cref="Ship"/>'s root <see cref="Block"/>. </summary>
        private int RootX => Width/2;
        /// <summary> The Y index of this <see cref="Ship"/>'s root <see cref="Block"/>. </summary>
        private int RootY => Height/2;
        /// <summary>
        /// The local indices of this <see cref="Ship"/>'s root <see cref="Block"/>.
        /// </summary>
        private PointI RootIndex => (RootX, RootY);

        /// <summary>
        /// Create a ship with specified /width/ and /height/.
        /// </summary>
        public Ship(Sea parent, int width, int height) {
            Sea = parent;

            this.blocks = new Block[width, height];

            Center = (PointF)RootIndex;

            // Place the root block.
            // It should be in the exact middle of the Ship.
            Block root = PlaceBlock(RootID, RootX, RootY);
            SetBlockFurniture(root, new Furniture(FurnitureDef.Get("cannon"), root));
        }

        /// <summary>
        /// Get the block at index (/x/, /y/).
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if either index exceeds the bounds of this <see cref="Ship"/>.</exception>
        public Block this[int x, int y] => GetBlock(x, y);

        public abstract void Update(Master master);

        /// <summary>
        /// Draw this <see cref="Ship"/> onscreen.
        /// </summary>
        public virtual void Draw(Master master) {
            // Draw each block.
            for(int x = 0; x < Width; x++) {
                for(int y = 0; y < Height; y++) {
                    if(GetBlock(x, y) is Block b)
                        b.Draw(master);
                }
            }

            // Draw each Furniture.
            for(int x = 0; x < Width; x++) {
                for(int y = 0; y < Height; y++) {
                    if(GetBlock(x, y) is Block b && b.Furniture is Furniture f)
                        f.Draw(master);
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
        internal (int x, int y) SeaPointToShip(float x, float y) {
            // Coordinates in Sea-space.
            var (seaX, seaY) = (x, y);

            // Rotated coordinates local to the ship's root
            var (rotX, rotY) = (x - CenterX, y - CenterY); // Translate the sea coords to be centered around the root block.
            
            // Flat coordinates local to the ship's root.
            var (shipX, shipY) = PointF.Rotate((rotX, rotY), -Angle);

            // Indices within the ship
            var (indX, indY) = (shipX + RootX + 0.5f, shipY + RootY + 0.5f); // Translate ship coords into indices centered on ship's bottom left corner.

            return ((int)Math.Floor(indX), (int)Math.Floor(indY)); // Floor the indices into integers, and then return them.
        }

        /// <summary>
        /// Transform the input coordinates from a <see cref="PointI"/> local to this <see cref="Ship"/>
        /// into a <see cref="PointF"/> local to the <see cref="Pirates_Nueva.Sea"/>.
        /// <para />
        /// NOTE: Is not necessarily the exact inverse of <see cref="SeaPointToShip(PointF)"/>, as that method
        /// has an element of rounding.
        /// </summary>
        /// <param name="shipPoint">A pair of coordinates within this <see cref="Ship"/>.</param>
        public PointF ShipPointToSea(PointF shipPoint) => ShipPointToSea(shipPoint.X, shipPoint.Y);
        /// <summary>
        /// Transform the input coordinates from coords local to this <see cref="Ship"/> into
        /// a pair of coordinates local to the <see cref="Pirates_Nueva.Sea"/>.
        /// <para />
        /// NOTE: Is not necessarily the exact inverse of <see cref="SeaPointToShip(float, float)"/>, as that method
        /// has an element of rounding.
        /// </summary>
        /// <param name="x">The x coordinate within this <see cref="Ship"/>.</param>
        /// <param name="y">The y coordinate within this <see cref="Ship"/>.</param>
        internal (float x, float y) ShipPointToSea(float x, float y) {
            // Coordinates within the ship.
            var (indX, indY) = (x, y);
            
            // Flat coordinates local to the ship's root.
            var (shipX, shipY) = (indX - RootX - 0.5f, indY - RootY - 0.5f);  // Translate the input coords to be centered around the root block.

            // Rotated coordinate's local to the ship's root.
            var (rotX, rotY) = PointF.Rotate((shipX, shipY), Angle); // Rotate the ship indices by the ship's angle.

            // Coordinates in Sea-space.
            var (seaX, seaY) = (CenterX + rotX, CenterY + rotY); // Add the sea-space coords of the ship's center to the local coords.

            return (seaX, seaY); // Return the Sea-space coordinates.
        }
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
            
            return unsafeGetBlock(x, y);
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

            return unsafeGetBlock(x, y) != null;
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
            
            if(unsafeGetBlock(x, y) == null)
                // If /x/, /y/ is empty, place a new Block there, and then return it.
                return this.blocks[x, y] = new Block(this, BlockDef.Get(id), x, y);
            else
                // Throw an InvalidOperationException if there is already a Block at /x/, /y/.
                throw new InvalidOperationException(
                    $"{nameof(Ship)}.{nameof(PlaceBlock)}(): There is already a {nameof(Block)} at position ({x}, {y})!"
                    );
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
            
            if(unsafeGetBlock(x, y) is Block b) {
                // If there is a Block at /x/, /y/, remove it, and then return it.
                this.blocks[x, y] = null;
                return b;
            }
            else {
                // Throw an InvalidOperationException if there is no Block to remove at /x/, /y/.
                throw new InvalidOperationException(
                    $"{nameof(Ship)}.{nameof(RemoveBlock)}(): There is no {nameof(Block)} at position ({x}, {y})!"
                    );
            }
        }
        #endregion

        #region Furniture Accessor Methods
        /// <summary>
        /// Place a <see cref="Furniture"/>, with <see cref="Def"/> /def/, at index /x/, /y/.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if either index exceeds the bounds of this <see cref="ship"/>.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if there is no <see cref="Block"/> at /x/, /y/, or if there is already a <see cref="Furniture"/> there.
        /// </exception>
        public Furniture PlaceFurniture(FurnitureDef def, int x, int y) {
            try {
                ValidateIndices($"{nameof(Ship)}.{nameof(PlaceFurniture)}()", x, y);
            }
            catch(ArgumentOutOfRangeException) {
                throw;
            }

            if(unsafeGetBlock(x, y) is Block b) {
                if(b.Furniture == null)
                    // If there is an empty Block to place it on, place a Furniture and return it.
                    return SetBlockFurniture(b, new Furniture(def, b));
                else
                    // Throw an InvalidOperationException if there is already a Furniture at /x/, /y/.
                    throw new InvalidOperationException(
                        $"{nameof(Ship)}.{nameof(PlaceFurniture)}(): There is already a {nameof(Furniture)} at index ({x}, {y})!"
                        );
            }
            // Throw an InvalidOperationException if there is no Block at /x/, /y/.
            else {
                throw new InvalidOperationException(
                    $"{nameof(Ship)}.{nameof(PlaceFurniture)}(): There is no {nameof(Block)} at index ({x}, {y})!"
                    );
            }
        }
        #endregion

        #region Private Methods
        /// <summary> Get the <see cref="Block"/> at position (/x/, /y/), without checking the indices. </summary>
        private Block unsafeGetBlock(int x, int y) => this.blocks[x, y];

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

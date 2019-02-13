using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pirates_Nueva
{
    public abstract class Ship : Entity, UI.IScreenSpaceTarget, IUpdatable, IDrawable, IFocusableParent
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

        /// <summary> A box drawn around this <see cref="Ship"/>, used for approximating collision. </summary>
        protected override BoundingBox Bounds {
            get {
                var lb = ShipPointToSea(0, 0);
                var lt = ShipPointToSea(0, Height-1);
                var rt = ShipPointToSea(Width-1, Height-1);
                var rb = ShipPointToSea(Width-1, 0);

                return new BoundingBox(
                    min(lb.x, lt.x, rt.x, rb.x), min(lb.y, lt.y, rt.y, rb.y),
                    max(lb.x, lt.x, rt.x, rb.x), max(lb.y, lt.y, rt.y, rb.y)
                    );

                float min(float f1, float f2, float f3, float f4) => Math.Min(Math.Min(f1, f2), Math.Min(f3, f4));
                float max(float f1, float f2, float f3, float f4) => Math.Max(Math.Max(f1, f2), Math.Max(f3, f4));
            }
        }

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

            Center = (PointF)RootIndex + (0.5f, 0.5f);
            
            Block root = PlaceBlock(RootID, RootX, RootY); // Place the root block.
        }

        /// <summary>
        /// Get the block at index (/x/, /y/).
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if either index exceeds the bounds of this <see cref="Ship"/>.</exception>
        public Block this[int x, int y] => GetBlock(x, y);

        protected override bool IsCollidingPrecise(PointF point) {
            var (shipX, shipY) = SeaPointToShip(point); // Convert the point to an index in this ship.

            if(shipX >= 0 && shipX < Width && shipY >= 0 && shipY < Height) // If the index is valid,
                return HasBlock(shipX, shipY);                              //     return whether or not there is a block there.
            else                                                            // Otherwise:
                return false;                                               //     just return false.
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
            var (shipX, shipY) = (indX - RootX - 0.5f, indY - RootY - 0.5f);  // Translate the input coords to be
                                                                              //     centered around the root block.
            // Rotated coordinate's local to the ship's root.
            var (rotX, rotY) = PointF.Rotate((shipX, shipY), Angle); // Rotate the ship indices by the ship's angle.

            // Coordinates in Sea-space.
            var (seaX, seaY) = (CenterX + rotX, CenterY + rotY); // Add the sea-space coords of the ship's center to the local coords.

            return (seaX, seaY); // Return the Sea-space coordinates.
        }
        #endregion

        #region Block Accessor Methods
        /// <summary>
        /// Get the <see cref="Block"/> at position (/x/, /y/).
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if either index exceeds the bounds of this <see cref="Ship"/>.</exception>
        public Block GetBlock(int x, int y) {
            ValidateIndices(nameof(GetBlock), x, y);
            
            return unsafeGetBlock(x, y);
        }
        /// <summary>
        /// Whether or not there is a <see cref="Block"/> at position (/x/, /y/).
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if either index exceeds the bounds of this <see cref="Ship"/>.</exception>
        public bool HasBlock(int x, int y) {
            ValidateIndices(nameof(HasBlock), x, y);

            return unsafeGetBlock(x, y) != null;
        }

        /// <summary>
        /// Place a <see cref="Block"/> of type /id/ at position (/x/, /y/).
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if either index exceeds the bounds of this <see cref="Ship"/>.</exception>
        /// <exception cref="InvalidOperationException">Thrown if there is already a <see cref="Block"/> at /x/, /y/.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if there is no <see cref="BlockDef"/> identified by /id/.</exception>
        /// <exception cref="InvalidCastException">Thrown if the <see cref="Def"/> identified by /id/ is not a <see cref="BlockDef"/>.</exception>
        public Block PlaceBlock(string id, int x, int y) {
            ValidateIndices(nameof(PlaceBlock), x, y);
            
            if(unsafeGetBlock(x, y) == null)                                        // If there is NOT a Block at /x/, /y/,
                return this.blocks[x, y] = new Block(this, BlockDef.Get(id), x, y); //     place a Block there and return it.
            else                                                                    // If there IS a Block at /x/, /y/,
                throw new InvalidOperationException(                                //     throw an InvalidOperationException.
                    $"{nameof(Ship)}.{nameof(PlaceBlock)}(): There is already a {nameof(Block)} at position ({x}, {y})!"
                    );
        }

        /// <summary>
        /// Remove the block at position (/x/, /y/).
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if either index exceeds the bounds of this <see cref="Ship"/>.</exception>
        /// <exception cref="InvalidOperationException">Thrown if there is no <see cref="Block"/> at /x/, /y/.</exception>
        public Block RemoveBlock(int x, int y) {
            ValidateIndices(nameof(RemoveBlock), x, y);
            
            if(unsafeGetBlock(x, y) is Block b) { // If there is a Block at /x/, /y/,
                this.blocks[x, y] = null;         //     remove it,
                return b;                         //     and then return it.
            }
            else {                                   // If there is no Block at /x/, /y/,
                throw new InvalidOperationException( //    throw an InvalidOperationException.
                    $"{nameof(Ship)}.{nameof(RemoveBlock)}(): There is no {nameof(Block)} at position ({x}, {y})!"
                    );
            }
        }
        #endregion

        #region Furniture Accessor Methods
        /// <summary>
        /// Get the <see cref="Furniture"/> at index /x/, /y/.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if either index exceeds the bounds of this <see cref="Ship"/>.</exception>
        public Furniture GetFurniture(int x, int y) {
            ValidateIndices(nameof(GetFurniture), x, y);

            return unsafeGetBlock(x, y)?.Furniture;
        }
        /// <summary>
        /// Whether or not there is a <see cref="Furniture"/> at position (/x/, /y/).
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if either index exceeds the bounds of this <see cref="Ship"/>.</exception>
        public bool HasFurniture(int x, int y) {
            ValidateIndices(nameof(HasFurniture), x, y);

            return unsafeGetBlock(x, y)?.Furniture != null; // Return true if there is a Block at /x/, /y/ AND that block has a Furniture.
        }

        /// <summary>
        /// Place a <see cref="Furniture"/>, with <see cref="Def"/> /def/, at index /x/, /y/.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if either index exceeds the bounds of this <see cref="ship"/>.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if there is no <see cref="Block"/> at /x/, /y/, or if there is already a <see cref="Furniture"/> there.
        /// </exception>
        public Furniture PlaceFurniture(FurnitureDef def, int x, int y) {
            ValidateIndices(nameof(PlaceFurniture), x, y);
            
            if(unsafeGetBlock(x, y) is Block b) {                       // If there is a block at /x/, /y/:
                if(b.Furniture == null)                                 //     If the block is empty,
                    return SetBlockFurniture(b, new Furniture(def, b)); //         place a Furniture there and return it.
                else                                                    //     If the block is occupied,
                    throw new InvalidOperationException(                //         throw an InvalidOperationException.
                        $"{nameof(Ship)}.{nameof(PlaceFurniture)}(): There is already a {nameof(Furniture)} at index ({x}, {y})!"
                        );
            }
            else {                                                      // If there is no block at /x/, /y/,
                throw new InvalidOperationException(                    //     throw an InvalidOperationException.
                    $"{nameof(Ship)}.{nameof(PlaceFurniture)}(): There is no {nameof(Block)} at index ({x}, {y})!"
                    );
            }
        }

        /// <summary>
        /// Remove the <see cref="Furniture"/> at index /x/, /y/.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if either index exceeds the bounds of this <see cref="Ship"/>.</exception>
        /// <exception cref="InvalidOperationException">Thrown if there is no <see cref="Furniture"/> at /x/, /y/.</exception>
        public Furniture RemoveFurniture(int x, int y) {
            ValidateIndices(nameof(RemoveFurniture), x, y);
            
            if(unsafeGetBlock(x, y)?.Furniture is Furniture f) { // If there is a furniture at /x/, /y/,
                SetBlockFurniture(f.Floor, null);                //     remove it,
                return f;                                        //     and then return it.
            }
            else {                                   // If there is no furniture at /x/, /y/,
                throw new InvalidOperationException( //     throw an InvalidOperationException.
                    $"{nameof(Ship)}.{nameof(RemoveFurniture)}(): There is no {nameof(Furniture)} at index ({x}, {y})!"
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
                    $@"{nameof(Ship)}.{methodName}(): Argument must be on the interval [0, {Width}). Its value is ""{x}""!"
                    );
            if(y < 0 || y >= Height)
                throw new ArgumentOutOfRangeException(
                    nameof(y),
                    $@"{nameof(Ship)}.{methodName}(): Argument must be on the interval [0, {Height}). Its value is ""{y}""!"
                    );
        }
        #endregion

        #region IUpdatable Implementation
        void IUpdatable.Update(Master master) => Update(master);
        protected abstract void Update(Master master);
        #endregion

        #region IDrawable Implementation
        void IDrawable.Draw(Master master) => Draw(master);
        /// <summary>
        /// Draw this <see cref="Ship"/> onscreen.
        /// </summary>
        protected virtual void Draw(Master master) {
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
                    if(GetFurniture(x, y) is Furniture f)
                        f.Draw(master);
                }
            }
        }
        #endregion

        #region IScreenSpaceTarget Implementation
        int UI.IScreenSpaceTarget.X => Sea.SeaPointToScreen(Center).X;
        int UI.IScreenSpaceTarget.Y => Sea.SeaPointToScreen(Center).Y;
        #endregion

        #region IFocusableParent Implementation
        List<IFocusable> IFocusableParent.GetFocusable(PointF seaPoint) {
            var focusable = new List<IFocusable>();

            var (shipX, shipY) = SeaPointToShip(seaPoint);
            if(GetFurniture(shipX, shipY) is Furniture f) {
                focusable.Add(f);
            }
            if(GetBlock(shipX, shipY) is Block b) {
                focusable.Add(b);
            }

            return focusable;
        }
        #endregion
    }
}

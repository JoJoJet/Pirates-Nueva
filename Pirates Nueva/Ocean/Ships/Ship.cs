using System;
using System.Collections.Generic;
using Pirates_Nueva.Path;
using Pirates_Nueva.Ocean.Agents;

namespace Pirates_Nueva.Ocean
{
    using Stock = Stock<Ship, Block>;
    public abstract class Ship : Entity, IAgentContainer<Ship, Block>, IGraph<Block>, IUpdatable, IDrawable<Sea>, IFocusableParent, UI.IScreenSpaceTarget
    {
        protected const string RootID = "root";

        private readonly Block?[,] blocks;
        private readonly List<Agent<Ship, Block>> agents = new List<Agent<Ship, Block>>();
        
        private readonly List<Job<Ship, Block>> jobs = new List<Job<Ship, Block>>();

        /// <summary>
        /// A delegate that allows this class to set the <see cref="Block.Furniture"/> property, even though that is a private property.
        /// </summary>
        internal static Func<Block, Furniture?, Furniture?> SetBlockFurniture { private protected get; set; }

        public ShipDef Def { get; }

        /// <summary> The horizontal length of this <see cref="Ship"/>. </summary>
        public int Width => Def.Width;
        /// <summary> The vertical length of this <see cref="Ship"/>. </summary>
        public int Height => Def.Height;

        /// <summary> The point that this <see cref="Ship"/> is moving towards. </summary>
        public PointF? Destination { get; protected set; }

        /// <summary>
        /// This <see cref="Ship"/>'s rotation. 0 is pointing directly rightwards, rotation is counter-clockwise.
        /// </summary>
        public Angle Angle { get; protected set; }

        /// <summary>
        /// The direction from this <see cref="Ship"/>'s center to its right edge, <see cref="Sea"/>-space.
        /// </summary>
        public Vector Right => Angle.Vector;

        /// <summary> The X index of this <see cref="Ship"/>'s root <see cref="Block"/>. </summary>
        private int RootX => RootIndex.X;
        /// <summary> The Y index of this <see cref="Ship"/>'s root <see cref="Block"/>. </summary>
        private int RootY => RootIndex.Y;
        /// <summary>
        /// The local indices of this <see cref="Ship"/>'s root <see cref="Block"/>.
        /// </summary>
        private PointI RootIndex => Def.RootIndex;

        /// <summary>
        /// Create a ship with specified /width/ and /height/.
        /// </summary>
        public Ship(Sea sea, ShipDef def) : base(sea) {
            Def = def;

            Center = (PointF)RootIndex + (0.5f, 0.5f);
            //
            // Construct the default shape of this ship def.
            this.blocks = new Block[Width, Height];
            foreach(var block in def.DefaultShape) {
                PlaceBlock(BlockDef.Get(block.ID), block.X, block.Y);
            }

            AddAgent(RootX, RootY); // Add an agent to the center.
        }

        /// <summary>
        /// Gets the block at index (/x/, /y/).
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if either index exceeds the bounds of this <see cref="Ship"/>.</exception>
        public Block? this[int x, int y] {
            get {
                ValidateIndices("Indexer", x, y);
                return unsafeGetBlock(x, y);
            }
        }
        
        /// <summary> A box drawn around this <see cref="Ship"/>, used for approximating collision. </summary>
        protected override BoundingBox GetBounds() {
            // Find the left-, right-, bottom-, and top-most extents of blocks in this ship.
            var (left, bottom, right, top) = (Width, Height, 0, 0);
            for(int x = 0; x < Width; x++) {
                for(int y = 0; y < Height; y++) {
                    if(HasBlock(x, y)) {
                        left = Math.Min(left, x);
                        right = Math.Max(right, x+1);
                        bottom = Math.Min(bottom, y);
                        top = Math.Max(top, y+1);
                    }
                }
            }

            // Transform those extents into sea-space.
            var lb = ShipPointToSea(left, bottom);
            var lt = ShipPointToSea(left, top);
            var rt = ShipPointToSea(right, top);
            var rb = ShipPointToSea(right, bottom);

            // Return a bounding box eveloping all four points above.
            return new BoundingBox(
                min(lb.x, lt.x, rt.x, rb.x), min(lb.y, lt.y, rt.y, rb.y),
                max(lb.x, lt.x, rt.x, rb.x), max(lb.y, lt.y, rt.y, rb.y)
                );

            float min(float f1, float f2, float f3, float f4) => Math.Min(Math.Min(f1, f2), Math.Min(f3, f4)); // Find the min of 4 values
            float max(float f1, float f2, float f3, float f4) => Math.Max(Math.Max(f1, f2), Math.Max(f3, f4)); // Find the max of 4 values
        }

        protected override bool IsCollidingPrecise(PointF point) {
            //
            // Convert the point to an index on the ship,
            // and return whether or not there is a block there.
            var (shipX, shipY) = SeaPointToShip(point);
            return HasBlock(shipX, shipY);
        }

        #region Space Transformation
        /// <summary>
        /// Transform the input <see cref="PointF"/> from <see cref="Sea"/> space
        /// to a <see cref="PointI"/> representing indices within this <see cref="Ship"/>.
        /// </summary>
        /// <param name="seaPoint">A pair of coordinates local to the <see cref="Sea"/></param>
        public PointI SeaPointToShip(PointF seaPoint) => SeaPointToShip(seaPoint.X, seaPoint.Y);
        /// <summary>
        /// Transform the input coordinates from <see cref="Sea"/>
        /// space to a pair of indices within to this <see cref="Ship"/>
        /// </summary>
        /// <param name="x">The x coordinate local to the <see cref="Sea"/>.</param>
        /// <param name="y">The y coordinate local to the <see cref="Sea"/>.</param>
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
        /// Transforms the input coordinates from a <see cref="PointI"/> local to this <see cref="Ship"/>
        /// into a <see cref="PointF"/> local to the <see cref="Sea"/>.
        /// <para />
        /// NOTE: Is not necessarily the exact inverse of <see cref="SeaPointToShip(PointF)"/>, as that method
        /// has an element of rounding.
        /// </summary>
        /// <param name="shipPoint">A pair of coordinates within this <see cref="Ship"/>.</param>
        public PointF ShipPointToSea(PointF shipPoint) => ShipPointToSea(shipPoint.X, shipPoint.Y);
        /// <summary>
        /// Transforms the input coordinates from coords local to this <see cref="Ship"/> into
        /// a pair of coordinates local to the <see cref="Sea"/>.
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
        
        /// <summary>
        /// Whether or not the specified indices are within the bounds of this ship.
        /// </summary>
        public bool AreIndicesValid(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;
        #endregion

        #region Block Accessor Methods
        /// <summary>
        /// Gets the <see cref="Block"/> at indices (/x/, /y/), if it exists.
        /// </summary>
        public bool TryGetBlock(int x, int y, out Block block) {
            if(AreIndicesValid(x, y) && unsafeGetBlock(x, y) is Block b) {
                block = b;
                return true;
            }
            else {
                //
                // We're putting `null` into a non-nullable reference type.
                // This seems bad, but as long as the 'try' pattern is used
                // correctly, users will never experience any null-reference-exceptions.
                block = null!;
                return false;
            }
        }
        /// <summary>
        /// Gets the <see cref="Block"/> at indices (/x/, /y/), or <see cref="null"/> if it does not exist.
        /// </summary>
        public Block? GetBlockOrNull(int x, int y) {
            if(AreIndicesValid(x, y))
                return unsafeGetBlock(x, y);
            else
                return null;
        }
        /// <summary>
        /// Returns whether or not there is a <see cref="Block"/> at position (/x/, /y/).
        /// </summary>
        public bool HasBlock(int x, int y)
            => AreIndicesValid(x, y) && unsafeGetBlock(x, y) != null;


        /// <summary>
        /// Places a <see cref="Block"/> with specified <see cref="Def"/> at position /x/, /y/.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if either index exceeds the bounds of this <see cref="Ship"/>.</exception>
        /// <exception cref="InvalidOperationException">Thrown if there is already a <see cref="Block"/> at /x/, /y/.</exception>
        public Block PlaceBlock(BlockDef def, int x, int y) {
            ValidateIndices(nameof(PlaceBlock), x, y);
            
            if(unsafeGetBlock(x, y) == null)                           // If there is NOT a Block at /x/, /y/,
                return this.blocks[x, y] = new Block(this, def, x, y); //     place a Block there and return it.
            else                                                       // If there IS a Block at /x/, /y/,
                throw new InvalidOperationException(                   //     throw an InvalidOperationException.
                    $"{nameof(Ship)}.{nameof(PlaceBlock)}(): There is already a {nameof(Block)} at position ({x}, {y})!"
                    );
        }

        /// <summary>
        /// Removes the block at position (/x/, /y/).
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
        /// Gets the <see cref="Furniture"/> at index /x/, /y/, if it exists.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if either index exceeds the bounds of this <see cref="Ship"/>.</exception>
        public bool TryGetFurniture(int x, int y, out Furniture furniture) {
            if(GetBlockOrNull(x, y)?.Furniture is Furniture f) {
                furniture = f;
                return true;
            }
            else {
                furniture = null!;
                return false;
            }
        }
        /// <summary>
        /// Gets the <see cref="Furniture"/> at indices /x/, /y/, or <see cref="null"/> if it does not exist.
        /// </summary>
        public Furniture? GetFurnitureOrNull(int x, int y)
            => GetBlockOrNull(x, y)?.Furniture;

        /// <summary>
        /// Returns whether or not there is a <see cref="Furniture"/> at position (/x/, /y/).
        /// </summary>
        public bool HasFurniture(int x, int y)
            => GetBlockOrNull(x, y)?.Furniture != null;

        /// <summary>
        /// Places a <see cref="Furniture"/> with specified <see cref="Def"/>, at index /x/, /y/.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if either index exceeds the bounds of this <see cref="ship"/>.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if there is no <see cref="Block"/> at /x/, /y/, or if there is already a <see cref="Furniture"/> there.
        /// </exception>
        public Furniture PlaceFurniture(FurnitureDef def, int x, int y, Dir dir) {
            const string Sig = nameof(Ship) + "." + nameof(PlaceFurniture) + "()";

            ValidateIndices(nameof(PlaceFurniture), x, y);
            
            if(unsafeGetBlock(x, y) is Block b) {                            // If there is a block at /x/, /y/:
                if(b.Furniture == null)                                      //     If the block is empty,
                    return SetBlockFurniture(b, def.Construct(b, dir))!;     //         place a Furniture there and return it.
                else                                                         //     If the block is occupied,
                    throw new InvalidOperationException(                     //         throw an InvalidOperationException.
                        $"{Sig}: There is already a {nameof(Furniture)} at index ({x}, {y})!"
                        );
            }
            else {                                                           // If there is no block at /x/, /y/,
                throw new InvalidOperationException(                         //     throw an InvalidOperationException.
                    $"{Sig}: There is no {nameof(Block)} at index ({x}, {y})!"
                    );
            }
        }

        /// <summary>
        /// Removes the <see cref="Furniture"/> at index /x/, /y/.
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

        #region Stock Accessor Methods
        /// <summary>
        /// Gets the <see cref="Stock"/> at /x/, /y/, if it exists.
        /// </summary>
        public bool TryGetStock(int x, int y, out Stock stock) {
            if(GetBlockOrNull(x, y)?.Stock is Stock s) {
                stock = s;
                return true;
            }
            else {
                stock = null!;
                return false;
            }
        }
        /// <summary>
        /// Gets the <see cref="Stock"/> at /x/, /y/, or null if it does not exist.
        /// </summary>
        public Stock? GetStockOrNull(int x, int y)
            => GetBlockOrNull(x, y)?.Stock;

        /// <summary>
        /// Places <see cref="Stock"/> with specified <see cref="ItemDef"/> at indices /x/, /y/.
        /// </summary>
        public Stock PlaceStock(ItemDef def, int x, int y) {
            const string Sig = nameof(Ship) + "." + nameof(PlaceStock) + "()";

            ValidateIndices(nameof(PlaceStock), x, y);

            if(unsafeGetBlock(x, y) is Block b) {
                if(b.Stock == null)
                    return b.Stock = new ShipStock(def, this, b);
                else
                    throw new InvalidOperationException(
                        $"{Sig}: There is already a {nameof(Stock)} at index ({x}, {y})!"
                        ); ;
            }
            else {
                throw new InvalidOperationException(
                    $"{Sig}: There is no {nameof(Block)} at index ({x}, {y})!"
                    );
            }
        }
        #endregion

        #region Agent Accessor Methods
        /// <summary>
        /// Gets the <see cref="Agent"/> at index /x/, /y/, if it exists.
        /// </summary>
        public bool TryGetAgent(int x, int y, out Agent<Ship, Block> agent) {
            foreach(var ag in this.agents) { // For each agent in this ship:
                if(ag.X == x && ag.Y == y) { // If its index is (/x/, /y/),
                    agent = ag;              //     set it as the out parameter,
                    return true;             //     and return true.
                }
            }

            agent = null!; // If we got this far, set the out parameter as null,
            return false;  // and return false.
        }
        /// <summary>
        /// Gets the <see cref="Agent"/> at index /x/, /y/, or <see cref="null"/> if it doesn't exist.
        /// </summary>
        public Agent<Ship, Block>? GetAgentOrNull(int x, int y) {
            foreach(var agent in this.agents) {
                if(agent.X == x && agent.Y == y)
                    return agent;
            }
            return null;
        }
        /// <summary>
        /// Add an <see cref="Agent"/> at index /x/, /y/.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if either index exceeds the bounds of this <see cref="Ship"/>.</exception>
        /// <exception cref="InvalidOperationException">Thrown if there is no <see cref="Block"/> at /x/, /y/.</exception>
        public Agent<Ship, Block> AddAgent(int x, int y) {
            ValidateIndices(nameof(AddAgent), x, y);
            
            if(unsafeGetBlock(x, y) is Block b) {   // If there is a block at /x/, /y/,
                var agent = new ShipAgent(this, b); //     create an agent on it,
                this.agents.Add(agent);             //     add the agent to the list of agents,
                return agent;                       //     and then return the agent.
            }
            else {                                   // If there is no block at /x/, /y/,
                throw new InvalidOperationException( //     throw an InvalidOperationException.
                    $"{nameof(Ship)}.{nameof(AddAgent)}(): There is no block to place the agent onto!"
                    );
            }
        }
        #endregion

        #region Job Accessor Methods
        /// <summary> Creates a job with the specified <see cref="Job{Ship, Block}.Toil"/>. </summary>
        public Job<Ship, Block> CreateJob(int x, int y, Job<Ship, Block>.Toil task) {
            var j = new Job<Ship, Block>(this, x, y, task);
            this.jobs.Add(j);
            return j;
        }

        /// <summary>
        /// Gets a <see cref="Job"/> that can currently be worked on by the specified <see cref="Agent"/>.
        /// </summary>
        public Job<Ship, Block>? GetWorkableJob(Agent<Ship, Block> hiree) {
            for(int i = 0; i < jobs.Count; i++) { // For each job in this ship:
                var job = jobs[i];
                if(job.Worker == null && job.Qualify(hiree, out _)) {   // If the job is unclaimed and the agent can work it,
                    return job;                                         //     return it.
                }
            }
                         // If we got this far without leaving the method,
                         //     that means there is no workable job on the ship.
            return null; //     Return null.
        }

        /// <summary> Removes the specified <see cref="Job"/> from this <see cref="Ship"/>. </summary>
        public void RemoveJob(Job<Ship, Block> job) => this.jobs.Remove(job);
        #endregion

        #region Private Methods
        /// <summary> Get the <see cref="Block"/> at position (/x/, /y/), without checking the indices. </summary>
        private Block? unsafeGetBlock(int x, int y) => this.blocks[x, y];

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
        void IUpdatable.Update(Master master, Time delta) => Update(master, delta);
        protected virtual void Update(Master master, Time delta) {
            if(Destination is PointF dest) {                            // If there is a destination:
                if(PointF.Distance(Center, dest) > 0.25f) {             // If the destination is more than half a block away,
                    var newAngle = new Vector(Center, dest).Angle;      //    get the angle towards the destination,
                    var step = Def.TurnSpeed * delta;                   //
                    Angle = Angle.MoveTowards(Angle, newAngle, step);   //    and slowly rotate the ship towards that angle.
                                                                        //
                    Center += Right * Def.Speed * delta;                //    Slowly move the ship to the right.
                }                                                       //
                else {                                                  // If the destination is within half a block,
                    Destination = null;                                 //     unassign the destination (we're there!)
                }
            }
            //
            // Update every part in the ship.
            for(int x = 0; x < Width; x++) {
                for(int y = 0; y < Height; y++) {
                    if(GetBlockOrNull(x, y) is IPartContract b)
                        b.Update(master);
                }
            }
            for(int x = 0; x < Width; x++) {
                for(int y = 0; y < Height; y++) {
                    if(GetFurnitureOrNull(x, y) is IPartContract f)
                        f.Update(master);
                }
            }
            //
            // Update every agent in the ship.
            foreach(var agent in this.agents) {
                (agent as IUpdatable).Update(master, delta);
            }
        }
        #endregion

        #region IDrawable Implementation
        void IDrawable<Sea>.Draw(ILocalDrawer<Sea> drawer) => Draw(drawer);
        /// <summary>
        /// Draw this <see cref="Ship"/> onscreen.
        /// </summary>
        protected virtual void Draw(ILocalDrawer<Sea> seaDrawer) {
            var drawer = new ShipDrawer(seaDrawer, this);
            // Draw each block.
            for(int x = 0; x < Width; x++) {
                for(int y = 0; y < Height; y++) {
                    if(GetBlockOrNull(x, y) is Block b)
                        DrawPart(b, drawer);
                }
            }

            // Draw each Furniture.
            for(int x = 0; x < Width; x++) {
                for(int y = 0; y < Height; y++) {
                    if(GetFurnitureOrNull(x, y) is Furniture f)
                        DrawPart(f, drawer);
                }
            }

            // Draw each stock.
            for(int x = 0; x < Width; x++) {
                for(int y = 0; y < Height; y++) {
                    if(GetStockOrNull(x, y) is Stock s)
                        (s as IDrawable<Ship>).Draw(drawer);
                }
            }

            // Draw each job.
            foreach(IDrawable<Ship> job in this.jobs) {
                job.Draw(drawer);
            }

            // Draw each agent.
            foreach(var agent in this.agents) {
                (agent as IDrawable<Ship>).Draw(drawer);
            }
        }

        /// <summary> Draw the specified <see cref="Part"/> to the screen. </summary>
        protected void DrawPart(Part p, ILocalDrawer<Ship> drawer) => (p as IPartContract).Draw(drawer);
        #endregion

        #region IScreenSpaceTarget Implementation
        int UI.IScreenSpaceTarget.X => Sea.SeaPointToScreen(Center).X;
        int UI.IScreenSpaceTarget.Y => Sea.SeaPointToScreen(CenterX, GetBounds().Top).y;
        #endregion

        #region IFocusableParent Implementation
        List<IFocusable> IFocusableParent.GetFocusable(PointF seaPoint) {
            var focusable = new List<IFocusable>();

            var (shipX, shipY) = SeaPointToShip(seaPoint);
            if(TryGetAgent(shipX, shipY, out var agent)) {
                focusable.Add(agent);
            }
            if(GetFurnitureOrNull(shipX, shipY) is Furniture f) {
                focusable.Add(f);
            }
            if(GetStockOrNull(shipX, shipY) is Stock s) {
                focusable.Add(s);
            }
            if(GetBlockOrNull(shipX, shipY) is Block b) {
                focusable.Add(b);
            }

            return focusable;
        }
        #endregion

        #region IGraph Implementation
        IEnumerable<Block> IGraph<Block>.Nodes {
            get {
                // Return every block in this ship.
                for(int x = 0; x < Width; x++) {
                    for(int y = 0; y < Height; y++) {
                        if(GetBlockOrNull(x, y) is Block b)
                            yield return b;
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// Makes some members of <see cref="Part"/> accessible only within this class.
        /// </summary>
        private interface IPartContract
        {
            void Update(Master master);
            void Draw(ILocalDrawer<Ship> drawer);
        }
        /// <summary>
        /// Part of a <see cref="Ocean.Ship"/>.
        /// </summary>
        public abstract class Part : IFocusable, IPartContract
        {
            /// <summary> The <see cref="Ocean.Ship"/> that contains this <see cref="Part"/>. </summary>
            public abstract Ship Ship { get; }

            /// <summary> The X index of this <see cref="Part"/>, local to its <see cref="Ocean.Ship"/>. </summary>
            public abstract int X { get; }
            /// <summary> The Y index of this <see cref="Part"/>, local to its <see cref="Ocean.Ship"/>. </summary>
            public abstract int Y { get; }
            /// <summary> The X and Y indices of this <see cref="Part"/>, local to its <see cref="Ocean.Ship"/>. </summary>
            public virtual PointI Index => (X, Y);

            /// <summary> The direction that this <see cref="Part"/> is facing. </summary>
            public virtual Dir Direction { get; protected set; }
            /// <summary> This <see cref="Part"/>'s angle, local to its <see cref="Ocean.Ship"/>. </summary>
            public virtual Angle Angle => Direction == Dir.Up    ? Angle.Up
                                        : Direction == Dir.Right ? Angle.Right
                                        : Direction == Dir.Down  ? Angle.Down
                                        :                          Angle.Left;

            internal Part() {  } // Ensures that this class can only be derived from within this assembly.

            #region IPartContract Implementation
            void IPartContract.Update(Master master) => Update(master);
            /// <summary> The update loop of this <see cref="Part"/>; is called every frame. </summary>
            protected virtual void Update(Master master) {  }

            void IPartContract.Draw(ILocalDrawer<Ship> drawer) => Draw(drawer);
            /// <summary> Draw this <see cref="Part"/> to the screen. </summary>
            protected abstract void Draw(ILocalDrawer<Ship> drawer);
            #endregion

            #region IFocusable Implementation
            bool IFocusable.IsFocused { set => IsFocused = value; }
            protected bool IsFocused { get; private set; }

            IFocusMenuProvider IFocusable.GetProvider(Master master) => GetFocusProvider(master);
            /// <summary> Gets a focus menu for a Ship Part. </summary>
            protected abstract IFocusMenuProvider GetFocusProvider(Master master);

            /// <summary>
            /// A class that provides a focus menu for a Ship Part.
            /// </summary>
            protected abstract class FocusProvider<TPart> : IFocusMenuProvider
                where TPart : Part
            {
                protected const string MenuID = "shipPartFocusFloating";

                public virtual bool IsLocked => false;
                protected TPart Part { get; }

                public FocusProvider(TPart part) => Part = part;

                public virtual void Update(Master master) {  }
                public virtual void Close(Master master) {  }
            }
            #endregion
        }
    }

    internal sealed class ShipDrawer : ILocalDrawer<Ship>
    {
        private ILocalDrawer<Sea> Drawer { get; }
        private Ship Ship { get; }

        public ShipDrawer(ILocalDrawer<Sea> drawer, Ship ship) {
            Drawer = drawer;
            Ship = ship;
        }

        public void DrawCorner(UI.Sprite sprite, float left, float top, float width, float height, in UI.Color tint)
            => Draw(sprite, left, top, width, height, Angle.Right, (0f, 1f), in tint);
        public void Draw(UI.Sprite sprite, float x, float y, float width, float height,
                         in Angle angle, in PointF origin, in UI.Color tint) {
            PointF texOfset = (1, 1) - origin;
            texOfset = (texOfset.X * width, texOfset.Y * height);
            texOfset += PointF.Rotate((-0.5f, 0.5f), in angle);

            var (seaX, seaY) = Ship.ShipPointToSea(x + texOfset.X, y + texOfset.Y);

            Drawer.Draw(sprite, seaX, seaY, width, height, -angle - Ship.Angle, (0, 0), in tint);
        }
        public void DrawLine(PointF start, PointF end, in UI.Color color)
            => Drawer.DrawLine(Ship.ShipPointToSea(start), Ship.ShipPointToSea(end), in color);

        public void DrawString(UI.Font font, string text, float left, float top, in UI.Color color) => throw new NotImplementedException();
    }
}

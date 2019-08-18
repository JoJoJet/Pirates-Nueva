using System;
using System.Collections.Generic;
using Pirates_Nueva.Path;
using Pirates_Nueva.Ocean.Agents;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;

namespace Pirates_Nueva.Ocean
{
    using Agent = Agent<Ship, Block>;
    using Stock = Stock<Ship, Block>;
    public abstract class Ship
        : Entity, IAgentContainer<Ship, Block>, ISpaceLocus<Ship>,
          IFocusableParent, IFocusable, 
          IUpdatable, IDrawable<Sea>, UI.IScreenSpaceTarget
    {
        protected const string RootID = "root";

        private readonly Block?[,] blocks;
        private readonly List<Agent> agents = new List<Agent>();
        
        private readonly List<Job<Ship, Block>> jobs = new List<Job<Ship, Block>>();

        /// <summary>
        /// A delegate that allows this class to set the <see cref="Block.Furniture"/> property, even though that is a private property.
        /// </summary>
        internal static Func<Block, Furniture?, Furniture?> SetBlockFurniture { private protected get; set; }

        public ShipDef Def { get; }

        /// <summary> The <see cref="Ocean.Faction"/> to which this Ship is aligned. </summary>
        public Faction Faction { get; }

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

        /// <summary>
        /// The object that handles transformation for this <see cref="Ship"/>.
        /// </summary>
        public Space<Ship, ShipTransformer> Transformer { get; }

        /// <summary> The X index of this <see cref="Ship"/>'s root <see cref="Block"/>. </summary>
        public int RootX => RootIndex.X;
        /// <summary> The Y index of this <see cref="Ship"/>'s root <see cref="Block"/>. </summary>
        public int RootY => RootIndex.Y;
        /// <summary> The local indices of this <see cref="Ship"/>'s root <see cref="Block"/>. </summary>
        private PointI RootIndex => Def.RootIndex;

        /// <summary>
        /// Create a ship with specified /width/ and /height/.
        /// </summary>
        public Ship(Sea sea, ShipDef def, Faction faction) : base(sea) {
            Def = def;
            Faction = faction;
            Transformer = new Space<Ship, ShipTransformer>(this);

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
            var lb = Transformer.PointFrom(left, bottom);
            var lt = Transformer.PointFrom(left, top);
            var rt = Transformer.PointFrom(right, top);
            var rb = Transformer.PointFrom(right, bottom);

            // Return a bounding box eveloping all four points above.
            return new BoundingBox(
                min(lb.X, lt.X, rt.X, rb.X), min(lb.Y, lt.Y, rt.Y, rb.Y),
                max(lb.X, lt.X, rt.X, rb.X), max(lb.Y, lt.Y, rt.Y, rb.Y)
                );

            float min(float f1, float f2, float f3, float f4) => Math.Min(Math.Min(f1, f2), Math.Min(f3, f4)); // Find the min of 4 values
            float max(float f1, float f2, float f3, float f4) => Math.Max(Math.Max(f1, f2), Math.Max(f3, f4)); // Find the max of 4 values
        }

        protected override bool IsCollidingPrecise(PointF point) {
            //
            // Convert the point to an index on the ship,
            // and return whether or not there is a block there.
            var (shipX, shipY) = Transformer.PointToIndex(point);
            return HasBlock(shipX, shipY);
        }

        /// <summary>
        /// Whether or not the specified indices are within the bounds of this ship.
        /// </summary>
        public bool AreIndicesValid(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

        #region Block Accessor Methods
        /// <summary>
        /// Gets the <see cref="Block"/> at indices (/x/, /y/), if it exists.
        /// </summary>
        public bool TryGetBlock(int x, int y, [NotNullWhen(true)] out Block? block) {
            if(AreIndicesValid(x, y) && unsafeGetBlock(x, y) is Block b) {
                block = b;
                return true;
            }
            else {
                block = null;
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

            if(unsafeGetBlock(x, y) == null) {          // If there is NOT a Block at /x/, /y/,
                var block = new Block(this, def, x, y); //     place a block there,
                block.SubscribeOnDestroyed(b => {       //     sign up to be notified of its destruction,
                    if(HasBlock(b.X, b.Y))              //
                        DestroyBlock(b.X, b.Y);          //
                });                                     //
                return this.blocks[x, y] = block;       //     and return it.
            }
            else                                                       // If there IS a Block at /x/, /y/,
                throw new InvalidOperationException(                   //     throw an InvalidOperationException.
                    $"{nameof(Ship)}.{nameof(PlaceBlock)}(): There is already a {nameof(Block)} at position ({x}, {y})!"
                    );
        }

        /// <summary>
        /// Destroys the block at position (/x/, /y/).
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if either index exceeds the bounds of this <see cref="Ship"/>.</exception>
        /// <exception cref="InvalidOperationException">Thrown if there is no <see cref="Block"/> at /x/, /y/.</exception>
        public void DestroyBlock(int x, int y) {
            ValidateIndices(nameof(DestroyBlock), x, y);
            
            if(unsafeGetBlock(x, y) is Block b) { // If there is a Block at /x/, /y/,
                this.blocks[x, y] = null;         //     remove it from this ship,
                b.Destroy();                      //     and destroy it.
            }
            else {                                   // If there is no Block at /x/, /y/,
                throw new InvalidOperationException( //    throw an InvalidOperationException.
                    $"{nameof(Ship)}.{nameof(DestroyBlock)}(): There is no {nameof(Block)} at position ({x}, {y})!"
                    );
            }
        }
        #endregion

        #region Furniture Accessor Methods
        /// <summary>
        /// Gets the <see cref="Furniture"/> at index /x/, /y/, if it exists.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if either index exceeds the bounds of this <see cref="Ship"/>.</exception>
        public bool TryGetFurniture(int x, int y, [NotNullWhen(true)] out Furniture? furniture) {
            if(GetBlockOrNull(x, y)?.Furniture is Furniture f) {
                furniture = f;
                return true;
            }
            else {
                furniture = null;
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
        public bool TryGetStock(int x, int y, [NotNullWhen(true)] out Stock? stock) {
            if(GetBlockOrNull(x, y)?.Stock is Stock s) {
                stock = s;
                return true;
            }
            else {
                stock = null;
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
                    return b.Stock = new Stock(def, this, b);
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
        public bool TryGetAgent(int x, int y, [NotNullWhen(true)] out Agent? agent) {
            foreach(var ag in this.agents) { // For each agent in this ship:
                if(ag.X == x && ag.Y == y) { // If its index is (/x/, /y/),
                    agent = ag;              //     set it as the out parameter,
                    return true;             //     and return true.
                }
            }

            agent = null;  // If we got this far, set the out parameter as null,
            return false;  // and return false.
        }
        /// <summary>
        /// Gets the <see cref="Agent"/> at index /x/, /y/, or <see cref="null"/> if it doesn't exist.
        /// </summary>
        public Agent? GetAgentOrNull(int x, int y) {
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
        public Agent AddAgent(int x, int y) {
            ValidateIndices(nameof(AddAgent), x, y);
            
            if(unsafeGetBlock(x, y) is Block b) {   // If there is a block at /x/, /y/,
                var agent = new Agent(this, b); //     create an agent on it,
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
        public Job<Ship, Block>? GetWorkableJob(Agent hiree) {
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

        #region IAgentContainer Implementation
        Block? IAgentContainer<Ship, Block>.GetSpotOrNull(int x, int y) => GetBlockOrNull(x, y);
        #endregion

        #region ISpaceLocus Implementation
        ISpaceLocus? ISpaceLocus.Parent => Sea;
        ISpace ISpaceLocus.Transformer => Transformer;
        ISpace<Ship> ISpaceLocus<Ship>.Transformer => Transformer;
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

        #region IUpdatable Implementation
        void IUpdatable.Update(in UpdateParams @params) => Update(in @params);
        protected virtual void Update(in UpdateParams @params) {
            var (delta, master) = @params;
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
                    if(GetBlockOrNull(x, y) is IShipPart b)
                        b.Update(master);
                }
            }
            for(int x = 0; x < Width; x++) {
                for(int y = 0; y < Height; y++) {
                    if(GetFurnitureOrNull(x, y) is IShipPart f)
                        f.Update(master);
                }
            }
            //
            // Update every agent in the ship.
            foreach(var agent in this.agents) {
                (agent as IUpdatable).Update(in @params);
            }
        }
        #endregion

        #region IDrawable Implementation
        void IDrawable<Sea>.Draw(ILocalDrawer<Sea> drawer) => Draw(drawer);
        /// <summary>
        /// Draw this <see cref="Ship"/> onscreen.
        /// </summary>
        protected virtual void Draw(ILocalDrawer<Sea> seaDrawer) {
            var drawer = new SpaceDrawer<Ship, ShipTransformer, Sea>(seaDrawer, Transformer);
            //
            // Draw each block.
            for(int x = 0; x < Width; x++) {
                for(int y = 0; y < Height; y++) {
                    if(GetBlockOrNull(x, y) is IDrawable<Ship> b)
                        b.Draw(drawer);
                }
            }
            //
            // Draw each Furniture.
            for(int x = 0; x < Width; x++) {
                for(int y = 0; y < Height; y++) {
                    if(GetFurnitureOrNull(x, y) is IDrawable<Ship> f)
                        f.Draw(drawer);
                }
            }
            //
            // Draw each stock.
            for(int x = 0; x < Width; x++) {
                for(int y = 0; y < Height; y++) {
                    if(GetStockOrNull(x, y) is Stock s)
                        (s as IDrawable<Ship>).Draw(drawer);
                }
            }
            //
            // Draw each job.
            foreach(IDrawable<Ship> job in this.jobs) {
                job.Draw(drawer);
            }
            //
            // Draw each agent.
            foreach(var agent in this.agents) {
                (agent as IDrawable<Ship>).Draw(drawer);
            }

            //
            // If we're being focused on,
            // draw a line to the destination.
            if(IsFocused && Destination is PointF dest)
                seaDrawer.DrawLine(Center, dest, in UI.Color.Black);
        }
        #endregion

        #region IScreenSpaceTarget Implementation
        int UI.IScreenSpaceTarget.X => (int)Sea.Transformer.PointFrom(Center).X;
        int UI.IScreenSpaceTarget.Y => (int)Sea.Transformer.PointFrom(CenterX, GetBounds().Top).Y;
        #endregion

        #region IFocusableParent Implementation
        List<IFocusable> IFocusableParent.GetFocusable(PointF seaPoint) {
            var focusable = new List<IFocusable>();

            var (shipX, shipY) = Transformer.PointToIndex(seaPoint);
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

        /// <summary>
        /// Makes some members of <see cref="Part"/> accessible only within this class.
        /// </summary>
        private interface IShipPart
        {
            void Update(Master master);
        }
        /// <summary>
        /// Part of a <see cref="Ocean.Ship"/>.
        /// </summary>
        public abstract class Part : IShipPart, IFocusable, IDrawable<Ship>, UI.IScreenSpaceTarget
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
            public virtual Angle Angle => Direction.Angle();

            internal Part() {  } // Ensures that this class can only be derived from within this assembly.

            void IShipPart.Update(Master master) => Update(master);
            /// <summary> The update loop of this <see cref="Part"/>; is called every frame. </summary>
            protected virtual void Update(Master master) {  }

            void IDrawable<Ship>.Draw(ILocalDrawer<Ship> drawer) => Draw(drawer);
            /// <summary> Draw this <see cref="Part"/> to the screen. </summary>
            protected abstract void Draw(ILocalDrawer<Ship> drawer);

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

            #region IScreenSpaceTarget Implementation
            private PointF ScreenTarget => Ship.Sea.Transformer.PointFrom(Ship.Transformer.PointFrom(X, Y));
            int UI.IScreenSpaceTarget.X => (int)ScreenTarget.X;
            int UI.IScreenSpaceTarget.Y => (int)ScreenTarget.Y;
            #endregion
        }

        #region IFocusable Implementation
        protected bool IsFocused { get; private set; }
        bool IFocusable.IsFocused { set => IsFocused = value; }

        IFocusMenuProvider IFocusable.GetProvider(Master master) => throw new NotImplementedException();

        /// <summary>
        /// Base class for implementations of <see cref="IFocusMenuProvider"/> for <see cref="Ship"/>s.
        /// </summary>
        /// <typeparam name="T">The subclass of <see cref="Ocean.Ship"/>.</typeparam>
        protected class FocusProvider<T> : IFocusMenuProvider
            where T : Ship
        {
            public bool IsLocked { get; protected set; }

            public T Ship { get; }

            public FocusProvider(T ship)
                => Ship = ship;

            public virtual void Update(Master master) {  }
            public virtual void Close(Master master) {  }
        }
        #endregion
    }

    public readonly struct ShipTransformer : ITransformer<Ship>
    {
        bool ITransformer<Ship>.HasRotation => true;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        PointF ITransformer<Ship>.PointTo(Ship ship, in PointF parent) {
            var untranslated = parent - ship.Center;                     // Un-translate the coordinates from the ship.
            var unrotated = PointF.Rotate(in untranslated, -ship.Angle); // Un-rotate the coordinates from around the ship's center.
            var indexed = new PointF(unrotated.X + ship.RootX + 0.5f,    // Center the coordinates around the ship's bottom left.
                                     unrotated.Y + ship.RootY + 0.5f);
            return indexed;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        PointF ITransformer<Ship>.PointFrom(Ship ship, in PointF indices) {
            var unindexed = new PointF(indices.X - ship.RootX - 0.5f,  // Un-center the coordinates from the ship's bottom left.
                                       indices.Y - ship.RootY - 0.5f); //
            var rotated = PointF.Rotate(unindexed, ship.Angle);        // Rotate the coordinates around the ship's center.
            var translated = rotated + ship.Center;                    // Translate the coordinates by the ship's position.
            return translated;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        Angle ITransformer<Ship>.AngleTo(Ship ship, in Angle parent) => parent - ship.Angle;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        Angle ITransformer<Ship>.AngleFrom(Ship ship, in Angle local) => local + ship.Angle;

        float ITransformer<Ship>.ScaleTo(Ship space, float parent) => parent;
        float ITransformer<Ship>.ScaleFrom(Ship space, float local) => local;
    }
}

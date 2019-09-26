using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;
using Pirates_Nueva.Path;
using Pirates_Nueva.Ocean.Agents;
using Pirates_Nueva.UI;
using System.Diagnostics;
using sang = Pirates_Nueva.SignedAngle;

namespace Pirates_Nueva.Ocean
{
    using Agent = Agent<Ship, Block>;
    using Stock = Stock<Ship, Block>;
    using Toil = Job<Ship, Block>.Toil;
    public class Ship : Entity,
        IAgentContainer<Ship, Block>, ISpaceLocus<Ship>,
        IFocusableParent, IFocusable, 
        IUpdatable, IDrawable<Sea>, IScreenSpaceTarget
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

        /// <summary>
        /// This <see cref="Ship"/>'s rotation. 0 is pointing directly rightwards, rotation is counter-clockwise.
        /// </summary>
        public Angle Angle { get; protected set; }

        /// <summary>
        /// The direction from this <see cref="Ship"/>'s center to its right edge, <see cref="Sea"/>-space.
        /// </summary>
        public Vector Right => Angle.Vector;

        public PointF? Destination { get; private set; }

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

        #region Navigation
        public void SetDestination(PointF destination) {
            Destination = destination;
        }
        #endregion

        /// <summary>
        /// Checks if the specified <see cref="Sea"/>-space point collides with this <see cref="Ship"/>.
        /// </summary>
        public bool Collides(in PointF seaPoint) {
            var local = (PointI)Transformer.PointTo(in seaPoint);
            return local.X >= 0 && local.X < Width
                && local.Y >= 0 && local.Y < Height
                && unsafeGetBlock(local.X, local.Y) != null;
        }

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
        private const int ProbeCount = 12;
        /// <summary>
        /// Fills an array with angle offsets for navigation "probes".
        /// </summary>
        private static void FillProbes(Span<sang> probes) {
            probes[0]  = sang.Unsafe( Angle.FullTurn * 10 / 64);
            probes[1]  = sang.Unsafe( Angle.FullTurn * 8  / 64);
            probes[2]  = sang.Unsafe( Angle.FullTurn * 6  / 64);
            probes[3]  = sang.Unsafe( Angle.FullTurn * 4  / 64);
            probes[4]  = sang.Unsafe( Angle.FullTurn * 2  / 64);
            probes[5]  = sang.Unsafe( Angle.FullTurn * 1  / 64);
            probes[6]  = sang.Unsafe(-Angle.FullTurn * 1  / 64);
            probes[7]  = sang.Unsafe(-Angle.FullTurn * 2  / 64);
            probes[8]  = sang.Unsafe(-Angle.FullTurn * 4  / 64);
            probes[9]  = sang.Unsafe(-Angle.FullTurn * 6  / 64);
            probes[10] = sang.Unsafe(-Angle.FullTurn * 8  / 64);
            probes[11] = sang.Unsafe(-Angle.FullTurn * 10 / 64);
        }
        /// <summary>
        /// Fills an array with local origin points for directly-forward-facing navigation probes.
        /// Specific to this ship. The length of the array is <see cref="Height"/> * 2.
        /// </summary>
        private void FillForwardProbes(Span<PointF?> probes) {
            for(int y = 0; y < Height; y++) {
                for(int x = Width-1; x >= 0; x--) {
                    if(this[x, y] != null) {
                        probes[y * 2    ] = new PointF(x + 0.5f, y + 1 / 3f);
                        probes[y * 2 + 1] = new PointF(x + 0.5f, y + 2 / 3f);
                        break;
                    }
                }
            }
        } 

        void IUpdatable.Update(in UpdateParams @params) => Update(in @params);
        protected virtual void Update(in UpdateParams @params) {
            var (delta, master) = @params;
            //
            // If we have reached the destination,
            // unassign it.
            if(Destination != null && Collides(Destination.Value)) {
                Destination = null;
            }
            else if(Destination is PointF dest) {
                //
                // Find the length of each probe that looks for obstacles in front of the ship.
                // This should be related to the ship's turning radius.
                float probeLength = Def.TurnRadius * 3;

                //
                // Get an array of probes.
                Span<sang> probes = stackalloc sang[ProbeCount];
                FillProbes(probes);
                Span<float> probeFactors = stackalloc float[ProbeCount];

                //
                // If any of the probes intersect with anything,
                // they should push the ship in the opposite direction.
                // This gives a decent amount of spacing between the ship and obstacles.
                //
                // FIXME: There is currently an issue where if there is an equal
                // amount of obstacles on both sides, the ship will just stall.
                // We need to include a special case that forces the ship to pick a side
                // when there is a wide, even-shaped obstacle ahead.
                bool anyCollision = false;
                var probePush = sang.Right;
                for(int i = 0; i < ProbeCount; i++) {
                    var ang = Angle + probes[i];
                    if(Sea.IntersectsWithIsland(Center, Center + ang.Vector * probeLength, out var sqrDist)) {
                        anyCollision = true;
                        //
                        // The strength with which the probe pushes should vary depending
                        // on the distance from the intersection to the ship.
                        // An intersection at the very end of the probe should
                        // have half the strength as one up close and personal.
                        probeFactors[i] = MathF.Sqrt(sqrDist) / probeLength;
                        var factor = MoreMath.Lerp(1.5f, 0.5f, probeFactors[i]);
                        var push = i >= ProbeCount / 2
                                   ? probes[ProbeCount - 1 - (i - ProbeCount / 2)]
                                   : probes[ProbeCount / 2 - 1 - i];
                        probePush += push * factor;
                    }
                    else {
                        probeFactors[i] = float.MaxValue;
                    }
                }
                var targetAng = (Angle)(Angle - probePush);
                //
                // If the probes hit things but the destination is close and is a straight shot,
                // allow the ship to navigate closer to obstacles than normal.
                var destFactor = PointF.Distance(Center, dest) / probeLength;
                if(anyCollision && destFactor < 1f) {
                    var destAng = new Vector(Center, dest).Angle;
                    var localDestAng = (sang)destAng - Angle;
                    //
                    // Iterate over the probes, starting from the center two and working towards the ends.
                    for(int i = 0; i < ProbeCount / 2; i++) {
                        int a = ProbeCount / 2 - i - 1,
                            b = ProbeCount / 2 + i;
                        //
                        // If the current two probes have found anything
                        // between the ship and the destination,
                        // that means there is NOT a straight shot.
                        // Break from the loop.
                        if(probeFactors[a] <= destFactor || probeFactors[b] <= destFactor) {
                            break;
                        }
                        //
                        // If the angle towards the destination is between the two current probes,
                        // that means the destination is a straight shot!
                        // Set the target angle as the angle towards the destination.
                        if(localDestAng <= probes[a] && localDestAng >= probes[b]) {
                            targetAng = destAng;
                            break;
                        }
                    }
                }
                //
                // If none of the probes collided with anything (the way is clear),
                // Then the ship should point in the direction of the destination.
                // If the ship is already pointing really close to the destination,
                // don't do anything. This is to reduce jitter.
                if(!anyCollision) {
                    var destAng = new Vector(Center, dest).Angle;
                    if(Angle.Distance(Angle, in destAng) > 0.05f)
                        targetAng = destAng;
                }

                var oldAng = Angle;
                //
                // Gradually turn the ship in the direction of the target angle.
                Angle = Angle.MoveTowards(Angle, targetAng, Def.TurnSpeed * delta);
                //
                // Throw an exception if the ship instantly turns by a large margin.
                // That bug should be gone, but who knows. At least this way, we can
                // inspect the code if it should happen.
                Debug.Assert(Angle.Distance(Angle, in oldAng) < Angle.FullTurn);

                //
                // Check if the path forward is obstructed by any islands.
                bool isObstructed = false;
                if(anyCollision) {
                    //
                    // Get an array of probes pointing directly forward.
                    Span<PointF?> forwardProbes = stackalloc PointF?[Height * 2];
                    FillForwardProbes(forwardProbes);
                    for(int i = 0; i < forwardProbes.Length; i++) {
                        if(forwardProbes[i] is null)
                            continue;
                        //
                        // Test collision for the probe.
                        // If it hit anything, that means the path forward is obstructed.
                        var origin = Transformer.PointFrom(forwardProbes[i]!.Value);
                        if(Sea.IntersectsWithIsland(origin, origin + Right)) {
                            isObstructed = true;
                            break;
                        }
                    }
                }

                //
                // If the path forward is clear, 
                // gradually move the ship in the direction of the bow.
                if(!isObstructed) {
                    Center += Right * (Def.Speed * delta);
                }
                //
                // If the path forward is obstructed,
                // reset the angle to its value at the start of this frame.
                else {
                    Angle = oldAng;
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
            // If we're being focused on, draw our path.
            if(IsFocused && Destination is PointF dest) {
                seaDrawer.DrawLine(Center, dest, in Color.White);

                //
                // Draw the probes extending from the root of this Ship.
                {
                    Span<sang> probes = stackalloc sang[ProbeCount];
                    FillProbes(probes);
                    for(int i = 0; i < ProbeCount; i++) {
                        var ang = Angle + probes[i];
                        var start = Center;
                        var end = Center + ang.Vector * Def.TurnRadius * 3;
                        var color = Sea.IntersectsWithIsland(start, end)
                                    ? Color.Black
                                    : Color.Lime;
                        seaDrawer.DrawLine(start, end, in color);
                    }
                }

                //
                // Draw the probes extending from the front of this Ship.
                {
                    Span<PointF?> probes = stackalloc PointF?[Height * 2];
                    FillForwardProbes(probes);
                    for(int i = 0; i < probes.Length; i++) {
                        if(probes[i] is null)
                            continue;
                        var origin = Transformer.PointFrom(probes[i]!.Value);
                        seaDrawer.DrawLine(origin, origin + Right, in Color.DarkLime);
                    }
                }
            }
        }
        #endregion

        #region IScreenSpaceTarget Implementation
        int IScreenSpaceTarget.X => (int)Sea.Transformer.PointFrom(Center).X;
        int IScreenSpaceTarget.Y => (int)Sea.Transformer.PointFrom(CenterX, GetBounds().Top).Y;
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

        #region IFocusable Implementation
        protected bool IsFocused { get; private set; }
        bool IFocusable.IsFocused { set => IsFocused = value; }

        IFocusMenuProvider IFocusable.GetProvider(Master master)
            => Faction.IsPlayer
               ? new PlayerFocusProvider<Ship>(this, master)
               : new FocusProvider<Ship>(this);

        /// <summary>
        /// Base class for implementations of <see cref="IFocusMenuProvider"/> for <see cref="Ship"/>s.
        /// </summary>
        /// <typeparam name="T">The subclass of <see cref="Ocean.Ship"/>.</typeparam>
        protected partial class FocusProvider<T> : IFocusMenuProvider
            where T : Ship
        {
            public bool IsLocked { get; protected set; }

            public T Ship { get; }

            public FocusProvider(T ship)
                => Ship = ship;

            public virtual void Update(Master master) { }
            public virtual void Close(Master master) { }
        }

        protected class PlayerFocusProvider<T> : FocusProvider<T>
            where T : Ship
        {
            private enum FocusState { None, Movement, Editing };
            private enum PlaceMode { None, Block, Furniture, Gunpowder };

            const string MenuID = "playerShipFloating";

            private FocusState state;
            private PlaceMode placeMode;
            private Dir placeDir;

            private FloatingMenu Menu { get; }

            public PlayerFocusProvider(T ship, Master master) : base(ship) {
                //
                // Create a GUI menu.
                master.GUI.AddMenu(
                    MenuID,
                    Menu = new FloatingMenu(
                        Ship, (0, -0.025f), Corner.BottomLeft,
                        new Button<GUI.Menu>("Edit", master.Font, () => SetState(FocusState.Editing)),
                        new Button<GUI.Menu>("Move", master.Font, () => SetState(FocusState.Movement))
                        )
                    );
            }

            const string QuitID = "playerShipEditing_quit",
                         BlockID = "playerShipEditing_placeBlock",
                         FurnID = "playerShipEditing_placeFurniture",
                         GunpID = "playerShipEditing_placeGunpowder";

            public override void Update(Master master) {
                switch(this.state) {
                    //
                    // Editing the ship's layout.
                    case FocusState.Editing:
                        //
                        // If there's no ship editing menu, add one.
                        if(!master.GUI.HasEdge(QuitID)) {
                            master.GUI.AddEdge(QuitID, Edge.Bottom, Direction.Left, new Button<Edge>("Quit", master.Font, () => unsetEditing()));
                            master.GUI.AddEdge(BlockID, Edge.Bottom, Direction.Left, new Button<Edge>("Block", master.Font, () => placeMode = PlaceMode.Block));
                            master.GUI.AddEdge(FurnID, Edge.Bottom, Direction.Left, new Button<Edge>("Furniture", master.Font, () => placeMode = PlaceMode.Furniture));
                            master.GUI.AddEdge(GunpID, Edge.Bottom, Direction.Left, new Button<Edge>("Gunpowder", master.Font, () => placeMode = PlaceMode.Gunpowder));
                        }
                        //
                        // Lock focus & call the editing method.
                        IsLocked = true;
                        updateEditing();

                        void unsetEditing() {
                            //
                            // Remove the ship editing menu.
                            if(master.GUI.HasEdge(QuitID))
                                master.GUI.RemoveEdges(QuitID, BlockID, FurnID, GunpID);
                            //
                            // Release the lock.
                            this.placeMode = PlaceMode.None;
                            IsLocked = false;
                            SetState(FocusState.None);
                        }
                        break;
                    //
                    // Sailing.
                    case FocusState.Movement:
                        IsLocked = true;                             // Lock focus onto this object.
                        master.GUI.Tooltip = "Click the new destination"; // Set a tooltip telling the user what to do.

                        if(master.Input.MouseLeft.IsDown && !master.GUI.IsMouseOverGUI) { // When the user clicks:
                            Ship.SetDestination(Ship.Sea.MousePosition);                  //     Set the destination as the click point,
                            SetState(FocusState.None);                                    //     unset the focus option,
                            IsLocked = false;                                             //     release focus from this object,
                            master.GUI.Tooltip = "";                                      //     and unset the tooltip.
                        }
                        break;
                }


                void updateEditing() {
                    placeDir = (Dir)(((int)placeDir + (int)master.Input.Horizontal.Down + 4) % 4); // Cycle through place directions.

                    // If the user left clicks, place a Block or Furniture.
                    if(master.Input.MouseLeft.IsDown && isMouseValid(out int shipX, out int shipY)) {

                        // If the place mode is 'Furniture', try to place a furniture.
                        if(placeMode == PlaceMode.Furniture) {
                            // If the place the user clicked has a Block but no Furniture.
                            if(Ship.HasBlock(shipX, shipY) && !Ship.HasFurniture(shipX, shipY))
                                Ship.PlaceFurniture(FurnitureDef.Get("cannon"), shipX, shipY, placeDir);
                        }
                        // If the place mode is 'Block', try to place a block.
                        if(placeMode == PlaceMode.Block) {
                            // If the place that the user clicked is not occupied.
                            if(Ship.HasBlock(shipX, shipY) == false)
                                Ship.CreateJob(
                                    shipX, shipY,
                                    new Toil(
                                        //
                                        // Place a block if next to the job.
                                        action: new PlaceBlock("wood"),
                                        new IsAdjacentToToil<Ship, Block>(
                                            new Toil(
                                                //
                                                // Path to the job if it's accessible.
                                                action: new PathToToilAdjacent<Ship, Block>(),
                                                new IsAccessibleToToilAdj<Ship, Block>()
                                                )
                                            )
                                        )
                                    );
                        }
                        if(placeMode == PlaceMode.Gunpowder) {
                            if(Ship.GetBlockOrNull(shipX, shipY) is Block b && b.Stock is null)
                                Ship.PlaceStock(ItemDef.Get("gunpowder"), shipX, shipY);
                        }
                    }
                    // If the user right clicks, remove a Block or Furniture.
                    if(master.Input.MouseRight.IsDown && isMouseValid(out shipX, out shipY)) {

                        // If the place mode is 'Furniture', try to remove a Furniture.
                        if(placeMode == PlaceMode.Furniture) {
                            if(Ship.HasFurniture(shipX, shipY))
                                Ship.RemoveFurniture(shipX, shipY);
                        }
                        // If the place mode is 'Block', try to remove a Block.
                        if(placeMode == PlaceMode.Block) {
                            // If the place that the user clicked has a block, and that block is not the Root.
                            if(Ship.GetBlockOrNull(shipX, shipY) is Block b && b.ID != RootID)
                                Ship.DestroyBlock(shipX, shipY);
                        }
                    }

                    //
                    // Returns whether or not the user clicked within the ship, and gives
                    // the position (local to the ship) as out paremters /x/ and /y/.
                    // Also: returns false if the user clicked a GUI element.
                    bool isMouseValid(out int x, out int y) {
                        var (seaX, seaY) = Ship.Sea.MousePosition;
                        (x, y) = Ship.Transformer.PointToIndex(seaX, seaY);

                        return Ship.AreIndicesValid(x, y) && !master.GUI.IsMouseOverGUI;
                    }
                }
            }
            public override void Close(Master master)
                => master.GUI.RemoveMenu(MenuID);

            /// <summary> Sets the current state to the specified value. </summary>
            private void SetState(FocusState state) {
                this.state = state;
                if(state == FocusState.None)
                    Menu.Unhide();
                else
                    Menu.Hide();
            }
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
        public abstract class Part : IShipPart, IFocusable, IDrawable<Ship>, IScreenSpaceTarget
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
            int IScreenSpaceTarget.X => (int)ScreenTarget.X;
            int IScreenSpaceTarget.Y => (int)ScreenTarget.Y;
            #endregion
        }
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

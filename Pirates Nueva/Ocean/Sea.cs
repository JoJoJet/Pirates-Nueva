using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Pirates_Nueva.Path;

namespace Pirates_Nueva.Ocean
{
    public sealed class Sea : ISpaceLocus<Sea>, IUpdatable, IDrawable<Screen>, IFocusableParent
    {
        private readonly List<Island> islands = new List<Island>();
        private readonly Chunk[,] chunks;

        private readonly List<Entity> entities     = new List<Entity>(),
                                      addBuffer    = new List<Entity>(),
                                      removeBuffer = new List<Entity>();

        public int ChunksWidth => this.chunks.GetLength(0);
        public int ChunksHeight => this.chunks.GetLength(1);

        public Camera Camera { get; }

        /// <summary> The number of screen pixels corresponding to a unit within this <see cref="Sea"/>. </summary>
        public int PPU => (int)Camera.Zoom;

        /// <summary> The position of the mouse, in <see cref="Sea"/>-space. </summary>
        public PointF MousePosition => Transformer.PointTo(Master.Input.MousePosition);

        public Space<Sea, SeaTransformer> Transformer { get; }
        
        internal Master Master { get; }

        internal Sea(Master master) {
            Master = master;
            Camera = new Camera(this);
            Transformer = new Space<Sea, SeaTransformer>(this);

            //
            // Generate the islands.
            const int IslandsWidth = 10, IslandsHeight = 10;
            const int Offset = 250;
            const int Chance = 45;

            var r = new Random();

            for(int x = 0; x < IslandsWidth; x++) {                                 // For every point in a square region:
                for(int y = 0; y < IslandsHeight; y++) {                            //
                    if(r.Next(0, 100) < Chance) {                                   // If we roll a random chance,
                        var i = new Island(this, x * Offset, y * Offset, r.Next()); //     Create an island at this point.
                        this.islands.Add(i);
                    }
                }
            }

            //
            // Generate chunks.
            const int ChunksWidth  = IslandsWidth  * Offset / Chunk.Width,
                      ChunksHeight = IslandsHeight * Offset / Chunk.Height;
            this.chunks = new Chunk[ChunksWidth, ChunksHeight];
            for(int x = 0; x < ChunksWidth; x++) {
                for(int y = 0; y < ChunksHeight; y++) {
                    this.chunks[x, y] = new Chunk(this, x, y);
                }
            }
            //
            // Find the chunks that each island occupies.
            foreach(var i in this.islands) {
                int left   = (int)(i.Left   / Chunk.Width),
                    right  = (int)(i.Right  / Chunk.Width),
                    bottom = (int)(i.Bottom / Chunk.Height),
                    top    = (int)(i.Top    / Chunk.Height);
                for(int x = left; x <= right; x++) {
                    for(int y = bottom; y <= top; y++) {
                        var ch = this.chunks[x, y];
                        var chl = ch.XIndex * Chunk.Width;
                        var chr = chl + Chunk.Width;
                        var chb = ch.YIndex * Chunk.Height;
                        var cht = chb + Chunk.Height;
                        var precise = i.Intersects((chl, chb), (chr, chb))
                                   || i.Intersects((chr, chb), (chr, cht))
                                   || i.Intersects((chr, cht), (chl, cht))
                                   || i.Intersects((chl, cht), (chl, chb));
                        if(precise)
                            ch.AddIsland(i);
                    }
                }
            }
        }

        public Chunk this[int xIndex, int yIndex] => this.chunks[xIndex, yIndex];

        /// <summary>
        /// Gets the index of the <see cref="Chunk"/> that contains the specified point.
        /// Note: the index of the chunk will not necessarily be in bounds.
        /// </summary>
        public static PointI RoundToChunks(in PointF point)
            => new PointI((int)(point.X / Chunk.Width), (int)(point.Y / Chunk.Height));

        /// <summary>
        /// Adds the specified <see cref="Entity"/> to this <see cref="Sea"/> next frame.
        /// </summary>
        public void AddEntity(Entity entity) {
            this.addBuffer.Add(entity ?? throw new ArgumentNullException(nameof(entity)));
        }
        /// <summary>
        /// Removes the specified <see cref="Entity"/> from this <see cref="Sea"/> next frame.
        /// </summary>
        public void RemoveEntity(Entity entity) {
            this.removeBuffer.Add(entity ?? throw new ArgumentNullException(nameof(entity)));
        }

        /// <summary>
        /// Finds and returns an <see cref="Entity"/> that matches the specified predicate.
        /// Returns null if no such entity exists.
        /// </summary>
        /// <typeparam name="T">The type of entity to search for.</typeparam>
        public T? FindEntity<T>(Predicate<T> finder)
            where T : Entity
            => this.entities.Select(e => e as T).Where(t => t != null && finder(t)).FirstOrDefault();

        /// <summary>
        /// Checks if the described line segment intersects with any <see cref="Island"/>s in this <see cref="Sea"/>.
        /// </summary>
        public bool IntersectsWithIsland(PointF start, PointF end) {
            var startChunk = RoundToChunks(in start);
            var endChunk   = RoundToChunks(in end);

            bool intersects = false;
            void step(int x, int y) {
                if(x < 0 || x >= ChunksWidth || y < 0 || y >= ChunksHeight)
                    return;
                foreach(var i in this[x, y].Islands) {
                    if(i.Intersects(start, end)) {
                        intersects = true;
                        break;
                    }
                }
            }

            if(startChunk == endChunk)
                step(startChunk.X, startChunk.Y);
            else
                Bresenham.Line(startChunk, endChunk, step);

            return intersects;
        }

        /// <summary>
        /// Checks if the described line segment intersects with any <see cref="Island"/>s in this <see cref="Sea"/>.
        /// If it does, outputs the squared distance from <paramref name="start"/> to the closest.
        /// </summary>
        /// <param name="sqrDistance">
        /// The squared distance from <paramref name="start"/> to the closest intersection.
        /// To get the euclidean distance, take the square root of this.</param>
        public bool IntersectsWithIsland(PointF start, PointF end, out float sqrDistance) {
            var startChunk = RoundToChunks(in start);
            var endChunk   = RoundToChunks(in end);

            bool intersects = false;
            var sqrDist = float.MaxValue;
            void step(int x, int y) {
                if(x < 0 || x >= ChunksWidth || y < 0 || y >= ChunksHeight)
                    return;
                foreach(var i in this[x, y].Islands) {
                    if(i.Intersects(start, end, out var dist)) {
                        intersects = true;
                        //
                        // If the distance from the start to the intersection
                        // is less than the last smallest distance,
                        // set it as the new smallest distance.
                        if(dist < sqrDist)
                            sqrDist = dist;
                    }
                }
            }

            if(startChunk == endChunk)
                step(startChunk.X, startChunk.Y);
            else
                Bresenham.Line(startChunk, endChunk, step);

            sqrDistance = sqrDist;
            return intersects;
        }

        #region ISpaceLocus Implementation
        ISpaceLocus? ISpaceLocus.Parent => Master.Screen;
        ISpace ISpaceLocus.Transformer => Transformer;
        ISpace<Sea> ISpaceLocus<Sea>.Transformer => Transformer;
        #endregion

        #region IUpdatable Implementation
        void IUpdatable.Update(in UpdateParams @params) {
            //
            // Add & remove entities.
            if(this.addBuffer.Count > 0) {
                this.entities.AddRange(this.addBuffer);
                this.addBuffer.Clear();
            }
            if(this.removeBuffer.Count > 0) {
                foreach(var e in this.removeBuffer)
                    this.entities.Remove(e);
                this.removeBuffer.Clear();
            }

            foreach(var ent in this.entities) { // For every entity:
                if(ent is IUpdatable u)         // If it is updatable,
                    u.Update(in @params);       //     call its Update() method.
            }
        }
        #endregion

        #region IDrawable<> Implementation
        void IDrawable<Screen>.Draw<TScreenDrawer>(in TScreenDrawer screenDrawer) {
            var drawer = new SpaceDrawer<Sea, SeaTransformer, TScreenDrawer, Screen>(screenDrawer, Transformer);
            var idrawer = drawer as ILocalDrawer<Sea>;
            //
            // Draw the grid of chunks
            for(int x = 0; x <= this.chunks.GetLength(0); x++) {
                idrawer.DrawLine((x * Chunk.Width, 0), (x * Chunk.Width, this.chunks.GetLength(1) * Chunk.Height), in UI.Color.Black);
            }
            for(int y = 0; y <= this.chunks.GetLength(1); y++) {
                idrawer.DrawLine((0, y * Chunk.Height), (this.chunks.GetLength(0) * Chunk.Width, y * Chunk.Height), in UI.Color.Black);
            }
            //
            // Draw the islands.
            foreach(var i in this.islands) {
                (i as IDrawable<Sea>).Draw(drawer);
            }

            //
            // Draw the entities.
            foreach(var ent in this.entities) {
                if(ent is IDrawable<Sea> d)
                    d.Draw(drawer);
            }
        }
        #endregion

        #region IFocusableParent Implementation
        /// <summary>
        /// Get any <see cref="IFocusable"/> objects located at /seaPoint/, in sea-space.
        /// </summary>
        List<IFocusable> IFocusableParent.GetFocusable(PointF seaPoint) {
            var focusable = new List<IFocusable>();

            foreach(var ent in entities) {          // For every ship:
                if(ent.IsColliding(seaPoint)) {     // If it is colliding with /seaPoint/,
                    if(ent is IFocusable f) {       // and it implements IFocusable,
                        focusable.Add(f);           // add it to the list of focusable objects.
                    }                                                  // Otherwise:
                    if(ent is IFocusableParent fp)                    // If it implement IFocusableParent,
                        focusable.AddRange(fp.GetFocusable(seaPoint)); // add any of its focusable children to the list.
                }
            }
            return focusable;
        }
        #endregion
    }

    public sealed class Chunk
    {
        /// <summary>
        /// The width or height of a <see cref="Chunk"/>.
        /// </summary>
        public const int Width  = 32,
                         Height = 32;

        private readonly List<Island> islands = new List<Island>();

        public Sea Sea { get; }
        public int XIndex { get; }
        public int YIndex { get; }
        public PointI Index { get; }

        public IReadOnlyList<Island> Islands => this.islands;

        public Chunk(Sea sea, int xIndex, int yIndex) {
            Sea = sea;
            XIndex = xIndex;
            YIndex = yIndex;
            Index = (XIndex, YIndex);
        }

        public void AddIsland(Island island) => this.islands.Add(island);
    }

    public readonly struct SeaTransformer : ITransformer<Sea>
    {
        bool ITransformer<Sea>.HasRotation => false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        PointF ITransformer<Sea>.PointTo(Sea sea, in PointF parent) {
            int height = sea.Master.Screen.Height;
            int ppu = sea.PPU;
            return new PointF(parent.X / ppu + sea.Camera.Left, (height - parent.Y) / ppu + sea.Camera.Bottom);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        PointF ITransformer<Sea>.PointFrom(Sea sea, in PointF local) {
            int height = sea.Master.Screen.Height;
            int ppu = sea.PPU;
            return new PointF((local.X - sea.Camera.Left) * ppu, height - (local.Y - sea.Camera.Bottom) * ppu);
        }

        Angle ITransformer<Sea>.AngleTo(Sea space, in Angle parent) => parent;
        Angle ITransformer<Sea>.AngleFrom(Sea space, in Angle local) => local;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        float ITransformer<Sea>.ScaleTo(Sea space, float parent) => parent / space.PPU;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        float ITransformer<Sea>.ScaleFrom(Sea space, float local) => local * space.PPU;
    }
}

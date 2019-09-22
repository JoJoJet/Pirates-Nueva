using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Pirates_Nueva.Path;

namespace Pirates_Nueva.Ocean
{
    public sealed class Sea : IGraph<Chunk>, ISpaceLocus<Sea>, IUpdatable, IDrawable<Screen>, IFocusableParent
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
        /// Finds and returns an entity that matches the specified predicate.
        /// </summary>
        public Entity FindEntity(Predicate<Entity> finder) => this.entities.First(e => finder(e));

        public bool IntersectsWithIsland(PointF start, PointF end) {
            var startChunk = new PointI((int)(start.X / Chunk.Width), (int)(start.Y / Chunk.Height));
            var endChunk   = new PointI((int)(end.X   / Chunk.Width), (int)(end.Y   / Chunk.Height));

            bool intersects = false;
            void step(int x, int y) {
                var ch = this[x, y];
                foreach(var i in ch.Islands) {
                    if(i.Intersects(start, end)) {
                        intersects = true;
                        break;
                    }
                }
            }

            if(startChunk == endChunk) {
                step(startChunk.X, startChunk.Y);
            }
            else {
                Bresenham.Line(startChunk, endChunk, step);
            }

            return intersects;
        }

        #region IGraph<> Implementation
        IEnumerable<Chunk> IGraph<Chunk>.Nodes {
            get {
                for(int x = 0; x < ChunksWidth; x++) {
                    for(int y = 0; y < ChunksHeight; y++) {
                        yield return this.chunks[x, y];
                    }
                }
            }
        }
        #endregion

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
        void IDrawable<Screen>.Draw(ILocalDrawer<Screen> topDrawer) {
            ILocalDrawer<Sea> drawer = new SpaceDrawer<Sea, SeaTransformer, Screen>(topDrawer, Transformer);
            //
            // Draw the grid of chunks
            for(int x = 0; x <= this.chunks.GetLength(0); x++) {
                drawer.DrawLine((x * Chunk.Width, 0), (x * Chunk.Width, this.chunks.GetLength(1) * Chunk.Height), in UI.Color.Black);
            }
            for(int y = 0; y <= this.chunks.GetLength(1); y++) {
                drawer.DrawLine((0, y * Chunk.Height), (this.chunks.GetLength(0) * Chunk.Width, y * Chunk.Height), in UI.Color.Black);
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

    public sealed class Chunk : INode<Chunk>
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

        #region INode<> Implementation
        IEnumerable<Edge<Chunk>> INode<Chunk>.Edges {
            get {
                if(check(0, 1, out var c))
                    yield return new Edge<Chunk>(1, c);
                if(check(1, 0, out c))
                    yield return new Edge<Chunk>(1, c);
                if(check(0, -1, out c))
                    yield return new Edge<Chunk>(1, c);
                if(check(-1, 0, out c))
                    yield return new Edge<Chunk>(1, c);

                const float Root2 = 1.41421356f;
                if(check(1, 1, out c))
                    yield return new Edge<Chunk>(Root2, c);
                if(check(1, -1, out c))
                    yield return new Edge<Chunk>(Root2, c);
                if(check(-1, -1, out c))
                    yield return new Edge<Chunk>(Root2, c);
                if(check(-1, 1, out c))
                    yield return new Edge<Chunk>(Root2, c);

                bool check(int xDir, int yDir, out Chunk chunk) {
                    int checkX = XIndex + xDir;
                    int checkY = YIndex + yDir;
                    //
                    // If the chunk that we're checking is off the grid, 
                    // return false right away.
                    if(checkX < 0  || checkX >= Sea.ChunksWidth || checkY < 0 || checkY >= Sea.ChunksHeight) {
                        chunk = null!;
                        return false;
                    }
                    //
                    // Find the initial points for three rays
                    // to cast into the chunk that we are checking.
                    const float ThirdW = (float)Width / 3,
                                HalfW  = (float)Width / 2,
                                ThirdH = (float)Height / 3,
                                HalfH  = (float)Height / 2;
                    var offset = (XIndex * Width, YIndex * Height);
                    (float x, float y) a = offset, b = offset, c = offset;
                    if(yDir != 0) {
                        a.x += ThirdW;
                        b.x += HalfW;
                        c.x += ThirdW * 2;
                    }
                    else {
                        a.x = b.x = c.x += HalfW;
                    }
                    if(xDir != 0) {
                        a.y += ThirdH;
                        b.y += HalfH;
                        c.y += ThirdH * 2;
                    }
                    else {
                        a.y = b.y = c.y += HalfH;
                    }
                    PointF ray = (xDir * Width, yDir * Height);
                    //
                    // Cast the ray against the current chunk and the one we're checking.
                    if(this.islands.Any(testIsland) || Sea[checkX, checkY].islands.Any(testIsland)) {
                        chunk = null!;
                        return false;
                    }
                    //
                    // If the chunk that we're checking is diagonal,
                    // also cast the ray against adjacent chunks.
                    if(xDir != 0 && yDir != 0 && (Sea[XIndex, checkY].islands.Any(testIsland) || Sea[checkX, YIndex].islands.Any(testIsland))) {
                        chunk = null!;
                        return false;
                    }
                    //
                    // If we got this far without returning,
                    // that means the chunk that we're checking
                    // both exists and has unimpeded passage from the current chunk.
                    chunk = Sea[checkX, checkY];
                    return true;

                    bool testIsland(Island i) => i.Intersects(a, a + ray) || i.Intersects(b, b + ray) || i.Intersects(c, c + ray);
                }
            }
        }
        #endregion
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

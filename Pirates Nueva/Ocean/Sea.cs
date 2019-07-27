using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Pirates_Nueva.Ocean
{
    public sealed class Sea : ISpaceLocus, IUpdatable, IDrawable<Master>, IFocusableParent
    {
        private readonly List<Entity> entities     = new List<Entity>(),
                                      addBuffer    = new List<Entity>(),
                                      removeBuffer = new List<Entity>();

        public Camera Camera { get; }

        /// <summary> The number of screen pixels corresponding to a unit within this <see cref="Sea"/>. </summary>
        public int PPU => (int)Camera.Zoom;

        public Archipelago Islands { get; }

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
            Islands = new Archipelago(this, new Random().Next());

            AddEntity(new PlayerShip(this, ShipDef.Get("dinghy")));
        }

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

        #region ISpaceLocus Implementation
        ISpaceLocus? ISpaceLocus.Parent => null;
        ISpace ISpaceLocus.Transformer => Transformer;
        #endregion

        #region IUpdatable Implementation
        void IUpdatable.Update(Master master, Time delta) {
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
                    u.Update(master, delta);    //     call its Update() method.
            }
        }
        #endregion

        #region IDrawable<> Implementation
        void IDrawable<Master>.Draw(ILocalDrawer<Master> topDrawer) {
            var drawer = new SpaceDrawer<Sea, SeaTransformer, Master>(topDrawer, Transformer);

            (Islands as IDrawable<Sea>).Draw(drawer);

            foreach(var ent in this.entities) { // For every entity:
                if(ent is IDrawable<Sea> d)     // If it is drawable,
                    d.Draw(drawer);             //     call its Draw() method.
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

        public sealed class Archipelago : IEnumerable<Island>, IDrawable<Sea>
        {
            private readonly Sea sea;
            private readonly Island?[,] islands;

            public Archipelago(Sea sea, int seed) {
                this.sea = sea;

                const int Width = 10, Height = 10;
                const int Chance = 45;

                var r = new Random(seed);
                
                /*
                 * Begin generating all of the islands.
                 */
                this.islands = new Island[Width, Height];
                for(int x = 0; x < Width; x++) {                                              // For every point in a square region:
                    for(int y = 0; y < Height; y++) {                                         //
                        if(r.Next(0, 100) < Chance) {                                         // If we roll a random chance,
                            islands[x, y] = new Island(this.sea, x * 180, y * 180, r.Next()); //     Create an island at this point.
                        }
                    }
                }
            }

            /// <summary> Gets the <see cref="Island"> at the specified index. </summary>
            public Island? this[int x, int y] {
                get {
                    if(x >= 0 && x < islands.GetLength(0) && y >= 0 && y < islands.GetLength(1))
                        return this.islands[x, y];
                    else
                        return null;
                }
            }

            #region IEnumerable Implementation
            public IEnumerator<Island> GetEnumerator() {
                for(int x = 0; x < islands.GetLength(0); x++) {     // For every spot in the archipelago:
                    for(int y = 0; y < islands.GetLength(1); y++) { //
                        if(islands[x, y] is Island isl)             // If there is an island there,
                            yield return isl;                       //     return it.
                    }
                }
            }
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
            #endregion

            #region IDrawable Implementation
            void IDrawable<Sea>.Draw(ILocalDrawer<Sea> drawer) {
                foreach(IDrawable<Sea> i in this) { // For every island in this archipelago:
                    i.Draw(drawer);                 // Call its Draw() method.
                }
            }
            #endregion
        }
    }

    public readonly struct SeaTransformer : ITransformer<Sea>
    {
        bool ITransformer<Sea>.HasRotation => false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        PointF ITransformer<Sea>.PointTo(Sea sea, in PointF parent) {
            int height = sea.Master.GUI.ScreenHeight;
            int ppu = sea.PPU;
            return new PointF(parent.X / ppu + sea.Camera.Left, (height - parent.Y) / ppu + sea.Camera.Bottom);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        PointF ITransformer<Sea>.PointFrom(Sea sea, in PointF local) {
            int height = sea.Master.GUI.ScreenHeight;
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

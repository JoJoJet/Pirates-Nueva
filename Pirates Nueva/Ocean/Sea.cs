using System;
using System.Collections.Generic;
using System.Linq;
using static Pirates_Nueva.NullableUtil;

namespace Pirates_Nueva.Ocean
{
    public sealed class Sea : IUpdatable, IDrawable<Master>, IFocusableParent
    {
        private readonly List<Entity> entities = new List<Entity>();
        private readonly List<Entity> addBuffer = new List<Entity>();
        private readonly List<Entity> removeBuffer = new List<Entity>();

        public Camera Camera { get; }

        /// <summary> The number of screen pixels corresponding to a unit within this <see cref="Sea"/>. </summary>
        public int PPU => (int)Camera.Zoom;

        public Archipelago Islands { get; }

        /// <summary> The position of the mouse, in <see cref="Sea"/>-space. </summary>
        public PointF MousePosition => ScreenPointToSea(Master.Input.MousePosition);
        
        private Master Master { get; }

        internal Sea(Master master) {
            Master = master;
            Camera = new Camera(this);

            // Generate the islands.
            Islands = new Archipelago(this);
            var isl = Islands as IArchiContract;
            isl.Generate(new Random().Next());

            AddEntity(new PlayerShip(this, ShipDef.Get("dinghy")));
            AddEntity(new EnemyShip(this, ShipDef.Get("dinghy"), 5, 30));
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

        void IDrawable<Master>.Draw(ILocalDrawer<Master> topDrawer) {
            var drawer = new SeaDrawer(topDrawer, this);

            (Islands as IDrawable<Sea>).Draw(drawer);

            foreach(var ent in this.entities) { // For every entity:
                if(ent is IDrawable<Sea> d)     // If it is drawable,
                    d.Draw(drawer);             //     call its Draw() method.
            }
        }

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

        #region Space Transformation
        /// <summary>
        /// Transform the input <see cref="PointI"/> from screen space to a <see cref="PointF"/> within this <see cref="Sea"/>.
        /// </summary>
        /// <param name="screenPoint">A pair of coordinates in screen space.</param>
        public PointF ScreenPointToSea(PointI screenPoint) => ScreenPointToSea(screenPoint.X, screenPoint.Y);
        /// <summary>
        /// Transform the input coordinates from screen space to a pair of coordinates within this <see cref="Sea"/>.
        /// </summary>
        /// <param name="x">The x coordinate local to the screen.</param>
        /// <param name="y">The y coordinate local to the screen.</param>
        internal (float x, float y) ScreenPointToSea(int x, int y) {
            int height = Master.GUI.ScreenHeight;
            return ((float)x / PPU + Camera.Left, (float)(height - y) / PPU + Camera.Bottom);
        }

        /// <summary>
        /// Transform the input <see cref="PointF"/> from <see cref="Sea"/>- to screen-space.
        /// </summary>
        /// <param name="seaPoint">A pair of coordinates within this <see cref="Sea"/>.</param>
        public PointI SeaPointToScreen(PointF seaPoint) => SeaPointToScreen(seaPoint.X, seaPoint.Y);
        /// <summary>
        /// Transform the input coordinates from this <see cref="Sea"/> to the screen.
        /// </summary>
        /// <param name="x">The x coordinate local to this <see cref="Sea"/>.</param>
        /// <param name="y">The y coordinate local to this <see cref="Sea"/>.</param>
        internal (int x, int y) SeaPointToScreen(float x, float y) {
            int height = Master.GUI.ScreenHeight;
            int ppu = PPU;
            return ((int)((x - Camera.Left) * ppu), (int)(height - (y - Camera.Bottom) * ppu));
        }
        #endregion

        /// <summary> Allows some methods to be accessible only within the <see cref="Sea"/> class. </summary>
        private interface IArchiContract
        {
            void Generate(int seed);
        }
        public sealed class Archipelago : IEnumerable<Island>, IDrawable<Sea>, IArchiContract
        {
            private Sea sea;
            private Island[,]? islands;

            internal Archipelago(Sea sea) {
                this.sea = sea;
            }

            /// <summary> Gets the <see cref="Island"> at the specified index. </summary>
            public Island? this[int x, int y] {
                get {
                    if(islands == null) {
                        ThrowNotInitialized();
                        return null;
                    }
                    if(x >= 0 && x < islands.GetLength(0) && y >= 0 && y < islands.GetLength(1))
                        return this.islands[x, y];
                    else
                        return null;
                }
            }

            void IArchiContract.Generate(int seed) {
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

            #region IEnumerable Implementation
            public IEnumerator<Island> GetEnumerator() {
                if(islands == null) {
                    ThrowNotInitialized();
                    yield break;
                }
                for(int x = 0; x < islands.GetLength(0); x++) {     // For every spot in the archipelago:
                    for(int y = 0; y < islands.GetLength(1); y++) { //
                        if(islands[x, y] != null)                   // If there is an island there,
                            yield return islands[x, y];             //     return it.
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

    internal sealed class SeaDrawer : ILocalDrawer<Sea>
    {
        private ILocalDrawer<Master> Drawer { get; }
        private Sea Sea { get; }

        public SeaDrawer(ILocalDrawer<Master> drawer, Sea sea) {
            Drawer = drawer;
            Sea = sea;
        }

        public void DrawCorner(UI.Sprite sprite, float left, float top, float width, float height, in UI.Color tint) {
            var (screenX, screenY) = Sea.SeaPointToScreen(left, top);
            var (screenW, screenH) = (width * Sea.PPU, height * Sea.PPU);

            Drawer.DrawCorner(sprite, screenX, screenY, (int)screenW, (int)screenH, in tint);
        }
        public void Draw(UI.Sprite sprite, float x, float y, float width, float height,
                         in Angle angle, in PointF origin, in UI.Color tint) {
            var (screenX, screenY) = Sea.SeaPointToScreen(x, y);
            var (screenW, screenH) = (width * Sea.PPU, height * Sea.PPU);

            Drawer.Draw(sprite, screenX, screenY, (int)screenW, (int)screenH, in angle, in origin, in tint);
        }
        public void DrawLine(PointF start, PointF end, in UI.Color color)
            => Drawer.DrawLine(Sea.SeaPointToScreen(start), Sea.SeaPointToScreen(end), in color);

        public void DrawString(UI.Font font, string text, float left, float top, in UI.Color color) => throw new NotImplementedException();
    }
}

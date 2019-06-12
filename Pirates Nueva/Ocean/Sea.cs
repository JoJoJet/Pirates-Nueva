using System;
using System.Collections.Generic;
using static Pirates_Nueva.NullableUtil;

namespace Pirates_Nueva.Ocean
{
    public sealed class Sea : IUpdatable, IDrawable, IFocusableParent
    {
        private readonly List<Entity> entities = new List<Entity>();

        public Camera Camera { get; }

        /// <summary> The number of screen pixels corresponding to a unit within this <see cref="Sea"/>. </summary>
        public int PPU => (int)Math.Round(Camera.Zoom);

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
            isl.Generate(new Random().Next(), master);

            this.entities.Add(new PlayerShip(this, ShipDef.Get("dinghy")));
        }

        void IUpdatable.Update(Master master, Time delta) {
            foreach(var ent in this.entities) { // For every entity:
                if(ent is IUpdatable u)         // If it is updatable,
                    u.Update(master, delta);    //     call its Update() method.
            }
        }

        void IDrawable.Draw(Master master) {
            (Islands as IDrawable).Draw(master);

            foreach(var ent in this.entities) { // For every entity:
                if(ent is IDrawable d)          // If it is drawable,
                    d.Draw(master);             //     call its Draw() method.
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
            return ((int)Math.Round((x - Camera.Left) * PPU), (int)Math.Round(height - (y - Camera.Bottom) * PPU));
        }
        #endregion

        /// <summary> Allows some methods to be accessible only within the <see cref="Sea"/> class. </summary>
        private interface IArchiContract
        {
            void Generate(int seed, Master master);
        }
        public sealed class Archipelago : IEnumerable<Island>, IDrawable, IArchiContract
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

            void IArchiContract.Generate(int seed, Master master) {
                const int Width = 10;
                const int Height = 10;
                const int Chance = 45;

                var r = new Random(seed);
                
                /*
                 * Begin generating all of the islands.
                 */
                this.islands = new Island[Width, Height];
                for(int x = 0; x < Width; x++) {                                    // For every point in a square area:
                    for(int y = 0; y < Height; y++) {                               //
                        if(r.Next(0, 100) < Chance) {                               // If we roll a random chance:
                            islands[x, y] = new Island(this.sea, x * 180, y * 180); //     Create an island at this point,
                            islands[x, y].Generate(r.Next(), master);               //     start to generate the island,
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
            void IDrawable.Draw(Master master) {
                foreach(IDrawable i in this) {  // For every island in this archipelago:
                    i.Draw(master);             // Call its Draw() method.
                }
            }
            #endregion
        }
    }
}

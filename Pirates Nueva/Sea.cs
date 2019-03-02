using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pirates_Nueva
{
    public sealed class Sea : IUpdatable, IDrawable, IFocusableParent
    {
        private readonly List<Entity> entities = new List<Entity>();

        /// <summary> The number of screen pixels corresponding to a unit within this <see cref="Sea"/>. </summary>
        public int PPU => 32;

        public Archipelago Islands { get; }

        private Master Master { get; }

        const string MouseDebugID = "debug_mouse";
        public Sea(Master master) {
            Master = master;

            // Generate the islands.
            Islands = new Archipelago(this);
            var isl = Islands as IArchiContract;
            Task.Run(async () => await isl.GenerateAsync(new Random().Next(), master)).Wait();

            this.entities.Add(new PlayerShip(this, 10, 5));

            master.GUI.AddEdge(MouseDebugID, new UI.EdgeText("mouse position", master.Font, GUI.Edge.Top, GUI.Direction.Right));
        }

        void IUpdatable.Update(Master master, Time delta) {
            foreach(var ent in this.entities) { // For every entity:
                if(ent is IUpdatable u)         // If it is updatable,
                    u.Update(master, delta);    //     call its Update() method.
            }

            if(master.GUI.TryGetEdge<UI.EdgeText>(MouseDebugID, out var tex)) {
                tex.Text = $"Mouse: {ScreenPointToSea(master.Input.MousePosition)}";
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
            return ((float)x / PPU, (float)(height - y) / PPU);
        }

        /// <summary>
        /// Transform the input <see cref="PointF"/> from this <see cref="Sea"/> to <see cref="PointI"/> local to the screen.
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
            return ((int)Math.Round(x *  PPU), (int)Math.Round(height - y * PPU));
        }
        #endregion

        /// <summary> Allows some methods to be accessible only within the <see cref="Sea"/> class. </summary>
        private interface IArchiContract
        {
            Task GenerateAsync(int seed, Master master);
        }
        public sealed class Archipelago : IEnumerable<Island>, IDrawable, IArchiContract
        {
            private Sea sea;
            private Island[,] islands;

            internal Archipelago(Sea sea) {
                this.sea = sea;
            }

            /// <summary> Get the <see cref="Island"> at the specified index. </summary>
            public Island this[int x, int y] {
                get {
                    if(x >= 0 && x < islands.GetLength(0) && y >= 0 && y < islands.GetLength(1))
                        return islands[x, y];
                    else
                        return null;
                }
            }

            async Task IArchiContract.GenerateAsync(int seed, Master master) {
                const int Width = 10;
                const int Height = 10;
                const int Chance = 25;

                var r = new Random(seed);
                var gens = new List<Task>(Width*Height * Chance/100); // A list of tasks for generating the shape of islands.
                
                /*
                 * Begin generating all of the islands.
                 */
                this.islands = new Island[Width, Height];
                for(int x = 0; x < Width; x++) {                                      // For every point in a square area:
                    for(int y = 0; y < Height; y++) {                                 //
                        if(r.Next(0, 100) < Chance) {                                 // If we roll a random chance:
                            islands[x, y] = new Island(this.sea, x * 30, y * 30);     //     Create an island at this point,
                            var gen = islands[x, y].GenerateAsync(r.Next(), master);  //     start to generate the island,
                            gens.Add(gen);                                            //     and store the task for generating it.
                        }
                    }
                }

                /*
                 * Wait until all of the islands finish generating.
                 */
                foreach(var gen in gens) { // For every island generation task:
                    await gen;             // wait until the task finishes execution.
                }
            }

            #region IEnumerable Implementation
            public IEnumerator<Island> GetEnumerator() {
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

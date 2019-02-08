using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Pirates_Nueva
{
    public class Ship : IUpdatable, IDrawable
    {
        private readonly Block[,] blocks;

        public Sea Parent { get; }

        /// <summary> The horizontal length of this <see cref="Ship"/>. </summary>
        public int Width => this.blocks.GetLength(0);
        /// <summary> The vertical length of this <see cref="Ship"/>. </summary>
        public int Height => this.blocks.GetLength(1);

        /// <summary>
        /// Create a ship with specified /width/ and /height/.
        /// </summary>
        public Ship(Sea parent, int width, int height) {
            Parent = parent;

            this.blocks = new Block[width, height];

            PlaceBlock("root", Width/2, Height/2);
        }

        /// <summary>
        /// Get the block at position (/x/, /y/).
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if either index exceeds the bounds of this <see cref="Ship"/>.</exception>
        public Block this[int x, int y] => GetBlock(x, y);

        /// <summary>
        /// Get the block at position (/x/, /y/).
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if either index exceeds the bounds of this <see cref="Ship"/>.</exception>
        public Block GetBlock(int x, int y) {
            try {
                ValidateIndices($"{nameof(Ship)}.{nameof(GetBlock)}()", x, y);
            }
            catch(ArgumentOutOfRangeException) {
                throw;
            }

            return this.blocks[x, y];
        }
        /// <summary>
        /// Place a block of type /id/ at position (/x/, /y/).
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if either index exceeds the bounds of this <see cref="Ship"/>.</exception>
        public Block PlaceBlock(string id, int x, int y) {
            try {
                ValidateIndices($"{nameof(Ship)}.{nameof(PlaceBlock)}()", x, y);
            }
            catch(ArgumentOutOfRangeException) {
                throw;
            }

            return this.blocks[x, y] = new Block(id, x, y);
        }

        /// <summary> Throw an exception if either index is out of range. </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if either index exceeds the bounds of this <see cref="Ship"/>.</exception>
        private void ValidateIndices(string methodName, int x, int y) {
            if(x < 0 || x >= Width)
                throw new ArgumentOutOfRangeException(
                    nameof(x),
                    $@"{methodName}: Argument must be on the interval [0, {Width}). Its value is ""{x}""!"
                    );
            if(y < 0 || y >= Height)
                throw new ArgumentOutOfRangeException(
                    nameof(y),
                    $@"{methodName}: Argument must be on the interval [0, {Height}). Its value is ""{y}""!"
                    );
        }

        public void Update(Master master) {

        }

        /// <summary>
        /// Draw this <see cref="Ship"/> onscreen.
        /// </summary>
        public void Draw(Master master) {
            for(int x = 0; x < Width; x++) {
                for(int y = 0; y < Height; y++) {
                    if(this.blocks[x, y] != null) {
                        this[x, y].Draw(master);
                    }
                }
            }
        }
    }
}

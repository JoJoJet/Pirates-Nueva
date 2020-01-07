using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Pirates_Nueva.Ocean
{
    /// <summary>
    /// An object that contains a grid of Block objects.
    /// </summary>
    public abstract class BlockContainer<TBlock>
        where TBlock : class
    {
        /// <summary>
        /// Returns the Block at indices (<paramref name="x"/>, <paramref name="y"/>),
        /// or null if it does not exist.
        /// </summary>
        public TBlock? GetBlockOrNull(int x, int y)
        {
            var blocks = GetBlockGrid();
            if(x >= 0 && y >= 0 && x < blocks.GetLength(0) && y < blocks.GetLength(1))
                return blocks[x, y];
            else
                return null;
        }
        /// <summary>
        /// Gets the Block at indices (<paramref name="x"/>, <paramref name="y"/>), if it exists.
        /// </summary>
        public bool TryGetBlock(int x, int y, [NotNullWhen(true)] out TBlock? block)
        {
            var blocks = GetBlockGrid();
            if(x >= 0 && y >= 0 && x < blocks.GetLength(0) && y < blocks.GetLength(1) && blocks[x, y] is TBlock b) {
                block = b;
                return true;
            }
            else {
                block = null;
                return false;
            }
        }
        /// <summary>
        /// Returns whether or not there is a Block at indices (<paramref name="x"/>, <paramref name="y"/>).
        /// </summary>
        public bool HasBlock(int x, int y)
        {
            var blocks = GetBlockGrid();
            return x >= 0 && y >= 0 && x < blocks.GetLength(0) && y < blocks.GetLength(1) && blocks[x, y] is TBlock b;
        }

        /// <summary>
        /// Accessor for the grid of blocks.
        /// </summary>
        protected abstract TBlock?[,] GetBlockGrid();
    }
}

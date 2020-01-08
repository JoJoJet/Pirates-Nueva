using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Pirates_Nueva.Ocean.Agents
{
    /// <summary>
    /// A base class for a grid of blocks that implements <see cref="IAgentContainer{TSelf, TSpot}"/>.
    /// </summary>
    /// <typeparam name="TSelf">
    /// The type that implements this class,
    /// which must also implement <see cref="IAgentContainer{TSelf, TSpot}"/>.</typeparam>
    public abstract class AgentBlockContainer<TSelf, TBlock>
        where TSelf : AgentBlockContainer<TSelf, TBlock>, IAgentContainer<TSelf, TBlock>
        where TBlock : class, IAgentSpot<TSelf, TBlock>
    {
        protected readonly List<Agent<TSelf, TBlock>> agents = new List<Agent<TSelf, TBlock>>();
        protected readonly List<Job<TSelf, TBlock>> jobs = new List<Job<TSelf, TBlock>>();

        #region Blocks
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
        #endregion

        #region Stock
        /// <summary>
        /// Gets the Stock at indices (<paramref name="x"/>, <paramref name="y"/>) if there is any.
        /// </summary>
        public bool TryGetStock(int x, int y, [NotNullWhen(true)] out Stock<TSelf, TBlock>? stock)
            => (stock = GetBlockOrNull(x, y)?.Stock) != null;

        /// <summary>
        /// Returns the Stock at indices (<paramref name="x"/>, <paramref name="y"/>),
        /// or returns null if no such Stock exists.
        /// </summary>
        public Stock<TSelf, TBlock>? GetStockOrNull(int x, int y) => GetBlockOrNull(x, y)?.Stock;
        
        /// <summary>
        /// Places Stock with specified <see cref="ItemDef"/> at the specified position.
        /// </summary>
        public Stock<TSelf, TBlock> PlaceStock(int x, int y, ItemDef def)
        {
            string Sig() => $"{typeof(TSelf).Name}.{nameof(PlaceStock)}()";

            if(GetBlockOrNull(x, y) is TBlock b) {
                if(b.Stock is null) {
                    return b.Stock = new Stock<TSelf, TBlock>(def, (this as TSelf)!, b);
                }
                throw new InvalidOperationException(
                    $"{Sig()}: There is already a {nameof(Stock<TSelf, TBlock>)} at indices ({x}, {y})!"
                    );
            }
            throw new InvalidOperationException(
                $"{Sig()}: There is no {typeof(TBlock)} at indices ({x}, {y})!"
                );
        }
        #endregion

        #region Agents
        /// <summary>
        /// Returns the Agent at indices (<paramref name="x"/>, <paramref name="y"/>), if there is one.
        /// </summary>
        public bool TryGetAgent(int x, int y, [NotNullWhen(true)] out Agent<TSelf, TBlock>? agent)
        {
            foreach(var a in this.agents) {
                if(a.CurrentSpot.Index == (x, y)) {
                    agent = a;
                    return true;
                }
            }
            agent = null;
            return false;
        }

        /// <summary>
        /// Returns the Agent at indices (<paramref name="x"/>, <paramref name="y"/>),
        /// or null if no such Agent exists.
        /// </summary>
        public Agent<TSelf, TBlock>? GetAgentOrNull(int x, int y)
        {
            foreach(var a in this.agents) {
                if(a.CurrentSpot.Index == (x, y))
                    return a;
            }
            return null;
        }

        /// <summary>
        /// Adds an Agent to the Block at indices (<paramref name="x"/>, <paramref name="y"/>),
        /// or throws an <see cref="InvalidOperationException"/> if no such Block exists.
        /// </summary>
        public Agent<TSelf, TBlock> AddAgent(int x, int y)
        {
            if(GetBlockOrNull(x, y) is TBlock b) {
                var agent = new Agent<TSelf, TBlock>((this as TSelf)!, b);
                this.agents.Add(agent);
                return agent;
            }
            throw new InvalidOperationException(
                $"{typeof(TSelf).Name}.{nameof(AddAgent)}(): There is no {typeof(TBlock).Name} on which to place the Agent!"
                );
        }
        #endregion

        #region Jobs
        /// <summary>
        /// Returns a Job that can currently be worked on by the specified Agent,
        /// or returns null if no such Job exists.
        /// </summary>
        public Job<TSelf, TBlock>? GetWorkableJob(Agent<TSelf, TBlock> hiree)
        {
            foreach(var j in this.jobs) {
                if(j.Worker is null && j.Qualify(hiree, out _))
                    return j;
            }
            return null;
        }

        /// <summary>
        /// Creates a Job at the specified indices.
        /// </summary>
        public Job<TSelf, TBlock> CreateJob(int x, int y, Job<TSelf, TBlock>.Toil task)
        {
            var j = new Job<TSelf, TBlock>((this as TSelf)!, x, y, task);
            this.jobs.Add(j);
            return j;
        }

        /// <summary>
        /// Removes the specified Job.
        /// </summary>
        public void RemoveJob(Job<TSelf, TBlock> job) => this.jobs.Remove(job);
        #endregion
    }
}

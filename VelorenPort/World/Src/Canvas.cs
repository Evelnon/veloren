using System;
using System.Collections.Generic;
using VelorenPort.NativeMath;

namespace VelorenPort.World {
    /// <summary>
    /// Editable view of a terrain chunk with access to column data.
    /// This is a greatly simplified version of the original Rust Canvas.
    /// </summary>
    [Serializable]
    public struct CanvasInfo {
        public int2 ChunkPos;
        public int2 Wpos;
        public ColumnGen Gen;
        public SimChunk SimChunk;
        public WorldSim Sim;

        public CanvasInfo(int2 chunkPos, WorldSim sim, SimChunk simChunk) {
            ChunkPos = chunkPos;
            Sim = sim;
            SimChunk = simChunk;
            Gen = new ColumnGen(sim);
            Wpos = TerrainChunkSize.CposToWpos(chunkPos);
        }
    }

    /// <summary>
    /// Provides read and write access to chunk blocks and iterates over columns.
    /// </summary>
    [Serializable]
    public class Canvas {
        private readonly CanvasInfo _info;
        private readonly Chunk _chunk;
        private readonly List<int3> _resourceBlocks = new();
        private readonly List<int3> _spawns = new();
        private readonly List<FaunaSpawn> _faunaSpawns = new();

        public Canvas(CanvasInfo info, Chunk chunk) {
            _info = info;
            _chunk = chunk;
        }

        public CanvasInfo Info => _info;
        public Chunk Chunk => _chunk;

        public Block GetBlock(int3 pos) => _chunk[pos.x, pos.y, pos.z];

        public void SetBlock(int3 pos, Block block) {
            if (block.GetRtsimResource() != null)
                AddResourceBlock(pos);
            _chunk[pos.x, pos.y, pos.z] = block;
        }

        /// <summary>
        /// Mark <paramref name="pos"/> as containing a resource block such as
        /// ore or a special structure.
        /// </summary>
        public void AddResourceBlock(int3 pos) {
            if (!_resourceBlocks.Contains(pos)) _resourceBlocks.Add(pos);
        }

        /// <summary>Remove a previously added resource block.</summary>
        public void RemoveResourceBlock(int3 pos) => _resourceBlocks.Remove(pos);

        /// <summary>List of resource blocks currently registered.</summary>
        public IReadOnlyList<int3> ResourceBlocks => _resourceBlocks;

        /// <summary>Remove all resource block markers.</summary>
        public void ClearResourceBlocks() => _resourceBlocks.Clear();

        /// <summary>
        /// Enumerate all columns within this canvas and invoke <paramref name="action"/> with the sampled data.
        /// </summary>
        public void ForEachColumn(Action<int2, ColumnSample> action) {
            int2 size = TerrainChunkSize.RectSize;
            for (int y = 0; y < size.y; y++)
            for (int x = 0; x < size.x; x++) {
                int2 wpos = _info.Wpos + new int2(x, y);
                var sample = _info.Gen.Get((wpos, (object)null!, (object?)null));
                if (sample != null) {
                    action(wpos, sample);
                }
            }
        }

        /// <summary>
        /// Retrieve a <see cref="Column"/> representing blocks at the given
        /// world position.
        /// </summary>
        public Column GetColumn(int2 wpos) {
            int2 local = wpos - _info.Wpos;
            var column = new Column(wpos, Block.Air);
            for (int z = 0; z < Chunk.Height; z++) {
                column[z] = _chunk[local.x, local.y, z];
            }
            return column;
        }

        /// <summary>
        /// Write the provided column back into this canvas.
        /// </summary>
        public void SetColumn(Column column) {
            int2 local = column.Position - _info.Wpos;
            for (int z = 0; z < Chunk.Height; z++) {
                _chunk[local.x, local.y, z] = column[z];
            }
        }

        /// <summary>Write multiple columns back to the canvas.</summary>
        public void SetColumns(IEnumerable<Column> columns) {
            foreach (var col in columns) SetColumn(col);
        }

        /// <summary>
        /// Fill the entire column at <paramref name="wpos"/> with the given block.
        /// </summary>
        public void FillColumn(int2 wpos, Block block) {
            int2 local = wpos - _info.Wpos;
            for (int z = 0; z < Chunk.Height; z++)
                _chunk[local.x, local.y, z] = block;
        }

        /// <summary>Register that an entity should spawn at <paramref name="pos"/>.</summary>
        public void Spawn(int3 pos) => Spawn(pos, FaunaKind.Generic);

        /// <summary>Register a specific type of wildlife spawn.</summary>
        public void Spawn(int3 pos, FaunaKind kind)
        {
            _spawns.Add(pos);
            _faunaSpawns.Add(new FaunaSpawn(pos, kind));
        }

        /// <summary>List of spawn points queued during generation.</summary>
        public IReadOnlyList<int3> Spawns => _spawns;
        public IReadOnlyList<FaunaSpawn> FaunaSpawns => _faunaSpawns;

        /// <summary>
        /// Copy registered resource blocks into the provided
        /// <see cref="ChunkSupplement"/> so generation code can
        /// persist them. This does not clear the local lists.
        /// </summary>
        public void WriteSupplementData(ChunkSupplement supplement)
        {
            foreach (var pos in _resourceBlocks)
            {
                supplement.ResourceBlocks.Add(pos);
                var kind = _chunk[pos.x, pos.y, pos.z].Kind;
                supplement.ResourceDeposits.Add(new ResourceDeposit(pos, kind));
            }
            foreach (var spawn in _faunaSpawns)
            {
                supplement.Wildlife.Add(spawn);
                supplement.WildlifeEntities.Add(new WildlifeEntity(spawn.Position, spawn.Kind));
            }
            foreach (var pos in _spawns)
                supplement.SpawnPoints.Add(pos);
        }

        /// <summary>
        /// Search vertically near <paramref name="wpos"/> for a walkable spawn location.
        /// Returns <c>null</c> if none was found within the search distance.
        /// </summary>
        public int3? FindSpawnPos(int3 wpos)
        {
            const int height = 2;
            const int searchDist = 8;

            int lx = wpos.x - _info.Wpos.x;
            int ly = wpos.y - _info.Wpos.y;
            if (lx < 0 || lx >= Chunk.Size.x || ly < 0 || ly >= Chunk.Size.y)
                return null;

            for (int dz = searchDist * 2; dz >= 1; dz--)
            {
                int z = (dz % 2 != 0) ? wpos.z + dz / 2 : wpos.z - dz / 2;
                if (z <= 0 || z >= Chunk.Height - height)
                    continue;

                bool belowSolid = _chunk[lx, ly, z - 1].Kind.IsFilled();
                bool space = true;
                for (int i = 0; i < height; i++)
                {
                    if (!_chunk[lx, ly, z + i].Kind.IsFluid())
                    {
                        space = false;
                        break;
                    }
                }

                if (belowSolid && space)
                    return new int3(wpos.x, wpos.y, z);
            }

            return null;
        }
    }
}

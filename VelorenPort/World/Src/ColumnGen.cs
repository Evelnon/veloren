using System;
using Unity.Mathematics;

namespace VelorenPort.World {
    /// <summary>
    /// Handles sampling of world columns for terrain and object generation.
    /// Port in progress from <c>column.rs</c>.
    /// </summary>
    internal class ColumnGen {
        private readonly WorldSim _sim;

        public ColumnGen(WorldSim sim) {
            _sim = sim;
        }

        public object Get((int2 Wpos, object Index, object? Calendar) input) {
            throw new NotImplementedException();
        }
    }
}

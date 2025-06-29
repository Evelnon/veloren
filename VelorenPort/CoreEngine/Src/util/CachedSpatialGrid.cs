using System;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Resource storing a SpatialGrid for reuse between systems during a tick.
    /// </summary>
    [Serializable]
    public class CachedSpatialGrid {
        public SpatialGrid Grid { get; }

        public CachedSpatialGrid() {
            const int Lg2CellSize = 5;      // 32
            const int Lg2LargeCellSize = 6; // 64
            const uint RadiusCutoff = 8;
            Grid = new SpatialGrid(Lg2CellSize, Lg2LargeCellSize, RadiusCutoff);
        }
    }
}

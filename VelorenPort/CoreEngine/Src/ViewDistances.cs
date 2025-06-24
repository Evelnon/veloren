using System;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Configuration for terrain and entity view distances.
    /// Mirrors common/src/view_distances.rs
    /// </summary>
    [Serializable]
    public struct ViewDistances {
        public uint Terrain;
        public uint Entity;

        public ViewDistances(uint terrain, uint entity) {
            Terrain = terrain;
            Entity = entity;
        }

        /// <summary>
        /// Clamp values to given maximums. Terrain distance is clamped to
        /// the optional max and entity distance is clamped to the resulting
        /// terrain value. Values are never below 1 unless max is 0.
        /// </summary>
        public ViewDistances Clamp(uint? max) {
            uint terrain = Terrain;
            if (max.HasValue) {
                uint m = max.Value;
                if (m == 0) terrain = 0;
                else if (terrain < 1) terrain = 1;
                terrain = Math.Min(terrain, m);
            } else {
                if (terrain < 1) terrain = 1;
            }

            uint entity = Entity;
            if (terrain > 0) {
                if (entity < 1) entity = 1;
                entity = Math.Min(entity, terrain);
            }

            return new ViewDistances(terrain, entity);
        }
    }
}


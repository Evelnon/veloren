using System;
using System.Collections.Generic;
using VelorenPort.NativeMath;

namespace VelorenPort.World.Sim
{
    /// <summary>
    /// Configuration used when evaluating map based pathfinding costs.
    /// </summary>
    [Serializable]
    public struct MapCostConfig
    {
        public float AltitudeWeight;
        public Dictionary<ConnectionKind, float>? ConnectionWeights;

        public MapCostConfig(float altitudeWeight,
            Dictionary<ConnectionKind, float>? connectionWeights = null)
        {
            AltitudeWeight = altitudeWeight;
            ConnectionWeights = connectionWeights;
        }
    }

    /// <summary>
    /// Helper functions for evaluating additional pathfinding costs
    /// using map sampling information.
    /// </summary>
    public static class MapCost
    {
        /// <summary>
        /// Compute the cost of moving from <paramref name="from"/> to
        /// <paramref name="to"/> in the direction <paramref name="dirIdx"/>
        /// according to <paramref name="cfg"/>.
        /// </summary>
        public static float Compute(MapSample from, MapSample to, int dirIdx, MapCostConfig cfg)
        {
            float cost = 0f;
            cost += math.abs((float)(to.Alt - from.Alt)) * cfg.AltitudeWeight;

            if (cfg.ConnectionWeights != null && from.Connections != null &&
                dirIdx >= 0 && dirIdx < from.Connections.Length)
            {
                var conn = from.Connections[dirIdx];
                if (conn.HasValue &&
                    cfg.ConnectionWeights.TryGetValue(conn.Value.Kind, out var w))
                {
                    cost += w;
                }
            }

            return cost;
        }
    }
}

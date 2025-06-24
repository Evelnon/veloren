using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Utility methods to iterate coordinates in a rectangular spiral pattern.
    /// Port of common/src/spiral.rs adapted for idiomatic C#.
    /// </summary>
    [Serializable]
    public static class Spiral {
        private static IEnumerable<int2> SpiralFrom(int startLayer) {
            int layer = startLayer;
            int i = 0;
            while (true) {
                int layerSize = Math.Max(1, layer * 8 + 4 * Math.Min(layer, 1) - 4);
                if (i >= layerSize) {
                    layer++;
                    i = 0;
                    layerSize = Math.Max(1, layer * 8 + 4 * Math.Min(layer, 1) - 4);
                }

                int Clamp(int v, int min, int max) => v < min ? min : (v > max ? max : v);
                int2 pos = new int2(
                    -layer + Clamp(i - (layerSize / 4) * 0, 0, layer * 2)
                            - Clamp(i - (layerSize / 4) * 2, 0, layer * 2),
                    -layer + Clamp(i - (layerSize / 4) * 1, 0, layer * 2)
                            - Clamp(i - (layerSize / 4) * 3, 0, layer * 2)
                );

                yield return pos;
                i++;
            }
        }

        /// <summary>Infinite sequence of spiral coordinates starting at the origin.</summary>
        public static IEnumerable<int2> Infinite() => SpiralFrom(0);

        /// <summary>All coordinates within the given radius.</summary>
        public static IEnumerable<int2> WithRadius(int radius) {
            int limit = (radius * 2 + 1) * (radius * 2 + 1);
            int maxDist2 = (radius + 1) * (radius + 1);
            int count = 0;
            foreach (var pos in Infinite()) {
                if (count++ >= limit) yield break;
                int dist2 = pos.x * pos.x + pos.y * pos.y;
                if (dist2 < maxDist2) yield return pos;
            }
        }

        /// <summary>Coordinates on the edge of a circle with the given radius.</summary>
        public static IEnumerable<int2> WithEdgeRadius(int radius) {
            int limit = (radius * 2 + 1) * (radius * 2 + 1);
            int maxDist2 = (radius + 1) * (radius + 1);
            int minDist2 = radius * radius;
            int count = 0;
            foreach (var pos in Infinite()) {
                if (count++ >= limit) yield break;
                int dist2 = pos.x * pos.x + pos.y * pos.y;
                if (dist2 < maxDist2 && dist2 >= minDist2) yield return pos;
            }
        }

        /// <summary>Coordinates forming a ring between inner radius and inner+margin.</summary>
        public static IEnumerable<int2> WithRing(uint innerRadius, uint margin) {
            uint outerRadius = innerRadius + margin - 1;
            uint adjustedInner = innerRadius > 0 ? innerRadius - 1 : 0;
            int limit = (int)((outerRadius * 2 + 1) * (outerRadius * 2 + 1)
                             - (adjustedInner * 2 + 1) * (adjustedInner * 2 + 1));
            int count = 0;
            foreach (var pos in SpiralFrom((int)innerRadius)) {
                if (count++ >= limit) yield break;
                yield return pos;
            }
        }
    }
}

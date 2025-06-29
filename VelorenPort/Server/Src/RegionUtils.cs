using System.Collections.Generic;
using VelorenPort.NativeMath;
using VelorenPort.CoreEngine;

namespace VelorenPort.Server
{
    /// <summary>
    /// Helper functions for region and chunk calculations.
    /// </summary>
    public static class RegionUtils
    {
        /// <summary>
        /// Convert a world position in blocks to chunk coordinates.
        /// </summary>
        public static int2 WorldToChunk(float3 pos)
        {
            int2 w = new int2((int)math.floor(pos.x), (int)math.floor(pos.y));
            return TerrainConstants.WorldToChunk(w);
        }

        /// <summary>
        /// Convert a world position in blocks to region coordinates.
        /// </summary>
        public static int2 WorldToRegion(float3 pos)
        {
            int2 w = new int2((int)math.floor(pos.x), (int)math.floor(pos.y));
            return new int2(w.x >> RegionConstants.RegionLog2, w.y >> RegionConstants.RegionLog2);
        }

        /// <summary>
        /// Convert a region key back to world coordinates of its minimum corner.
        /// </summary>
        public static float2 RegionToWorld(int2 key)
        {
            return new float2(key.x * RegionConstants.RegionSize, key.y * RegionConstants.RegionSize);
        }

        public static bool RegionInViewDistance(int2 key, float3 pos, float vd)
        {
            float vdExtended = vd + RegionConstants.TetherLength * math.sqrt(2f);
            float2 minRegionPos = RegionToWorld(key);
            float2 diff = new float2(pos.x - minRegionPos.x, pos.y - minRegionPos.y);
            if (diff.x < 0) diff.x += RegionConstants.RegionSize;
            if (diff.y < 0) diff.y += RegionConstants.RegionSize;
            return math.lengthsq(diff) < vdExtended * vdExtended;
        }

        public static HashSet<int2> RegionsInViewDistance(float3 pos, float vd)
        {
            var set = new HashSet<int2>();
            float vdExtended = vd + RegionConstants.TetherLength * math.sqrt(2f);
            int2 max = WorldToRegion(pos + new float3(vdExtended, vdExtended, 0));
            int2 min = WorldToRegion(pos - new float3(vdExtended, vdExtended, 0));
            for (int x = min.x; x <= max.x; x++)
            for (int y = min.y; y <= max.y; y++)
            {
                var key = new int2(x, y);
                if (RegionInViewDistance(key, pos, vd))
                    set.Add(key);
            }
            return set;
        }

        /// <summary>
        /// Create a region subscription for an entity at the given position.
        /// </summary>
        public static RegionSubscription InitializeRegionSubscription(Pos pos, Presence presence)
        {
            int2 fuzzyChunk = WorldToChunk(pos.Value);
            float chunkSize = TerrainConstants.ChunkSize.x;
            float radius = presence.EntityViewDistance.Current * chunkSize
                           + (PresenceConstants.ChunkFuzz + chunkSize) * math.sqrt(2f);
            var regions = RegionsInViewDistance(pos.Value, radius);
            var sub = new RegionSubscription
            {
                FuzzyChunk = fuzzyChunk,
                LastEntityViewDistance = presence.EntityViewDistance.Current
            };
            foreach (var key in regions)
                sub.Regions.Add(key);
            return sub;
        }
    }
}

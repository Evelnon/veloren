using System.Collections.Generic;
using Unity.Mathematics;
using VelorenPort.CoreEngine;

namespace VelorenPort.Server
{
    /// <summary>
    /// Utilities to update a <see cref="RegionSubscription"/> when an entity
    /// moves or changes view distance.
    /// </summary>
    public static class RegionSubscriptionUpdater
    {
        /// <summary>
        /// Update the given subscription based on the entity position and
        /// view distance. Regions that left the view are removed while new
        /// regions are added.
        /// </summary>
        public static void UpdateSubscription(Pos pos, Presence presence, RegionSubscription sub)
        {
            uint vd = presence.EntityViewDistance.Current;
            int2 currentChunk = RegionUtils.WorldToChunk(pos.Value);
            int2 diff = currentChunk - sub.FuzzyChunk;
            bool chunkChanged = math.abs(diff.x) > PresenceConstants.ChunkFuzz ||
                                math.abs(diff.y) > PresenceConstants.ChunkFuzz;
            bool vdChanged = sub.LastEntityViewDistance != vd;

            if (!chunkChanged && !vdChanged)
                return;

            sub.FuzzyChunk = currentChunk;
            sub.LastEntityViewDistance = vd;

            float chunkSize = TerrainConstants.ChunkSize.x;
            float radius = vd * chunkSize + (PresenceConstants.ChunkFuzz + chunkSize) * math.sqrt(2f);
            var newRegions = RegionUtils.RegionsInViewDistance(pos.Value, radius);

            var toRemove = new List<int2>();
            foreach (var key in sub.Regions)
                if (!newRegions.Contains(key))
                    toRemove.Add(key);
            foreach (var key in toRemove)
                sub.Regions.Remove(key);

            foreach (var key in newRegions)
                sub.Regions.Add(key);
        }
    }
}

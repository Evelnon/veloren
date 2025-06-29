using System;
using System.Collections.Generic;
using VelorenPort.NativeMath;
using VelorenPort.CoreEngine;
using VelorenPort.World;

namespace VelorenPort.Server {
    /// <summary>
    /// Level of detail map constructed from the world. Mirrors `server/src/lod.rs`.
    /// </summary>
    [Serializable]
    public class Lod {
        private static readonly Zone EMPTY_ZONE = new Zone(new List<LodObject>());

        public Dictionary<int2, Zone> Zones { get; } = new();

        public static Lod FromWorld(World.World world, TestWorld.IndexOwned index) {
            int2 zoneSz = (world.Sim.GetSize() + (int)Lod.ZoneSize - 1) / (int)Lod.ZoneSize;
            var zones = new Dictionary<int2, Zone>();
            for (int i = 0; i < zoneSz.x; i++)
            for (int j = 0; j < zoneSz.y; j++) {
                var zonePos = new int2(i, j);
                zones[zonePos] = world.GetLodZone(zonePos);
            }
            var lod = new Lod();
            foreach (var kv in zones) lod.Zones[kv.Key] = kv.Value;
            return lod;
        }

        public Zone Zone(int2 zonePos) => Zones.TryGetValue(zonePos, out var zone) ? zone : EMPTY_ZONE;
    }
}

using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Level of detail structures mirrored from common/src/lod.rs
    /// </summary>
    public static class Lod {
        /// <summary>Size of a zone in chunks.</summary>
        public const uint ZoneSize = 32;

        /// <summary>Convert zone coordinate to world coordinate.</summary>
        public static int ToWorldPos(int wpos) {
            return wpos * (TerrainConstants.ChunkSize.x * (int)ZoneSize);
        }

        /// <summary>Convert world coordinate to zone coordinate using Euclidean division.</summary>
        public static int FromWorldPos(int zonePos) {
            int denom = TerrainConstants.ChunkSize.x * (int)ZoneSize;
            return DivEuclid(zonePos, denom);
        }

        private static int DivEuclid(int a, int b) {
            int r = a % b;
            if (r < 0) r += Math.Abs(b);
            return (a - r) / b;
        }
    }

    [Flags]
    public enum InstFlags : byte {
        SnowCovered   = 0b0000_0001,
        Glow          = 0b0000_0010,
        RotateHalfPi  = 0b0000_0100,
        RotatePi      = 0b0000_1000,
    }

    public enum ObjectKind : ushort {
        GenericTree,
        Pine,
        Dead,
        House,
        GiantTree,
        Mangrove,
        Acacia,
        Birch,
        Redwood,
        Baobab,
        Frostpine,
        Haniwa,
        Desert,
        Palm,
        Arena,
        SavannahHut,
        SavannahAirshipDock,
        TerracottaPalace,
        TerracottaHouse,
        TerracottaYard,
        AirshipDock,
        CoastalHouse,
        CoastalWorkshop,
    }

    [Serializable]
    public struct LodObject {
        public ObjectKind Kind;
        public int3 Pos;
        public InstFlags Flags;
        public Rgb<byte> Color;

        public LodObject(ObjectKind kind, int3 pos, InstFlags flags, Rgb<byte> color) {
            Kind = kind;
            Pos = pos;
            Flags = flags;
            Color = color;
        }
    }

    [Serializable]
    public struct Zone {
        public List<LodObject> Objects;
        public Zone(List<LodObject> objects) { Objects = objects; }
    }
}

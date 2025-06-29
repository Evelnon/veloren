using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace VelorenPort.World {
    /// <summary>
    /// Light-weight representation of world map information.
    /// This is far less detailed than the Rust version but provides
    /// enough data for debugging and basic clients.
    /// </summary>
    [Serializable]
    public class WorldMapMsg {
        public int2 Dimensions { get; set; }
        public float MaxHeight { get; set; }
        public List<Marker> Sites { get; } = new();
        public List<PoiInfo> Pois { get; } = new();
        public List<ulong> PossibleStartingSites { get; } = new();
    }

    [Serializable]
    public struct Marker {
        public string Name;
        public int2 Position;
        public MarkerKind Kind;
    }

    [Serializable]
    public struct PoiInfo {
        public string Name;
        public int2 Position;
        public Site.PoiKind Kind;
    }
}

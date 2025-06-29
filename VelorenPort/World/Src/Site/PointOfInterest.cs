using System;
using Unity.Mathematics;

namespace VelorenPort.World.Site {
    /// <summary>
    /// Representation of an important landmark. More closely follows the
    /// Rust structure with a name and a kind describing the feature.
    /// </summary>
    [Serializable]
    public class PointOfInterest {
        public int2 Position { get; set; }
        public string Name { get; set; } = "POI";
        public PoiKind Kind { get; set; } = PoiKind.Peak(0);
    }
}

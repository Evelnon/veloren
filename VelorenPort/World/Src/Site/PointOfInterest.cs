using Unity.Mathematics;

namespace VelorenPort.World.Site {
    /// <summary>Simple placeholder representing important local landmarks.</summary>
    public class PointOfInterest {
        public int2 Position { get; set; }
        public string Description { get; set; } = "POI";
        public PoiKind Kind { get; set; } = new PoiKind.Peak(0);
    }
}

using System;
using System.Collections.Generic;
using Unity.Mathematics;
using VelorenPort.CoreEngine;
using VelorenPort.World.Sim;
using SiteEntity = VelorenPort.World.Site.Site;
using PlaceEntity = VelorenPort.World.Site.Place;
using PoiEntity = VelorenPort.World.Site.PointOfInterest;

namespace VelorenPort.World {
    /// <summary>
    /// Representation of a generated chunk with simulation attributes.
    /// Mirrors the <c>SimChunk</c> struct from the Rust code.
    /// </summary>
    [Serializable]
    public class SimChunk {
        public float Chaos { get; set; }
        public float Alt { get; set; }
        public float Basement { get; set; }
        public float WaterAlt { get; set; }
        public int2? Downhill { get; set; }
        public float Flux { get; set; }
        public float Temp { get; set; }
        public float Humidity { get; set; }
        public float Rockiness { get; set; }
        public float TreeDensity { get; set; }
        public ForestKind ForestKind { get; set; }
        public float SpawnRate { get; set; }
        public RiverData River { get; set; } = new();
        public float SurfaceVeg { get; set; }
        public List<Store<SiteEntity>.Id> Sites { get; set; } = new();
        public Store<PlaceEntity>.Id? Place { get; set; }
        public Store<PoiEntity>.Id? Poi { get; set; }
        public (Way way, Path path) Path { get; set; }
        public float CliffHeight { get; set; }
        public Spot? Spot { get; set; }
        public bool ContainsWaypoint { get; set; }
    }

}

using System;
using VelorenPort.NativeMath;
using VelorenPort.World.Site;
using VelorenPort.World.Site.Util;
using VelorenPort.CoreEngine;
using System.Collections.Generic;
using System.Linq;

namespace VelorenPort.World.Civ
{
    /// <summary>
    /// Simplified representation of the airship travel network. This mirrors
    /// the structures from <c>airship_travel.rs</c> but omits the complex
    /// generation logic.
    /// </summary>
    [Serializable]
    public struct AirshipDockingPosition
    {
        public uint Id { get; init; }
        public float3 Position { get; init; }
        public AirshipDockingPosition(uint id, float3 pos) { Id = id; Position = pos; }
    }

    [Serializable]
    public enum AirshipDockingSide
    {
        Port,
        Starboard,
    }

    [Serializable]
    public class AirshipDockingApproach
    {
        public AirshipDockingPosition DockPos { get; init; }
        public float3 AirshipPos { get; init; }
        public Dir AirshipDirection { get; init; }
        public float2 DockCenter { get; init; }
        public float Height { get; init; }
        public float2 ApproachInitialPos { get; init; }
        public float2 ApproachFinalPos { get; init; }
        public AirshipDockingSide Side { get; init; }
        public Store<Site.Site>.Id SiteId { get; init; }
    }

    [Serializable]
    public class AirshipRoute
    {
        public Store<Site.Site>.Id[] Sites { get; } = new Store<Site.Site>.Id[2];
        public AirshipDockingApproach[] Approaches { get; } = new AirshipDockingApproach[2];
        public uint Distance { get; init; }
        public float TravelTime { get; init; }
    }

    [Serializable]
    public class Airships
    {
        private const float CruiseSpeed = 200f;
        public Dictionary<uint, AirshipRoute> Routes { get; } = new();

        public IEnumerable<AirshipRoute> RoutesFrom(Store<Site.Site>.Id id)
            => Routes.Values.Where(r => r.Sites[0] == id || r.Sites[1] == id);

        public AirshipRoute? GetRoute(uint id)
            => Routes.TryGetValue(id, out var r) ? r : null;

        /// <summary>Create a trivial route between two sites.</summary>
        public void CreateRoute(uint id, Store<Site.Site>.Id a, Store<Site.Site>.Id b)
        {
            var approachA = new AirshipDockingApproach
            {
                DockPos = new AirshipDockingPosition(0, float3.zero),
                AirshipPos = float3.zero,
                AirshipDirection = Dir.Y,
                DockCenter = float2.zero,
                Height = 100f,
                ApproachInitialPos = float2.zero,
                ApproachFinalPos = float2.zero,
                Side = AirshipDockingSide.Port,
                SiteId = a
            };
            var approachB = new AirshipDockingApproach
            {
                DockPos = new AirshipDockingPosition(1, float3.zero),
                AirshipPos = float3.zero,
                AirshipDirection = Dir.Y,
                DockCenter = float2.zero,
                Height = 100f,
                ApproachInitialPos = float2.zero,
                ApproachFinalPos = float2.zero,
                Side = AirshipDockingSide.Port,
                SiteId = b
            };
            Routes[id] = new AirshipRoute
            {
                Sites = { [0] = a, [1] = b },
                Approaches = { [0] = approachA, [1] = approachB },
                Distance = 0u,
                TravelTime = 0f
            };
        }

        /// <summary>
        /// Generate simple airship routes connecting all sites with an airship dock.
        /// </summary>
        public void GenerateRoutes(WorldSim sim, Store<Site.Site> sites)
        {
            Routes.Clear();

            var docks = new List<(Store<Site.Site>.Id id, float3 pos)>();
            foreach (var (id, site) in sites.Enumerate())
            {
                if (!site.Plots.Any(p => p.Kind.ToString().Contains("AirshipDock")))
                    continue;
                float alt = sim.GetSurfaceAltApprox(site.Position);
                docks.Add((id, new float3(site.Position.x + 0.5f, site.Position.y + 0.5f, alt + 10f)));
            }

            uint routeId = 0;
            for (int i = 0; i < docks.Count; i++)
                for (int j = i + 1; j < docks.Count; j++)
                {
                    var a = docks[i];
                    var b = docks[j];
                    Routes[routeId++] = BuildRoute(a.id, b.id, a.pos, b.pos);
                }
        }

        private static AirshipDockingApproach BuildApproach(float3 from, float3 to, Store<Site.Site>.Id dest)
        {
            var dirVec = math.normalize(to - from);
            var dir = Dir.FromUnnormalized(dirVec) ?? Dir.Y;
            return new AirshipDockingApproach
            {
                DockPos = new AirshipDockingPosition(0, to),
                AirshipPos = to,
                AirshipDirection = dir,
                DockCenter = to.xy,
                Height = 100f,
                ApproachInitialPos = from.xy,
                ApproachFinalPos = to.xy,
                Side = AirshipDockingSide.Starboard,
                SiteId = dest
            };
        }

        private static AirshipRoute BuildRoute(Store<Site.Site>.Id a, Store<Site.Site>.Id b, float3 posA, float3 posB)
        {
            float dist = math.distance(posA.xy, posB.xy);
            var approachAB = BuildApproach(posA, posB, b);
            var approachBA = BuildApproach(posB, posA, a);
            return new AirshipRoute
            {
                Sites = { [0] = a, [1] = b },
                Approaches = { [0] = approachAB, [1] = approachBA },
                Distance = (uint)dist,
                TravelTime = dist / CruiseSpeed
            };
        }
    }
}

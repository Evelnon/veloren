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
        public float CycleTime { get; init; }
    }

    [Serializable]
    public class Airships
    {
        private const float CruiseSpeed = 200f;
        private const float DockingDuration = 90f;
        private const float StdCruiseHeight = 400f;
        private const float DockAlignX = 18f;
        private const float DockAlignY = 1f;
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

        private static AirshipDockingApproach BuildApproach(float3 from, float3 to, Store<Site.Site>.Id dest, AirshipDockingSide side)
        {
            var dirVec = math.normalize(to - from);
            var right = new float2(-dirVec.y, dirVec.x);
            var dir = Dir.FromUnnormalized(new float3(dirVec.x, dirVec.y, 0f));

            float3 dockPos = to;
            float3 airshipPos = dockPos + new float3(right * (side == AirshipDockingSide.Port ? DockAlignX : -DockAlignX), -3f);
            airshipPos += new float3(dirVec * DockAlignY, 0f);

            float2 finalPos = dockPos.xy - dirVec.xy * (side == AirshipDockingSide.Starboard ? 500f : -500f);
            float2 initialPos = finalPos + right * 500f;

            return new AirshipDockingApproach
            {
                DockPos = new AirshipDockingPosition(0, dockPos),
                AirshipPos = airshipPos,
                AirshipDirection = dir,
                DockCenter = dockPos.xy,
                Height = StdCruiseHeight,
                ApproachInitialPos = initialPos,
                ApproachFinalPos = finalPos,
                Side = side,
                SiteId = dest
            };
        }

        private static AirshipRoute BuildRoute(Store<Site.Site>.Id a, Store<Site.Site>.Id b, float3 posA, float3 posB)
        {
            float dist = math.distance(posA.xy, posB.xy);
            var approachAB = BuildApproach(posA, posB, b, AirshipDockingSide.Starboard);
            var approachBA = BuildApproach(posB, posA, a, AirshipDockingSide.Port);
            float travel = dist / CruiseSpeed;
            return new AirshipRoute
            {
                Sites = { [0] = a, [1] = b },
                Approaches = { [0] = approachAB, [1] = approachBA },
                Distance = (uint)dist,
                TravelTime = travel,
                CycleTime = travel * 2f + DockingDuration * 2f
            };
        }
    }
}

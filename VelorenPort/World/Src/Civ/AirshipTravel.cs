using System;
using VelorenPort.NativeMath;
using VelorenPort.World.Site;
using VelorenPort.World.Site.Util;
using VelorenPort.CoreEngine;
using System.Collections.Generic;

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
    }

    [Serializable]
    public class Airships
    {
        public Dictionary<uint, AirshipRoute> Routes { get; } = new();

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
                Distance = 0u
            };
        }
    }
}

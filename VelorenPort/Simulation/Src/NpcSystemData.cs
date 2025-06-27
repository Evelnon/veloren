using System;
using System.Collections.Generic;
using Unity.Entities;
using VelorenPort.CoreEngine;

namespace VelorenPort.Simulation {
    /// <summary>
    /// Data accessed by NPC related systems when processing simulation events.
    /// Mirrors the `NpcSystemData` struct from rtsim.
    /// </summary>
    [Serializable]
    public class NpcSystemData {
        public IReadOnlyDictionary<Entity, Pos> Positions { get; }
        public IdMaps IdMaps { get; }
        public ServerConstants ServerConstants { get; }

        public NpcSystemData(
            IReadOnlyDictionary<Entity, Pos> positions,
            IdMaps idMaps,
            ServerConstants serverConstants)
        {
            Positions = positions;
            IdMaps = idMaps;
            ServerConstants = serverConstants;
        }
    }
}

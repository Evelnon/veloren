using System;
using System.Collections.Generic;
using VelorenPort.CoreEngine;

namespace VelorenPort.Simulation {
    /// <summary>
    /// Simplified store for NPCs. Provides creation and lookup by identifier.
    /// </summary>
    [Serializable]
    public class Npcs {
        private int _counter;
        private readonly Dictionary<NpcId, Npc> _npcs = new();

        public NpcId CreateNpc(Npc npc) {
            var id = new NpcId(_counter++);
            _npcs[id] = npc;
            return id;
        }

        public bool Remove(NpcId id) => _npcs.Remove(id);

        public Npc? Get(NpcId id) => _npcs.TryGetValue(id, out var npc) ? npc : null;

        public IEnumerable<KeyValuePair<NpcId, Npc>> All => _npcs;
    }
}

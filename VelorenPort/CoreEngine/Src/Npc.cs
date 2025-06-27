using System;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Minimal representation of a non-player character.
    /// </summary>
    [Serializable]
    public class Npc {
        public Uid Id { get; }
        public string Name { get; set; } = "NPC";
        public float Health { get; private set; } = 100f;
        public VelorenPort.Simulation.SiteId? Home { get; set; }

        public Npc(Uid id) {
            Id = id;
        }

        public void TakeDamage(float dmg) {
            Health = Math.Max(0f, Health - dmg);
        }

        public string Say(string text) => $"{Name}: {text}";

        public bool Alive => Health > 0f;

        public void Tick(float dt) {
            // Placeholder behaviour: slowly regenerate
            if (Health < 100f)
                Health = Math.Min(100f, Health + dt * 2f);
        }
    }
}

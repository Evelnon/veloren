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
        public SiteId? Home { get; set; }
        public NpcState State { get; private set; } = NpcState.Idle;

        public Npc(Uid id) {
            Id = id;
        }

        public void TakeDamage(float dmg) {
            Health = Math.Max(0f, Health - dmg);
        }

        public string Say(string text) => $"{Name}: {text}";

        public bool Alive => Health > 0f;

        public void Tick(float dt) {
            if (Health < 100f)
                Health = Math.Min(100f, Health + dt * 2f);

            switch (State) {
                case NpcState.Idle:
                    IdleTime += dt;
                    if (IdleTime > 5f) {
                        State = NpcState.Patrol;
                        IdleTime = 0f;
                    }
                    break;
                case NpcState.Patrol:
                    // Wander for a short time then return to idle
                    IdleTime += dt;
                    if (IdleTime > 3f)
                    {
                        State = NpcState.Idle;
                        IdleTime = 0f;
                    }
                    break;
                case NpcState.Chase:
                case NpcState.Flee:
                case NpcState.Combat:
                    // In this stub we simply stay in combat
                    break;
            }
        }

        public void EnterCombat() => State = NpcState.Combat;

        public void ExitCombat() {
            State = NpcState.Idle;
            IdleTime = 0f;
        }

        private float IdleTime { get; set; }
    }

    public enum NpcState {
        Idle,
        Patrol,
        Chase,
        Flee,
        Combat
    }
}

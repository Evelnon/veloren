using System;
using Unity.Mathematics;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Basic combat structures providing damage amounts and types.
    /// This is a tiny subset of the much larger Rust combat module and
    /// only intended for testing other systems.
    /// </summary>
    [Serializable]
    public enum DamageKind {
        Physical,
        Fire,
        Ice,
        Poison
    }

    /// <summary>Information about a hit dealt to an entity.</summary>
    [Serializable]
    public struct HitInfo {
        public Uid Target;
        public float Amount;
        public DamageKind Kind;

        public HitInfo(Uid target, float amount, DamageKind kind) {
            Target = target;
            Amount = amount;
            Kind = kind;
        }
    }

    /// <summary>
    /// Represents an attack performed by one entity against another.
    /// </summary>
    [Serializable]
    public struct Attack {
        public Uid Attacker;
        public HitInfo Hit;

        public Attack(Uid attacker, HitInfo hit) {
            Attacker = attacker;
            Hit = hit;
        }
    }

    /// <summary>Damage resistances per kind, range 0..1.</summary>
    [Serializable]
    public struct Resistances {
        private System.Collections.Generic.Dictionary<DamageKind, float> _values;

        public Resistances(float defaultResist = 0f) {
            _values = new();
            foreach (DamageKind kind in Enum.GetValues(typeof(DamageKind)))
                _values[kind] = defaultResist;
        }

        public float this[DamageKind k] {
            get => _values[k];
            set => _values[k] = value;
        }
    }

    public interface IDamageable {
        Uid Id { get; }
        float Health { get; set; }
    }

    /// <summary>An event emitted whenever damage is applied.</summary>
    [Serializable]
    public struct DamageEvent {
        public Uid Target;
        public Uid Attacker;
        public float Amount;
        public DamageKind Kind;

        public DamageEvent(Uid target, Uid attacker, float amount, DamageKind kind) {
            Target = target;
            Attacker = attacker;
            Amount = amount;
            Kind = kind;
        }
    }

    public static class CombatUtils {
        /// <summary>
        /// Apply <paramref name="attack"/> to the provided health dictionary.
        /// If the target is not present it will be added with the negative
        /// damage amount. This is a trivial placeholder used by tests.
        /// </summary>
        public static void ApplyAttack(System.Collections.Generic.Dictionary<Uid, float> health,
                                       Attack attack,
                                       Resistances? resist = null) {
            if (!health.TryGetValue(attack.Hit.Target, out var hp))
                hp = 0f;
            float dmg = attack.Hit.Amount;
            if (resist.HasValue)
                dmg *= 1f - math.clamp(resist.Value[attack.Hit.Kind], 0f, 1f);
            hp -= dmg;
            health[attack.Hit.Target] = hp;
        }

        /// <summary>
        /// Apply damage directly to a <see cref="IDamageable"/> instance and
        /// report the outcome via <paramref name="log"/> if provided.
        /// </summary>
        public static void Apply(IDamageable target, Attack attack,
                                 Resistances? resist,
                                 System.Collections.Generic.ICollection<DamageEvent>? log = null) {
            float dmg = attack.Hit.Amount;
            if (resist.HasValue)
                dmg *= 1f - math.clamp(resist.Value[attack.Hit.Kind], 0f, 1f);
            target.Health = math.max(0f, target.Health - dmg);
            log?.Add(new DamageEvent(target.Id, attack.Attacker, dmg, attack.Hit.Kind));
        }
    }
}

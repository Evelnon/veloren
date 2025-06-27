using System;
using System.Collections.Generic;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Describes permanent effects that can be applied to entities.
    /// Mirrors <c>PermanentEffect</c> from the Rust implementation.
    /// </summary>
    [Serializable]
    public enum PermanentEffect {
        CycleBodyType,
    }

    /// <summary>
    /// A buff specification applied by an effect. Equivalent to the Rust
    /// <c>BuffEffect</c> structure.
    /// </summary>
    [Serializable]
    public class BuffEffect {
        public comp.BuffKind Kind { get; set; }
        public comp.BuffData Data { get; set; } = new comp.BuffData();
        public List<comp.BuffCategory> CatIds { get; set; } = new();
    }

    /// <summary>
    /// Represents a single effect outcome. Ported from <c>effect.rs</c>.
    /// </summary>
    [Serializable]
    public abstract record Effect {
        [Serializable]
        public sealed record Health(comp.HealthChange Change) : Effect;
        [Serializable]
        public sealed record Poise(float Value) : Effect;
        [Serializable]
        public sealed record Damage(combat.Damage Damage) : Effect;
        [Serializable]
        public sealed record Buff(BuffEffect Effect) : Effect;
        [Serializable]
        public sealed record Permanent(PermanentEffect Effect) : Effect;

        /// <summary>Human readable description for debugging.</summary>
        public string Info() => this switch {
            Health h => $"{h.Change.Amount:+0.##;-0.##;+0} health",
            Poise p => $"{p.Value:+0.##;-0.##;+0} poise",
            Damage d => $"{d.Damage.Value:+0.##;-0.##;+0}",
            Buff b => $"{b.Effect} buff",
            Permanent p => p.Effect.ToString(),
            _ => string.Empty,
        };

        /// <summary>Whether this effect negatively impacts the target.</summary>
        public bool IsHarm() => this switch {
            Health h => h.Change.Amount < 0f,
            Poise p => p.Value < 0f,
            Damage => true,
            Buff b => !b.Effect.Kind.IsBuff(),
            _ => false,
        };

        /// <summary>
        /// Modify the strength of this effect by the given factor.
        /// Behaviour follows the Rust implementation of <c>modify_strength</c>.
        /// </summary>
        public void ModifyStrength(float modifier) {
            switch (this) {
                case Health h:
                    h.Change.Amount *= modifier;
                    break;
                case Poise p:
                    p.Value *= modifier;
                    break;
                case Damage d:
                    d.Damage.InterpolateDamage(modifier, 0f);
                    break;
                case Buff b:
                    b.Effect.Data.Strength *= modifier;
                    break;
                case Permanent:
                    break;
            }
        }
    }
}

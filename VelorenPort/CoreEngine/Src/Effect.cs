using System;
using System.Collections.Generic;
using VelorenPort.CoreEngine.comp;

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
        public sealed record DamageEffect(combat.Damage Damage) : Effect;
        [Serializable]
        public sealed record Buff(BuffEffect Effect) : Effect;
        [Serializable]
        public sealed record Permanent(PermanentEffect Effect) : Effect;

        /// <summary>Human readable description for debugging.</summary>
        public string Info() => this switch {
            Health h => $"{h.Change.Amount:+0.##;-0.##;+0} health",
            Poise p => $"{p.Value:+0.##;-0.##;+0} poise",
            DamageEffect d => $"{d.Damage.Value:+0.##;-0.##;+0}",
            Buff b => $"{b.Effect} buff",
            Permanent p => p.Effect.ToString(),
            _ => string.Empty,
        };

        /// <summary>Whether this effect negatively impacts the target.</summary>
        public bool IsHarm() => this switch {
            Health h => h.Change.Amount < 0f,
            Poise p => p.Value < 0f,
            DamageEffect => true,
            Buff b => true,
            _ => false,
        };

        /// <summary>
        /// Modify the strength of this effect by the given factor.
        /// Behaviour follows the Rust implementation of <c>modify_strength</c>.
        /// </summary>
        public void ModifyStrength(float modifier)
        {
            switch (this)
            {
                case Health h:
                    var hcField = typeof(Health).GetField("<Change>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                    var change = h.Change;
                    change.Amount *= modifier;
                    hcField?.SetValue(h, change);
                    break;
                case Poise p:
                    var valueField = typeof(Poise).GetField("<Value>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                    var value = p.Value * modifier;
                    valueField?.SetValue(p, value);
                    break;
                case DamageEffect d:
                    d.Damage.InterpolateDamage(modifier, 0f);
                    break;
                case Buff b:
                    var data = b.Effect.Data;
                    data.Strength *= modifier;
                    b.Effect.Data = data;
                    break;
                case Permanent:
                    break;
            }
        }
    }
}

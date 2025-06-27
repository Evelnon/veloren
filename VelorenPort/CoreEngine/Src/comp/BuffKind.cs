using System;

namespace VelorenPort.CoreEngine.comp {
    /// <summary>
    /// Enumeration of all buff and debuff kinds. Mirrors `BuffKind` from
    /// `common/src/comp/buff.rs`.
    /// </summary>
    [Serializable]
    public enum BuffKind {
        // BUFFS
        Regeneration,
        Saturation,
        Potion,
        Agility,
        RestingHeal,
        EnergyRegen,
        ComboGeneration,
        IncreaseMaxEnergy,
        IncreaseMaxHealth,
        Invulnerability,
        ProtectingWard,
        Frenzied,
        Hastened,
        Fortitude,
        Reckless,
        Flame,
        Frigid,
        Lifesteal,
        Bloodfeast,
        ImminentCritical,
        Fury,
        Sunderer,
        Defiance,
        Berserk,
        ScornfulTaunt,
        Tenacity,
        Resilience,
        // DEBUFFS
        Burning,
        Bleeding,
        Cursed,
        Crippled,
        Frozen,
        Wet,
        Ensnared,
        Poisoned,
        Parried,
        PotionSickness,
        Heatstroke,
        Rooted,
        Winded,
        Concussion,
        Staggered,
        // COMPLEX
        Polymorphed,
    }
}

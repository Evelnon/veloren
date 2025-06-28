using System;

namespace VelorenPort.CoreEngine.comp
{
    /// <summary>Placeholder data for buff effects.</summary>
    [Serializable]
    public struct BuffData
    {
        public float Strength;
    }

    /// <summary>Categories used to group buffs.</summary>
    [Serializable]
    public enum BuffCategory
    {
        Natural,
        Physical,
        Magical,
        Divine,
    }

    /// <summary>Represents a change in health.</summary>
    [Serializable]
    public struct HealthChange
    {
        public float Amount;
    }

    /// <summary>Basic body enumeration.</summary>
    [Serializable]
    public enum Body
    {
        Humanoid,
        Creature,
    }

}

namespace VelorenPort.CoreEngine.comp.poise
{
    /// <summary>State of an entity's poise.</summary>
    [Serializable]
    public enum PoiseState
    {
        Normal,
        Interrupted,
    }
}

namespace VelorenPort.CoreEngine.comp
{
    /// <summary>Types of utterances entities can perform.</summary>
    [Serializable]
    public enum UtteranceKind
    {
        Generic,
    }

    /// <summary>Origin of damage.</summary>
    [Serializable]
    public enum DamageSource
    {
        Generic,
    }

    namespace beam
    {
        [Serializable]
        public enum FrontendSpecifier { Basic }
    }

    namespace skillset
    {
        [Serializable]
        public enum SkillGroupKind { Combat }
    }

    namespace terrain
    {
        [Serializable]
        public enum SpriteKind { Generic }
    }

    namespace fluid_dynamics
    {
        [Serializable]
        public enum LiquidKind { Water }
    }
}

namespace VelorenPort.CoreEngine.combat
{
    /// <summary>Basic damage container.</summary>
    [Serializable]
    public struct Damage
    {
        public float Value;
        public void InterpolateDamage(float factor, float baseDamage) => Value = Value * factor + baseDamage;
    }

    /// <summary>Information about the contributor of damage.</summary>
    [Serializable]
    public struct DamageContributor
    {
        public VelorenPort.CoreEngine.Uid Attacker;
    }
}

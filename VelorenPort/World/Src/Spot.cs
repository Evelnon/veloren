using System;
using System.Collections.Generic;

namespace VelorenPort.World
{
    /// <summary>
    /// Enumeration of small landmarks or structures that can appear
    /// across the world. Mirrors a subset of the Rust <c>Spot</c> enum.
    /// </summary>
    [Serializable]
    public enum Spot
    {
        DwarvenGrave,
        SaurokAltar,
        MyrmidonTemple,
        GnarlingTotem,
        WitchHouse,
        GnomeSpring,
        WolfBurrow,
        Igloo,
        LionRock,
        TreeStumpForest,
        DesertBones,
        Arch,
        AirshipCrash,
        FruitTree,
        Shipwreck,
        Shipwreck2,
        FallenTree,
        GraveSmall,
        JungleTemple,
        SaurokTotem,
        JungleOutpost
    }

    /// <summary>
    /// Conditions describing where a spot can spawn. Only partially
    /// ported from the Rust implementation.
    /// </summary>
    [Serializable]
    public abstract record SpotCondition
    {
        [Serializable] public record MaxGradient(float Value) : SpotCondition;
        [Serializable] public record Biome(List<BiomeKind> Biomes) : SpotCondition;
        [Serializable] public record NearCliffs() : SpotCondition;
        [Serializable] public record NearRiver() : SpotCondition;
        [Serializable] public record IsWay() : SpotCondition;
        [Serializable] public record IsUnderwater() : SpotCondition;
        [Serializable] public record Typical() : SpotCondition;
        [Serializable] public record MinWaterDepth(float Value) : SpotCondition;
        [Serializable] public record Not(SpotCondition Condition) : SpotCondition;
        [Serializable] public record All(List<SpotCondition> Conditions) : SpotCondition;
        [Serializable] public record Any(List<SpotCondition> Conditions) : SpotCondition;
    }

    /// <summary>
    /// Metadata loaded from manifest files describing how often a spot
    /// should appear and under which conditions. The asset loading system
    /// is not yet ported, so this structure is currently unused.
    /// </summary>
    [Serializable]
    public class SpotProperties
    {
        public string BaseStructures { get; set; } = string.Empty;
        public float Freq { get; set; }
            = 1f;
        public SpotCondition Condition { get; set; } = new SpotCondition.Typical();
        public bool Spawn { get; set; } = true;
    }
}

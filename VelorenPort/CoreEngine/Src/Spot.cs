using System;
using System.Collections.Generic;

namespace VelorenPort.CoreEngine
{
    /// <summary>
    /// Enumeration of special structures that can spawn in the world.
    /// Mirrors <c>spot.rs</c> from the Rust codebase.
    /// </summary>
    [Serializable]
    public abstract record Spot
    {
        public sealed record DwarvenGrave : Spot;
        public sealed record SaurokAltar : Spot;
        public sealed record MyrmidonTemple : Spot;
        public sealed record GnarlingTotem : Spot;
        public sealed record WitchHouse : Spot;
        public sealed record GnomeSpring : Spot;
        public sealed record WolfBurrow : Spot;
        public sealed record Igloo : Spot;
        // Random world objects
        public sealed record LionRock : Spot;
        public sealed record TreeStumpForest : Spot;
        public sealed record DesertBones : Spot;
        public sealed record Arch : Spot;
        public sealed record AirshipCrash : Spot;
        public sealed record FruitTree : Spot;
        public sealed record Shipwreck : Spot;
        public sealed record Shipwreck2 : Spot;
        public sealed record FallenTree : Spot;
        public sealed record GraveSmall : Spot;
        public sealed record JungleTemple : Spot;
        public sealed record SaurokTotem : Spot;
        public sealed record JungleOutpost : Spot;
        /// <summary>
        /// Marker used when loading spot definitions from RON files.
        /// </summary>
        public sealed record RonFile(SpotProperties Props) : Spot;
    }

    /// <summary>
    /// Conditions used to decide when a spot can spawn.
    /// </summary>
    [Serializable]
    public abstract record SpotCondition
    {
        public sealed record MaxGradient(float Value) : SpotCondition;
        public sealed record Biome(List<World.BiomeKind> Biomes) : SpotCondition;
        public sealed record NearCliffs : SpotCondition;
        public sealed record NearRiver : SpotCondition;
        public sealed record IsWay : SpotCondition;
        public sealed record IsUnderwater : SpotCondition;
        /// <summary>No cliffs, no river, no way</summary>
        public sealed record Typical : SpotCondition;
        /// <summary>Implies IsUnderwater</summary>
        public sealed record MinWaterDepth(float Depth) : SpotCondition;
        public sealed record Not(SpotCondition Condition) : SpotCondition;
        public sealed record All(List<SpotCondition> Conditions) : SpotCondition;
        public sealed record Any(List<SpotCondition> Conditions) : SpotCondition;
    }

    /// <summary>
    /// Metadata describing how a spot should be generated.
    /// </summary>
    [Serializable]
    public class SpotProperties
    {
        public string BaseStructures { get; set; }
        public float Freq { get; set; }
        public SpotCondition Condition { get; set; }
        public bool Spawn { get; set; }

        public SpotProperties(string baseStructures, float freq, SpotCondition condition, bool spawn)
        {
            BaseStructures = baseStructures;
            Freq = freq;
            Condition = condition;
            Spawn = spawn;
        }
    }

    /// <summary>
    /// Collection of spot properties typically loaded from a RON manifest.
    /// </summary>
    [Serializable]
    public class RonSpots
    {
        public List<SpotProperties> Spots { get; }

        public RonSpots(List<SpotProperties> spots)
        {
            Spots = spots;
        }
    }
}

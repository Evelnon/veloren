using System;
using VelorenPort.NativeMath;
using VelorenPort.CoreEngine;

namespace VelorenPort.World {
    /// <summary>
    /// Types shared across world generation such as forests and trees.
    /// Mirrors <c>all.rs</c>.
    /// </summary>
    public enum ForestKind {
        Palm,
        Acacia,
        Baobab,
        Oak,
        Chestnut,
        Cedar,
        Pine,
        Redwood,
        Birch,
        Mangrove,
        Giant,
        Swamp,
        Frostpine,
        Dead,
        Mapletree,
        Cherry,
        AutumnTree
    }

    public struct Environment {
        public float Humid;
        public float Temp;
        public float NearWater;
    }

    /// <summary>
    /// Metadata about a generated tree.
    /// </summary>
    public struct TreeAttr {
        public int2 Pos;
        public uint Seed;
        public float Scale;
        public ForestKind ForestKind;
        public bool Inhabited;
    }


    public static class ForestKindExt {
        public static (float start, float end) HumidRange(this ForestKind kind) => kind switch {
            ForestKind.Palm => (0.25f, 1.4f),
            ForestKind.Acacia => (0.05f, 0.55f),
            ForestKind.Baobab => (0.2f, 0.6f),
            ForestKind.Oak => (0.35f, 1.5f),
            ForestKind.Chestnut => (0.35f, 1.5f),
            ForestKind.Cedar => (0.275f, 1.45f),
            ForestKind.Pine => (0.2f, 1.4f),
            ForestKind.Redwood => (0.6f, 1.0f),
            ForestKind.Frostpine => (0.2f, 1.4f),
            ForestKind.Birch => (0.0f, 0.6f),
            ForestKind.Mangrove => (0.5f, 1.3f),
            ForestKind.Swamp => (0.5f, 1.1f),
            ForestKind.Dead => (0.0f, 1.5f),
            ForestKind.Mapletree => (0.65f, 1.25f),
            ForestKind.Cherry => (0.45f, 0.75f),
            ForestKind.AutumnTree => (0.25f, 0.65f),
            _ => (0f, 0f)
        };

        public static (float start, float end) TempRange(this ForestKind kind) => kind switch {
            ForestKind.Palm => (0.4f, 1.6f),
            ForestKind.Acacia => (0.3f, 1.6f),
            ForestKind.Baobab => (0.4f, 0.9f),
            ForestKind.Oak => (-0.35f, 0.45f),
            ForestKind.Chestnut => (-0.35f, 0.45f),
            ForestKind.Cedar => (-0.65f, 0.15f),
            ForestKind.Pine => (-0.85f, -0.2f),
            ForestKind.Redwood => (-0.5f, -0.3f),
            ForestKind.Frostpine => (-1.8f, -0.8f),
            ForestKind.Birch => (-0.7f, 0.25f),
            ForestKind.Mangrove => (0.35f, 1.6f),
            ForestKind.Swamp => (-0.6f, 0.8f),
            ForestKind.Dead => (-1.5f, 1.0f),
            ForestKind.Mapletree => (-0.15f, 0.25f),
            ForestKind.Cherry => (-0.10f, 0.15f),
            ForestKind.AutumnTree => (-0.45f, 0.05f),
            _ => (0f, 0f)
        };

        public static (float start, float end)? NearWaterRange(this ForestKind kind) => kind switch {
            ForestKind.Palm => (0.35f, 1.8f),
            ForestKind.Swamp => (0.5f, 1.8f),
            _ => null
        };

        public static float IdealProclivity(this ForestKind kind) => kind switch {
            ForestKind.Palm => 0.4f,
            ForestKind.Acacia => 0.6f,
            ForestKind.Baobab => 0.2f,
            ForestKind.Oak => 1.0f,
            ForestKind.Chestnut => 0.3f,
            ForestKind.Cedar => 0.3f,
            ForestKind.Pine => 1.0f,
            ForestKind.Redwood => 2.5f,
            ForestKind.Frostpine => 1.0f,
            ForestKind.Birch => 0.65f,
            ForestKind.Mangrove => 2.0f,
            ForestKind.Swamp => 1.0f,
            ForestKind.Dead => 0.01f,
            ForestKind.Mapletree => 0.65f,
            ForestKind.Cherry => 12.0f,
            ForestKind.AutumnTree => 125.0f,
            _ => 0.0f
        };

        public static float ShrubDensityFactor(this ForestKind kind) => kind switch {
            ForestKind.Palm => 0.2f,
            ForestKind.Acacia => 0.3f,
            ForestKind.Baobab => 0.2f,
            ForestKind.Oak => 0.4f,
            ForestKind.Chestnut => 0.3f,
            ForestKind.Cedar => 0.3f,
            ForestKind.Pine => 0.5f,
            ForestKind.Frostpine => 0.3f,
            ForestKind.Birch => 0.65f,
            ForestKind.Mangrove => 1.0f,
            ForestKind.Swamp => 0.4f,
            ForestKind.Mapletree => 0.4f,
            ForestKind.Cherry => 0.3f,
            ForestKind.AutumnTree => 0.4f,
            _ => 1.0f
        };

        public static StructureBlock LeafBlock(this ForestKind kind) => kind switch {
            ForestKind.Palm => StructureBlock.PalmLeavesOuter,
            ForestKind.Acacia => StructureBlock.Acacia,
            ForestKind.Baobab => StructureBlock.Baobab,
            ForestKind.Oak => StructureBlock.TemperateLeaves,
            ForestKind.Chestnut => StructureBlock.Chestnut,
            ForestKind.Cedar => StructureBlock.PineLeaves,
            ForestKind.Pine => StructureBlock.PineLeaves,
            ForestKind.Redwood => StructureBlock.PineLeaves,
            ForestKind.Birch => StructureBlock.TemperateLeaves,
            ForestKind.Mangrove => StructureBlock.Mangrove,
            ForestKind.Giant => StructureBlock.TemperateLeaves,
            ForestKind.Swamp => StructureBlock.TemperateLeaves,
            ForestKind.Frostpine => StructureBlock.FrostpineLeaves,
            ForestKind.Dead => StructureBlock.TemperateLeaves,
            ForestKind.Mapletree => StructureBlock.MapleLeaves,
            ForestKind.Cherry => StructureBlock.CherryLeaves,
            ForestKind.AutumnTree => StructureBlock.AutumnLeaves,
            _ => StructureBlock.TemperateLeaves
        };

        public static float Proclivity(this ForestKind kind, Environment env)
        {
            float result = kind.IdealProclivity();
            result *= MathUtil.Close(env.Humid, kind.HumidRange());
            result *= MathUtil.Close(env.Temp, kind.TempRange());
            var nearRange = kind.NearWaterRange();
            if (nearRange.HasValue)
            {
                result *= MathUtil.Close(env.NearWater, nearRange.Value);
            }
            return result;
        }
    }
}

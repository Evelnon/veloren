using System;
using System.Collections.Generic;
using VelorenPort.CoreEngine;

namespace VelorenPort.World.Site.Economy {
    /// <summary>
    /// Opaque index into the list of goods used by the economy. Mirrors
    /// <c>GoodIndex</c> from <c>map_types.rs</c>.
    /// </summary>
    [Serializable]
    public struct GoodIndex : IEquatable<GoodIndex> {
        public const int LENGTH = 24;
        private readonly int _idx;

        public GoodIndex(int index) { _idx = index; }

        public int ToInt() => _idx;
        public static GoodIndex FromInt(int i) => new(i);

        public override int GetHashCode() => _idx.GetHashCode();
        public override bool Equals(object? obj) => obj is GoodIndex gi && gi._idx == _idx;
        public bool Equals(GoodIndex other) => other._idx == _idx;

        public static bool operator ==(GoodIndex a, GoodIndex b) => a._idx == b._idx;
        public static bool operator !=(GoodIndex a, GoodIndex b) => a._idx != b._idx;

        /// <summary>List of all goods corresponding to indices.</summary>
        public static readonly Good[] Values = new Good[LENGTH] {
            new Good.Territory(BiomeKind.Grassland),
            new Good.Territory(BiomeKind.Forest),
            new Good.Territory(BiomeKind.Lake),
            new Good.Territory(BiomeKind.Ocean),
            new Good.Territory(BiomeKind.Mountain),
            new Good.RoadSecurity(),
            new Good.Ingredients(),
            new Good.Flour(),
            new Good.Meat(),
            new Good.Wood(),
            new Good.Stone(),
            new Good.Food(),
            new Good.Tools(),
            new Good.Armor(),
            new Good.Potions(),
            new Good.Transportation(),
            new Good.Recipe(),
            new Good.Coin(),
            new Good.Terrain(BiomeKind.Lake),
            new Good.Terrain(BiomeKind.Mountain),
            new Good.Terrain(BiomeKind.Grassland),
            new Good.Terrain(BiomeKind.Forest),
            new Good.Terrain(BiomeKind.Desert),
            new Good.Terrain(BiomeKind.Ocean)
        };

        public static IEnumerable<GoodIndex> All() {
            for (int i = 0; i < LENGTH; i++) yield return new GoodIndex(i);
        }

        public static bool TryFromGood(Good good, out GoodIndex index) {
            for (int i = 0; i < LENGTH; i++) {
                if (EqualsGood(Values[i], good)) { index = new GoodIndex(i); return true; }
            }
            index = default; return false;
        }

        private static bool EqualsGood(Good a, Good b) {
            if (a.GetType() != b.GetType()) return false;
            return a switch {
                Good.Territory ta when b is Good.Territory tb => ta.Biome == tb.Biome,
                Good.Terrain ta when b is Good.Terrain tb => ta.Biome == tb.Biome,
                _ => true
            };
        }

        public Good ToGood() => Values[_idx];
    }
}

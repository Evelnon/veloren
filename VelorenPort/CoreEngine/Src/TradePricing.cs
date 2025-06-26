using System.Collections.Generic;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Minimal port of Rust's trade_pricing utilities. The original code reads
    /// large data tables from RON files to map items to their material value.
    /// This C# version stores those mappings in memory using a dictionary.
    /// </summary>
    internal static class TradePricing {
        private static readonly Dictionary<ItemDefinitionIdOwned, List<(float Amount, Good Material)>> Materials = new();

        /// <summary>
        /// Register material values for a specific item definition.
        /// </summary>
        public static void Register(ItemDefinitionIdOwned item, IEnumerable<(float Amount, Good Material)> materials) {
            Materials[item] = new List<(float, Good)>(materials);
        }

        /// <summary>
        /// Lookup material composition of an item. Returns <c>null</c> when the
        /// item is unknown.
        /// </summary>
        public static List<(float Amount, Good Material)>? GetMaterials(ItemDefinitionIdOwned item) {
            return Materials.TryGetValue(item, out var mats) ? mats : null;
        }
    }
}

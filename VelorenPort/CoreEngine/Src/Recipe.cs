using System;
using System.Collections.Generic;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Simplified crafting recipe used for basic item transformations.
    /// </summary>
    [Serializable]
    public class Recipe {
        public record Ingredient(string Item, int Count);

        public List<Ingredient> Inputs { get; } = new();
        public Dictionary<string, int> Outputs { get; } = new();
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public enum Category { Crafting, Cooking, Alchemy }
        public Category Kind { get; set; } = Category.Crafting;

        public bool CanCraft(Dictionary<string, int> inventory) {
            foreach (var ing in Inputs) {
                if (!inventory.TryGetValue(ing.Item, out int have) || have < ing.Count)
                    return false;
            }
            return true;
        }

        public void AddIngredient(string item, int count) {
            Inputs.Add(new Ingredient(item, count));
        }

        public void AddOutput(string item, int count) {
            if (Outputs.ContainsKey(item)) Outputs[item] += count;
            else Outputs[item] = count;
        }

        public void Craft(Dictionary<string, int> inventory) {
            if (!CanCraft(inventory)) return;
            foreach (var ing in Inputs) inventory[ing.Item] -= ing.Count;
            foreach (var (outItem, outCount) in Outputs) {
                if (inventory.ContainsKey(outItem)) inventory[outItem] += outCount;
                else inventory[outItem] = outCount;
            }
        }

        /// <summary>Return ingredients still missing to craft the recipe once.</summary>
        public List<Ingredient> MissingIngredients(Dictionary<string, int> inventory) {
            var missing = new List<Ingredient>();
            foreach (var ing in Inputs) {
                inventory.TryGetValue(ing.Item, out int have);
                if (have < ing.Count)
                    missing.Add(new Ingredient(ing.Item, ing.Count - have));
            }
            return missing;
        }

        /// <summary>Craft the recipe multiple times if possible.</summary>
        public void CraftMany(Dictionary<string, int> inventory, int times) {
            for (int i = 0; i < times; i++) {
                if (!CanCraft(inventory)) break;
                Craft(inventory);
            }
        }
    }
}

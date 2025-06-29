using System;
using System.Collections.Generic;

namespace VelorenPort.CoreEngine {
    /// <summary>Variants describing recipe inputs.</summary>
    [Serializable]
    public abstract record RecipeInput {
        [Serializable] public sealed record Item(string Name) : RecipeInput;
        [Serializable] public sealed record Tag(string Value) : RecipeInput;
        [Serializable] public sealed record TagSameItem(string Value) : RecipeInput;
        [Serializable] public sealed record ListSameItem(List<string> Items) : RecipeInput;
    }

    /// <summary>Error values returned when salvaging items.</summary>
    [Serializable]
    public enum SalvageError {
        NotSalvageable
    }

    /// <summary>Reasons modular weapon crafting may fail.</summary>
    [Serializable]
    public enum ModularWeaponError {
        InvalidSlot,
        ComponentMismatch,
        DifferentTools,
        DifferentHands
    }

    /// <summary>Raw representation of a recipe input before asset loading.</summary>
    [Serializable]
    public abstract record RawRecipeInput {
        [Serializable] public sealed record Item(string Name) : RawRecipeInput;
        [Serializable] public sealed record Tag(string Value) : RawRecipeInput;
        [Serializable] public sealed record TagSameItem(string Value) : RawRecipeInput;
        [Serializable] public sealed record ListSameItem(string List) : RawRecipeInput;
    }
}

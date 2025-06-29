using System;
using System.Collections.Generic;

namespace VelorenPort.CoreEngine {
    [Serializable]
    [System.Text.Json.Serialization.JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    [System.Text.Json.Serialization.JsonDerivedType(typeof(ItemDefinitionIdOwned.Simple), "simple")]
    [System.Text.Json.Serialization.JsonDerivedType(typeof(ItemDefinitionIdOwned.Modular), "modular")]
    [System.Text.Json.Serialization.JsonDerivedType(typeof(ItemDefinitionIdOwned.Compound), "compound")]
    public abstract record ItemDefinitionIdOwned {
        public sealed record Simple(string Id) : ItemDefinitionIdOwned;
        public sealed record Modular(string PseudoBase, List<ItemDefinitionIdOwned> Components) : ItemDefinitionIdOwned;
        public sealed record Compound(string SimpleBase, List<ItemDefinitionIdOwned> Components) : ItemDefinitionIdOwned;
    }
}

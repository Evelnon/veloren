using System;
using System.Collections.Generic;

namespace VelorenPort.CoreEngine {
    [Serializable]
    public abstract record ItemDefinitionIdOwned {
        public sealed record Simple(string Id) : ItemDefinitionIdOwned;
        public sealed record Modular(string PseudoBase, List<ItemDefinitionIdOwned> Components) : ItemDefinitionIdOwned;
        public sealed record Compound(string SimpleBase, List<ItemDefinitionIdOwned> Components) : ItemDefinitionIdOwned;
    }
}

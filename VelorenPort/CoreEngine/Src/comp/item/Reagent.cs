using System;

namespace VelorenPort.CoreEngine.comp.item {
    /// <summary>
    /// Enumeration of reagents used for explosions and other effects.
    /// Mirrors <c>common/src/comp/inventory/item/mod.rs</c>.
    /// </summary>
    [Serializable]
    public enum Reagent {
        Blue,
        Green,
        Purple,
        Red,
        White,
        Yellow,
        Phoenix,
    }
}

using System;

namespace VelorenPort.CoreEngine.comp.item {
    /// <summary>
    /// Kinds of tools and weapons. Only a subset of the original enumeration
    /// is required for currently ported systems.
    /// Mirrors <c>common/src/comp/inventory/item/tool.rs</c>.
    /// </summary>
    [Serializable]
    public enum ToolKind {
        // weapons
        Sword,
        Axe,
        Hammer,
        Bow,
        Staff,
        Sceptre,
        // future weapons
        Dagger,
        Shield,
        Spear,
        Blowgun,
        // tools
        Debug,
        Farming,
        Pick,
        Shovel,
        /// Music Instruments
        Instrument,
        /// Throwable item
        Throwable,
        // npcs
        /// Intended for invisible weapons (e.g. claws or bites)
        Natural,
        /// Placeholder used by non-humanoid NPCs
        Empty,
    }
}

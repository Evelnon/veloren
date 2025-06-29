using System;

namespace VelorenPort.CoreEngine
{
    /// <summary>
    /// Data required to create a new character. Mirrors common/src/character.rs
    /// </summary>
    [Serializable]
    public class Character
    {
        public CharacterId? Id { get; set; }
        public string Alias { get; set; }

        public Character(CharacterId? id, string alias)
        {
            Id = id;
            Alias = alias;
        }
    }

    /// <summary>
    /// Data needed to present a character entry in a selection list.
    /// Mirrors <c>CharacterItem</c> in the Rust code.
    /// </summary>
    [Serializable]
    public class CharacterItem
    {
        public Character Character { get; set; }
        public comp.Body Body { get; set; }
        public bool Hardcore { get; set; }
        public ReducedInventory Inventory { get; set; }
        public string? Location { get; set; }

        public CharacterItem(Character character, comp.Body body, bool hardcore, ReducedInventory inventory, string? location)
        {
            Character = character;
            Body = body;
            Hardcore = hardcore;
            Inventory = inventory;
            Location = location;
        }
    }

    /// <summary>
    /// Character related constants mirrored from the Rust code.
    /// </summary>
    public static class CharacterConstants
    {
        /// <summary>Maximum number of characters a player may have.</summary>
        public const int MaxCharactersPerPlayer = 8;

        /// <summary>Maximum allowed character name length.</summary>
        public const int MaxNameLength = 20;
    }
}

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

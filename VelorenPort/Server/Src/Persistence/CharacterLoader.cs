using System.Collections.Generic;
using VelorenPort.CoreEngine;

namespace VelorenPort.Server.Persistence {
    /// <summary>
    /// Simplified character loader that works with the in-memory
    /// <see cref="CharacterUpdater"/>. Database support is planned
    /// but not yet implemented.
    /// </summary>
    public class CharacterLoader {
        private readonly Dictionary<string, List<CharacterId>> _characters = new();

        public IReadOnlyList<CharacterId> LoadCharacterList(string playerUuid) {
            return _characters.TryGetValue(playerUuid, out var list)
                ? list
                : (IReadOnlyList<CharacterId>)System.Array.Empty<CharacterId>();
        }

        public void AddCharacter(string playerUuid, CharacterId id) {
            if (!_characters.TryGetValue(playerUuid, out var list)) {
                list = new List<CharacterId>();
                _characters[playerUuid] = list;
            }
            list.Add(id);
        }
    }
}

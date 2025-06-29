using System;
using System.Collections.Generic;
using VelorenPort.CoreEngine.ECS;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Miscellaneous game resources equivalent to parts of common/src/resources.rs.
    /// </summary>
    public enum GameMode {
        Server,
        Client,
        Singleplayer,
    }

    [Serializable]
    public struct PlayerEntity {
        public Entity? Entity;
        public PlayerEntity(Entity? entity) { Entity = entity; }
    }

    [Serializable]
    public struct PlayerPhysicsSetting {
        public bool ClientOptIn;
        public bool ServerAuthoritativePhysicsOptIn() => ClientOptIn;
    }

    public class PlayerPhysicsSettings {
        public Dictionary<Guid, PlayerPhysicsSetting> Settings { get; } = new();
    }

    public enum MapKind {
        Square,
        Circle,
    }

    public enum BattleMode {
        PvP,
        PvE,
    }

    public static class BattleModeExt {
        public static bool MayHarm(this BattleMode mode, BattleMode other)
            => mode == BattleMode.PvP && other == BattleMode.PvP;
    }
}

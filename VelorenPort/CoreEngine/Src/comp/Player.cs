using System;
using System.Text.RegularExpressions;

namespace VelorenPort.CoreEngine.comp {
    using VelorenPort.CoreEngine;
    /// <summary>
    /// Player information such as alias, battle mode and UUID. Mirrors
    /// `common/src/comp/player.rs`.
    /// </summary>
    [Serializable]
    public class Player {
        public const int MAX_ALIAS_LEN = 32;

        public string Alias { get; set; }
        public BattleMode BattleMode { get; set; }
        public Time? LastBattlemodeChange { get; set; }
        private readonly Guid _uuid;

        public Player(string alias, BattleMode battleMode, Guid uuid, Time? lastBattlemodeChange) {
            Alias = alias;
            BattleMode = battleMode;
            LastBattlemodeChange = lastBattlemodeChange;
            _uuid = uuid;
        }

        public bool MayHarm(Player other) => BattleMode.MayHarm(other.BattleMode);

        public bool IsValid() => AliasValidate(Alias) == null;

        public static AliasError? AliasValidate(string alias) {
            if (!Regex.IsMatch(alias, "^[A-Za-z0-9_-]*$")) {
                return AliasError.ForbiddenCharacters;
            }
            if (alias.Length > MAX_ALIAS_LEN) {
                return AliasError.TooLong;
            }
            return null;
        }

        /// <summary>Not to be confused with Uid.</summary>
        public Guid Uuid => _uuid;
    }

    public enum AliasError {
        ForbiddenCharacters,
        TooLong,
    }

    public static class AliasErrorExt {
        public static string Message(this AliasError err) => err switch {
            AliasError.ForbiddenCharacters => "Alias contains illegal characters.",
            AliasError.TooLong => "Alias is too long.",
            _ => err.ToString(),
        };
    }

    /// <summary>
    /// Reasons why a player was disconnected. Corresponds to the Rust enum of the same name.
    /// </summary>
    [Serializable]
    public enum DisconnectReason {
        Kicked,
        NewerLogin,
        NetworkError,
        Timeout,
        ClientRequested,
        InvalidClientType,
    }
}

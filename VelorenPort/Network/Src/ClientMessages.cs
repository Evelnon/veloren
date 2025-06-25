using System;
using VelorenPort.CoreEngine;

namespace VelorenPort.Network {
    /// <summary>
    /// Type of client connecting to the server. Mirrors the enum in
    /// `common/net/src/msg/client.rs`.
    /// </summary>
    [Serializable]
    public abstract record ClientType {
        public sealed record Game : ClientType;
        public sealed record ChatOnly : ClientType;
        public sealed record SilentSpectator : ClientType;
        public sealed record Bot(bool Privileged) : ClientType;

        public bool IsValidForRole(AdminRole? role) => this switch {
            SilentSpectator => role.HasValue,
            Bot { Privileged: var p } => !p || role.HasValue,
            _ => true,
        };

        public bool EmitLoginEvents() => this is not SilentSpectator;
        public bool CanSpectate() => this is Game || this is SilentSpectator;
        public bool CanEnterCharacter() => this is Game;
        public bool CanSendMessage() => this is not SilentSpectator;
    }

    /// <summary>
    /// Data provided when registering a client connection.
    /// </summary>
    [Serializable]
    public struct ClientRegister {
        public string TokenOrUsername { get; set; }
        public string? Locale { get; set; }
        public ClientRegister(string tokenOrUsername, string? locale) {
            TokenOrUsername = tokenOrUsername;
            Locale = locale;
        }
    }
}

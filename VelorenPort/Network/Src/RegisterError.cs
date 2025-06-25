using System;

namespace VelorenPort.Network {
    /// <summary>
    /// Possible errors when registering a client. Mirrors the RegisterError enum
    /// from the Rust networking code.
    /// </summary>
    [Serializable]
    public abstract record RegisterError {
        public sealed record AuthError(string Message) : RegisterError;
        public sealed record Banned(BanInfo Info) : RegisterError;
        public sealed record Kicked(string Reason) : RegisterError;
        public sealed record InvalidCharacter : RegisterError;
        public sealed record NotOnWhitelist : RegisterError;
        public sealed record TooManyPlayers : RegisterError;
    }
}

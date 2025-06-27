using System;
using VelorenPort.Network;

namespace VelorenPort.Client {
    /// <summary>
    /// Error types returned by the client API. Mirrors <c>error.rs</c> from the
    /// Rust implementation.
    /// </summary>
    [Serializable]
    public abstract record Error {
        public sealed record Kicked(string Reason) : Error;
        public sealed record NetworkErr(NetworkError Error) : Error;
        public sealed record ParticipantErr(ParticipantError Error) : Error;
        public sealed record StreamErr(StreamError Error) : Error;
        public sealed record ServerTimeout : Error;
        public sealed record ServerShutdown : Error;
        public sealed record TooManyPlayers : Error;
        public sealed record NotOnWhitelist : Error;
        public sealed record AuthErr(string Message) : Error;
        public sealed record AuthClientError(Exception Error) : Error;
        public sealed record AuthServerUrlInvalid(string Url) : Error;
        public sealed record AuthServerNotTrusted : Error;
        public sealed record HostnameLookupFailed(Exception Error) : Error;
        public sealed record Banned(BanInfo Info) : Error;
        public sealed record InvalidCharacter : Error;
        public sealed record Other(string Message) : Error;
        public sealed record SpecsErr(Exception Error) : Error;

        public override string ToString() => this switch {
            Kicked k => $"Kicked: {k.Reason}",
            NetworkErr n => $"Network Error: {n.Error}",
            ParticipantErr p => $"Participant Error: {p.Error}",
            StreamErr s => $"Stream Error: {s.Error}",
            ServerTimeout => "Server timeout",
            ServerShutdown => "Server shutdown",
            TooManyPlayers => "Too many players",
            NotOnWhitelist => "Not on whitelist",
            AuthErr a => $"Authentication error: {a.Message}",
            AuthClientError ac => $"Auth client error: {ac.Error}",
            AuthServerUrlInvalid u => $"Invalid auth server url: {u.Url}",
            AuthServerNotTrusted => "Auth server not trusted",
            HostnameLookupFailed e => $"Hostname lookup failed: {e.Error.Message}",
            Banned b => $"Banned: {b.Info.Reason}",
            InvalidCharacter => "Invalid character",
            Other o => o.Message,
            SpecsErr se => $"Specs error: {se.Error}",
            _ => base.ToString()
        };
    }
}

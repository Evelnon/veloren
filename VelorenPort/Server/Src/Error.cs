using System;
using VelorenPort.Network;

namespace VelorenPort.Server {
    /// <summary>
    /// Error codes used throughout the server. Modeled after
    /// <c>server/src/error.rs</c> and capable of wrapping errors from the
    /// networking and persistence layers.
    /// </summary>
    public abstract record Error {
        public sealed record NetworkErr(NetworkError Error) : Error;
        public sealed record ParticipantErr(ParticipantError Error) : Error;
        public sealed record StreamErr(StreamError Error) : Error;
        public sealed record DatabaseErr(Exception Error) : Error;
        public sealed record PersistenceErr(PersistenceError Error) : Error;
        public sealed record RtsimError(Exception Error) : Error;
        public sealed record Other(string Message) : Error;

        public override string ToString() => this switch {
            NetworkErr n => $"Network Error: {n.Error}",
            ParticipantErr p => $"Participant Error: {p.Error}",
            StreamErr s => $"Stream Error: {s.Error}",
            DatabaseErr d => $"Database Error: {d.Error}",
            PersistenceErr pe => $"Persistence Error: {pe.Error}",
            RtsimError r => $"Rtsim Error: {r.Error}",
            Other o => $"Error: {o.Message}",
            _ => "Unknown Error"
        };
    }
}

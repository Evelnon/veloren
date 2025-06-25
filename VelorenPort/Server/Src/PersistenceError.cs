using System;

namespace VelorenPort.Server {
    /// <summary>
    /// Represents failures while loading or storing persistent data.
    /// Mirrors <c>persistence/error.rs</c> from the Rust codebase.
    /// </summary>
    public abstract record PersistenceError {
        public sealed record AssetError(string Message) : PersistenceError;
        public sealed record CharacterLimitReached : PersistenceError;
        public sealed record DatabaseConnectionError(Exception Error) : PersistenceError;
        public sealed record DatabaseError(Exception Error) : PersistenceError;
        public sealed record CharacterDataError : PersistenceError;
        public sealed record SerializationError(Exception Error) : PersistenceError;
        public sealed record ConversionError(string Message) : PersistenceError;
        public sealed record OtherError(string Message) : PersistenceError;

        public override string ToString() => this switch {
            AssetError a => a.Message,
            CharacterLimitReached => "Character limit exceeded",
            DatabaseConnectionError e => e.Error.Message,
            DatabaseError e => e.Error.Message,
            CharacterDataError => "Error while loading character data",
            SerializationError s => s.Error.Message,
            ConversionError c => c.Message,
            OtherError o => o.Message,
            _ => base.ToString()
        };
    }
}

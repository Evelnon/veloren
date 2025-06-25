using System;
namespace VelorenPort.Network {
    /// <summary>
    /// Errors thrown by Stream operations. In Rust these variants often carry
    /// more context, which we keep here as associated data.
    /// </summary>
    [System.Serializable]
    public abstract record StreamError {
        /// <summary>Stream was closed and can no longer send or receive.</summary>
        public sealed record StreamClosed : StreamError;

        /// <summary>Error occurred while decompressing a message.</summary>
        public sealed record Compression(Exception Error) : StreamError;

        /// <summary>Failed to deserialize the incoming message.</summary>
        public sealed record Deserialize(Exception Error) : StreamError;
    }
}

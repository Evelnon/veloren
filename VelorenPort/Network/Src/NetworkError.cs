namespace VelorenPort.Network {
    /// <summary>
    /// Errors thrown by <see cref="Network"/> methods. This replicates the
    /// <c>NetworkError</c> enum from the Rust implementation where some
    /// variants carry additional data.
    /// </summary>
    [System.Serializable]
    public abstract record NetworkError {
        /// <summary>
        /// The network has been closed and no further operations are allowed.
        /// </summary>
        public sealed record NetworkClosed : NetworkError;

        /// <summary>
        /// Failed to start listening for connections. Contains the underlying
        /// exception describing the failure.
        /// </summary>
        public sealed record ListenFailed(Exception Error) : NetworkError;

        /// <summary>
        /// Connection attempt failed with a detailed error.
        /// </summary>
        public sealed record ConnectFailed(NetworkConnectError Error) : NetworkError;
    }
}

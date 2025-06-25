namespace VelorenPort.Network {
    /// <summary>
    /// Errors thrown during connection establishment. Mirrors the
    /// <c>NetworkConnectError</c> enum from Rust where some variants carry
    /// additional information.
    /// </summary>
    [System.Serializable]
    public abstract record NetworkConnectError {
        /// <summary>
        /// Either a PID clash or an attempted connection hijack.
        /// </summary>
        public sealed record InvalidSecret : NetworkConnectError;

        /// <summary>
        /// Error that occurred during the handshake phase.
        /// Carries the detailed protocol error information.
        /// </summary>
        public sealed record Handshake(InitProtocolError<ProtocolsError> Error) : NetworkConnectError;

        /// <summary>
        /// Underlying I/O failure while establishing the connection.
        /// </summary>
        public sealed record Io(Exception Error) : NetworkConnectError;
    }
}

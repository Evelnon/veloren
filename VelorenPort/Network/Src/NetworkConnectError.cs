namespace VelorenPort.Network {
    /// <summary>
    /// Errors thrown during connection establishment.
    /// </summary>
    public enum NetworkConnectError {
        InvalidSecret,
        Handshake,
        Io
    }
}

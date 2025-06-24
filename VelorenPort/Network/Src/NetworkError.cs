namespace VelorenPort.Network {
    /// <summary>
    /// Errors thrown by Network methods.
    /// Mirrors the Rust NetworkError enum.
    /// </summary>
    public enum NetworkError {
        NetworkClosed,
        ListenFailed,
        ConnectFailed
    }
}

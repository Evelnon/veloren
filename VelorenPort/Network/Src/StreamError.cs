namespace VelorenPort.Network {
    /// <summary>
    /// Errors thrown by Stream operations.
    /// </summary>
    public enum StreamError {
        StreamClosed,
        Compression,
        Deserialize
    }
}

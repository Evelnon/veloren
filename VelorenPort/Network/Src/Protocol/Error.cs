namespace VelorenPort.Network.Protocol {
    /// <summary>
    /// Error types produced by protocol implementations.
    /// </summary>
    public abstract record ProtocolError<T> {
        public sealed record Custom(T Error) : ProtocolError<T>;
        public sealed record Violated : ProtocolError<T>;
    }
}

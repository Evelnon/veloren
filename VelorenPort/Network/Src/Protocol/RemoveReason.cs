namespace VelorenPort.Network.Protocol {
    /// <summary>
    /// Reason for removing metrics counters. Matches the Rust <c>RemoveReason</c> enum.
    /// </summary>
    public enum RemoveReason {
        Finished,
        Dropped
    }
}

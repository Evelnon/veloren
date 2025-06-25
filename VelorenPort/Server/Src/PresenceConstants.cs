namespace VelorenPort.Server
{
    /// <summary>
    /// Constants used when managing region subscriptions.
    /// </summary>
    public static class PresenceConstants
    {
        /// <summary>
        /// Distance from the stored fuzzy chunk before snapping to the
        /// current chunk.
        /// </summary>
        public const uint ChunkFuzz = 2;

        /// <summary>
        /// Distance out of a region before removing it from subscriptions.
        /// </summary>
        public const uint RegionFuzz = 16;
    }
}

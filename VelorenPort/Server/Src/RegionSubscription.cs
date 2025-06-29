using System.Collections.Generic;
using VelorenPort.NativeMath;

namespace VelorenPort.Server
{
    /// <summary>
    /// Tracks which world regions a client has subscribed to. This mirrors
    /// the server::presence::RegionSubscription struct in the original
    /// project but uses idiomatic C# collections.
    /// </summary>
    public class RegionSubscription
    {
        /// <summary>
        /// Chunk coordinate from which the subscription was last updated.
        /// It is allowed to deviate by a small fuzz factor before switching
        /// to the current chunk to avoid excessive churn.
        /// </summary>
        public int2 FuzzyChunk { get; set; }

        /// <summary>
        /// View distance used to determine which regions are relevant.
        /// </summary>
        public uint LastEntityViewDistance { get; set; }

        /// <summary>
        /// Set of region keys currently subscribed by the client.
        /// </summary>
        public HashSet<int2> Regions { get; } = new();
    }
}

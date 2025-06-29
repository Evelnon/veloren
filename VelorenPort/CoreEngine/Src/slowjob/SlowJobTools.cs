using System;

namespace VelorenPort.CoreEngine.SlowJob {
    /// <summary>
    /// Common job names used with <see cref="SlowJobPool"/>.
    /// Mirrors constants from the Rust implementation.
    /// </summary>
    public static class SlowJobTools {
        public const string ChunkGenerator = "CHUNK_GENERATOR";
    }
}

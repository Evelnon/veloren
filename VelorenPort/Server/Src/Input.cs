using System;

namespace VelorenPort.Server
{
    /// <summary>
    /// Input container used by the server subsystems. Mirrors the
    /// <c>input.rs</c> struct which currently contains no data but derives
    /// <c>Default</c>.
    /// </summary>
    [Serializable]
    public sealed class Input
    {
        /// <summary>
        /// Returns a default instance matching Rust's <c>Default</c> derive.
        /// </summary>
        public static Input Default => new Input();

        private Input() { }
    }
}

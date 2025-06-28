using System;

namespace VelorenPort.Network.Protocol {
    /// <summary>
    /// Helper utilities for protocol code.
    /// </summary>
    public static class Util {
        public static ulong Clamp(ulong value, ulong min, ulong max) => Math.Min(Math.Max(value, min), max);
    }
}

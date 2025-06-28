using System;

namespace VelorenPort.Network {
    /// <summary>
    /// Parameters used when creating a Stream. Includes promises, priority and guaranteed bandwidth.
    /// </summary>
    [Serializable]
    public struct StreamParams {
        public Promises Promises { get; private set; }
        public byte Priority { get; private set; }
        public ulong GuaranteedBandwidth { get; private set; }

        public StreamParams(Promises promises, byte priority = 0, ulong guaranteedBandwidth = 0) {
            Promises = promises;
            Priority = priority;
            GuaranteedBandwidth = guaranteedBandwidth;
        }
    }
}

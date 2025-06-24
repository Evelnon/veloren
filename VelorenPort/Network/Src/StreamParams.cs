using System;

namespace VelorenPort.Network {
    /// <summary>
    /// Parameters used when creating a Stream. Currently only promises.
    /// </summary>
    [Serializable]
    public struct StreamParams {
        public Promises Promises { get; private set; }

        public StreamParams(Promises promises) {
            Promises = promises;
        }
    }
}

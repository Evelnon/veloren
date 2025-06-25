using System;

namespace VelorenPort.Network {
    /// <summary>
    /// Errors that can occur on the internal MPSC channels.
    /// </summary>
    [Serializable]
    public abstract record MpscError {
        public sealed record Send(Exception Error) : MpscError;
        public sealed record Recv : MpscError;
    }
}

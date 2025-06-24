using System;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Reason a player disconnected from the server.
    /// Matches the enum from common/src/comp/player.rs.
    /// </summary>
    [Serializable]
    public enum DisconnectReason {
        Kicked,
        NewerLogin,
        NetworkError,
        Timeout,
        ClientRequested,
        InvalidClientType,
    }
}

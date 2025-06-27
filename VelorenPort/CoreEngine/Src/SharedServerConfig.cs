using System;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Basic configuration shared between server and client. Only exposes a
    /// couple of settings required by currently ported systems.
    /// </summary>
    [Serializable]
    public struct SharedServerConfig {
        public ServerConstants Constants;
        public string Motd;

        public SharedServerConfig(ServerConstants constants, string motd) {
            Constants = constants;
            Motd = motd;
        }

        public static SharedServerConfig Default =>
            new SharedServerConfig(new ServerConstants(), "Welcome to Veloren!");
    }
}

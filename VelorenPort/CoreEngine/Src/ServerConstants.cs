using System;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Configuration values that remain constant for the server lifetime.
    /// Mirrors shared_server_config.rs
    /// </summary>
    [Serializable]
    public struct ServerConstants {
        /// <summary>
        /// Factor by which the in-game day/night cycle is faster than real time.
        /// </summary>
        public double DayCycleCoefficient;

        public ServerConstants(double coef) {
            DayCycleCoefficient = coef;
        }
    }
}

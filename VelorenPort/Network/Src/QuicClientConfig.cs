using System;

namespace VelorenPort.Network {
    /// <summary>
    /// Configuración de cliente para conexiones QUIC. Contiene parámetros
    /// típicos usados en la inicialización del cliente rust.
    /// </summary>
    [Serializable]
    public class QuicClientConfig {
        /// <summary>Deshabilita la verificación de certificados.</summary>
        public bool InsecureSkipVerify { get; init; } = false;

        /// <summary>Tamaño máximo permitido para 0-RTT.</summary>
        public int MaxEarlyData { get; init; } = 0;
    }
}

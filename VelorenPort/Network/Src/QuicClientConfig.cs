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

        /// <summary>Tiempo máximo de inactividad antes de cerrar la conexión.</summary>
        public TimeSpan IdleTimeout { get; init; } = TimeSpan.Zero;

        /// <summary>Habilita la reanudación de sesión TLS.</summary>
        public bool AllowSessionResumption { get; init; } = false;

        /// <summary>Permite el uso de datos 0-RTT al reanudar conexiones.</summary>
        public bool EnableZeroRtt { get; init; } = false;

        /// <summary>Habilita la migración de conexión entre direcciones.</summary>
        public bool EnableConnectionMigration { get; init; } = false;
    }
}

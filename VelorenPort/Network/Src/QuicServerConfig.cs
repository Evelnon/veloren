using System;

namespace VelorenPort.Network {
    /// <summary>
    /// Configuración para servidores QUIC. Incluye opciones básicas de
    /// certificado y límites de conexión necesarias para iniciar un servidor
    /// compatible con el cliente original.
    /// </summary>
    [Serializable]
    public class QuicServerConfig {
        /// <summary>Ruta al certificado TLS del servidor.</summary>
        public string CertificatePath { get; init; } = string.Empty;

        /// <summary>Ruta a la clave privada asociada.</summary>
        public string PrivateKeyPath { get; init; } = string.Empty;

        /// <summary>Número máximo de conexiones simultáneas.</summary>
        public int MaxConnections { get; init; } = 512;

        /// <summary>Tamaño máximo de paquete en bytes.</summary>
        public int MaxPacketSize { get; init; } = 1350;

        /// <summary>Cantidad máxima de datos aceptados en 0-RTT.</summary>
        public int MaxEarlyData { get; init; } = 0;

        /// <summary>Tiempo máximo de inactividad antes de cerrar la conexión.</summary>
        public TimeSpan IdleTimeout { get; init; } = TimeSpan.Zero;

        /// <summary>Permite reanudar sesiones TLS de clientes.</summary>
        public bool AllowSessionResumption { get; init; } = false;

        /// <summary>Permite aceptar datos 0-RTT en clientes reanudados.</summary>
        public bool EnableZeroRtt { get; init; } = false;

        /// <summary>Habilita la migración de la conexión a nuevas direcciones.</summary>
        public bool EnableConnectionMigration { get; init; } = false;
    }
}

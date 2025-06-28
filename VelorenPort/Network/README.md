# Network

Corresponde al crate `network` que implementa la capa de comunicación utilizando QUIC y serialización.

**Viabilidad**: Alta. Existe soporte en C# para networking asíncrono (sockets, QUIC mediante librerías externas). Será necesario portar los mensajes y protocolos definiendo estructuras equivalentes.

**Notas**:
- Traducir las estructuras de mensajes definidas con `serde` a clases C# usando `System.Text.Json` o similar.
- Evaluar uso de bibliotecas de QUIC en C# o migrar a WebSockets si es más conveniente para Unity.

Se añadieron las uniones discriminadas `ConnectAddr` y `ListenAddr` (con soporte para configuración QUIC y UDP) y el tipo `ParticipantEvent` en `Src/` junto con la definición de la assembly `Network`.
Se creó también la carpeta `Protocol/` con estructuras mínimas que replican el subcrate `network-protocol` de Rust.
Se agregaron también `Pid`, `Sid`, `Promises`, `StreamParams` y `Message` para comenzar a manejar identificadores y serialización de mensajes de forma básica.
Se sumaron las estructuras `Channel` y `Participant` para gestionar colas de mensajes simuladas. También se añadieron las enumeraciones `NetworkError`, `NetworkConnectError`, `ParticipantError` y `StreamError` junto a la clase `Stream` para cubrir los mensajes de error básicos y el flujo de comunicación.
El módulo ahora incluye utilidades y estructura de soporte:
`Metrics` para contar tráfico de red (ahora con integración Prometheus), `Scheduler` para tareas asincrónicas concurrentes, `Util` con funciones auxiliares y `Api` como punto de entrada de alto nivel.

Se recomienda avanzar por fases, migrando primero las definiciones de mensajes y manteniendo una capa de compatibilidad con el servidor en Rust. El resto de la lógica de networking puede portarse gradualmente para facilitar las pruebas.
Se añadió igualmente la clase `Network` con métodos asíncronos de `ListenAsync` y `ConnectAsync` para orquestar las conexiones. Desde esta versión se soporta UDP además de TCP y QUIC. También se implementaron conexiones locales MPSC para pruebas sin red.
El proceso de *handshake* ahora intercambia los identificadores `Pid` y un secreto aleatorio para autenticar nuevos canales, replicando el comportamiento del servidor Rust.
Además se implementó `ClientType` junto con la estructura `ClientRegister` para
describir el tipo de cliente y los datos iniciales de registro que requiere el
servidor. La lógica de validación de roles y permisos sigue la misma que en
Rust gracias a los métodos `IsValidForRole`, `CanSpectate` y similares.

## Analysis of Remaining Migration Tasks

Despite the `100%` mark that appears in older documents, the current C# module
mirrors only a subset of the original Rust crate. Several areas remain
incomplete or entirely missing:


### Advanced Stream Management

The Rust crate implements multiple queues with priority levels and a robust
reliability layer. The C# `Stream` class only contains a basic retransmission
system. Congestion control and message prioritization are not implemented.
Handshake negotiation is simplified and does not cover the full state machine
used in Rust.

### Metrics and Monitoring

`Metrics` integrates the `prometheus-net` package to expose counters. Only a
subset of the events from the Rust version are tracked. Several network
statistics, like per-channel congestion and scheduler load, are still missing.

### Scheduler and Concurrency

The scheduler executes queued tasks and provides `DisconnectAsync` and
`ShutdownAsync` helpers. It lacks the advanced task prioritization and dynamic
load balancing present in the Rust implementation. Graceful shutdown of pending
operations is still limited.

### Protocol Coverage

UDP transport is experimental and lacks the reliability and prioritization
layers provided by the Rust crate. Only basic QUIC configuration options are
supported.

### Compatibility with Existing Rust Server

The current C# server logs connection attempts but does not exchange real
messages with the Rust server. Protocol negotiation, authentication and
encryption steps from the original project are not replicated.

### FFI/Interop Considerations

`Docs/Interoperabilidad.md` notes that integration with Rust code via FFI or
Wasm is still under investigation. No interop layer has been implemented.

### Testing Status

Only a handful of tests cover the local MPSC transport. End-to-end integration
tests comparable to the Rust suite are missing.

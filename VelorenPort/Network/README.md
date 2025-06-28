# Network

Corresponde al crate `network` que implementa la capa de comunicación utilizando QUIC y serialización.

**Viabilidad**: Alta. Existe soporte en C# para networking asíncrono (sockets, QUIC mediante librerías externas). Será necesario portar los mensajes y protocolos definiendo estructuras equivalentes.

**Notas**:
- Traducir las estructuras de mensajes definidas con `serde` a clases C# usando `System.Text.Json` o similar.
- Evaluar uso de bibliotecas de QUIC en C# o migrar a WebSockets si es más conveniente para Unity.

Se añadieron las uniones discriminadas `ConnectAddr` y `ListenAddr` (con soporte para configuración QUIC) y el tipo `ParticipantEvent` en `Src/` junto con la definición de la assembly `Network`.
Se agregaron también `Pid`, `Sid`, `Promises`, `StreamParams` y `Message` para comenzar a manejar identificadores y serialización de mensajes de forma básica.
Se sumaron las estructuras `Channel` y `Participant` para gestionar colas de mensajes simuladas. También se añadieron las enumeraciones `NetworkError`, `NetworkConnectError`, `ParticipantError` y `StreamError` junto a la clase `Stream` para cubrir los mensajes de error básicos y el flujo de comunicación.
El módulo ahora incluye utilidades y estructura de soporte:
`Metrics` para contar tráfico de red, `Scheduler` para tareas asincrónicas, `Util` con funciones auxiliares y `Api` como punto de entrada de alto nivel.

Se recomienda avanzar por fases, migrando primero las definiciones de mensajes y manteniendo una capa de compatibilidad con el servidor en Rust. El resto de la lógica de networking puede portarse gradualmente para facilitar las pruebas.
Se añadió igualmente la clase `Network` con métodos asíncronos de `ListenAsync` y `ConnectAsync` para orquestar las conexiones.
Desde esta versión se ha portado el protocolo de *handshake*, validando el número mágico `VELOREN` y la versión de red antes de establecer cada conexión.
Además se implementó `ClientType` junto con la estructura `ClientRegister` para
describir el tipo de cliente y los datos iniciales de registro que requiere el
servidor. La lógica de validación de roles y permisos sigue la misma que en
Rust gracias a los métodos `IsValidForRole`, `CanSpectate` y similares.

## Analysis of Remaining Migration Tasks

Despite the `100%` mark in `MigrationStatus.md`, the current C# module still
mirrors only a subset of the original Rust crate. Important areas remain
partially implemented or entirely missing:


### Advanced Stream Management

Rust uses channels, priority queues and reliability layers. The `Stream` class
here is a simple unidirectional queue with no prioritization or bandwidth
control.

### Metrics and Monitoring

Prometheus counters in Rust track many events. The `Metrics` class only counts
bytes and message totals and lacks integration with monitoring tools.

### Scheduler and Concurrency

The Rust scheduler coordinates multiple tasks and ensures graceful shutdowns.
The C# `Scheduler` is a minimal serial queue.

### Protocol Coverage

Support for TCP, UDP, QUIC and other transports is not fully realised. The
implementation mostly wraps TCP and QUIC without additional reliability logic.

### Compatibility with Existing Rust Server

While a compatibility layer is mentioned, the server code currently logs events
instead of performing real communication, so behaviour differs from the Rust
version.

### FFI/Interop Considerations

`Docs/Interoperabilidad.md` notes that integration of Rust modules through FFI
or Wasm is still *pendiente de investigación*.

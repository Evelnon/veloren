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
Se añadió igualmente la clase `Network` con métodos asíncronos de `ListenAsync` y `ConnectAsync` para orquestar las conexiones. Desde esta versión se soporta UDP además de TCP y QUIC.
El proceso de *handshake* ahora intercambia los identificadores `Pid` y un secreto aleatorio para autenticar nuevos canales, replicando el comportamiento del servidor Rust.
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
now includes optional bandwidth throttling when `StreamParams` sets
`GuaranteedBandwidth`, limiting the amount of bytes written per second.
Additionally, when the `Promises.GuaranteedDelivery` flag is present the stream
implements a basic reliability layer with acknowledgments and automatic
retransmission. Sophisticated prioritization is still pending.

### Metrics and Monitoring

Ahora se exponen contadores Prometheus usando la librería `prometheus-net`, de
modo que pueden consultarse desde herramientas externas. La cobertura de eventos
es todavía limitada respecto al crate original. Desde esta versión el envío y
recepción de mensajes, así como la apertura y cierre de `Streams` y
`Participants`, incrementan contadores automáticamente para facilitar la
observación durante las pruebas.

### Scheduler and Concurrency

El planificador ha sido actualizado para ejecutar tareas en paralelo de forma
segura e incluye un método de cierre ordenado similar al `Scheduler` original.

### Protocol Coverage

Se añadió soporte experimental para UDP además de TCP y QUIC. Todavía faltan las
capas de fiabilidad y priorización presentes en el proyecto original.

### Compatibility with Existing Rust Server

While a compatibility layer is mentioned, the server code currently logs events
instead of performing real communication, so behaviour differs from the Rust
version.

### FFI/Interop Considerations

`Docs/Interoperabilidad.md` notes that integration of Rust modules through FFI
or Wasm is still *pendiente de investigación*.

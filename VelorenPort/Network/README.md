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
El proceso de *handshake* se divide ahora en dos pasos. Primero se envía un encabezado con el número mágico y la versión de red y, tras validarlo, se intercambia un paquete de inicialización con el `Pid`, un secreto y las banderas de `HandshakeFeatures`. Esto replica de manera más fiel la negociación utilizada por el servidor en Rust. Las aplicaciones pueden especificar las banderas deseadas al llamar a `ListenAsync` o `ConnectAsync`.
Desde esta versión también se devuelve la versión exacta de red del interlocutor y se calcula la intersección de `HandshakeFeatures` entre ambos clientes para activar únicamente las opciones soportadas por ambos extremos. La versión remota queda registrada en cada `Participant` para poder validar la compatibilidad en etapas posteriores.
Ahora `Handshake.PerformAsync` negocia las capacidades opcionales: tras recibir las banderas del par remoto calcula la intersección con las locales y devuelve solo las características habilitadas por ambos. Esto simplifica la activación automática de compresión o cifrado cuando está disponible.
Adicionalmente el *handshake* asigna un desplazamiento inicial de `Sid` para que cada parte numere sus flujos en rangos distintos evitando colisiones.
El intercambio concluye con el envío de un byte de confirmación que obliga a ambos extremos a esperar hasta recibirlo, garantizando que la negociación ha finalizado correctamente antes de abrir streams.
El procedimiento se implementa internamente como una máquina de estados (`SendHeader`, `ReceiveHeader`, `SendInit`, etc.) similar a la versión en Rust, lo que facilitará añadir compatibilidad con versiones futuras.
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
reliability layer. The C# `Stream` class implements retransmission with a
dynamic congestion window that follows a simplified AIMD algorithm. Priority
weights can be adjusted globally via `Api.SetStreamPriorityWeights`, but more
advanced scheduling is still missing.
Handshake negotiation is simplified and does not cover the full state machine
used in Rust.

### Metrics and Monitoring

`Metrics` integrates the `prometheus-net` package to expose counters and gauges.
El sistema reporta bytes y mensajes por canal, congestión encolada y carga del
`Scheduler`. Se añadieron gauges para medir el RTT instantáneo y el RTT promedio de los streams fiables.
Además mantiene un historial rotativo de eventos accesible mediante `DrainEvents` para fines de depuración y es posible
registrar estos eventos en un archivo con `StartEventLog` / `StopEventLog`.

### Message Validation (Debug)

Durante las compilaciones de depuración, la clase `Stream` comprueba que la compresión de cada `Message` coincida con las `Promises` del flujo. Esta verificación permite detectar errores tempranos al enviar o recibir datos.

### Scheduler and Concurrency

The scheduler executes queued tasks, informa su carga promedio y la cantidad
de trabajadores activos. Los trabajadores pueden ajustarse dinámicamente con
`SetSchedulerWorkers` y es posible habilitar un modo de autoescalado básico
que ajusta el número de hilos según la cola de tareas. Aun así, falta una
priorización avanzada y un balanceo más fino. Se puede drenar el planificador
de forma opcional durante el apagado.

### Protocol Coverage

UDP transport now implements a simple reliability layer with acknowledgments
and retransmissions. Streams identify packets by ID so loss is detected and
recovered transparently. Streams may optionally encrypt payloads using a key
derived from the shared handshake secret. Prioritization remains basic and
QUIC options are still limited.

### Compatibility with Existing Rust Server

The current C# server logs connection attempts but does not exchange real
messages with the Rust server. Protocol negotiation and authentication remain
unimplemented, though streams can already encrypt their payloads if both sides
negotiate the feature during the handshake.

### FFI/Interop Considerations

`Docs/Interoperabilidad.md` notes that integration with Rust code via FFI or
Wasm is still under investigation. No interop layer has been implemented.

### Testing Status

Only a handful of tests cover the local MPSC transport. End-to-end integration
tests comparable to the Rust suite are missing.

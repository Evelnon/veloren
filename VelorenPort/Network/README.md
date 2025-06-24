# Network

Corresponde al crate `network` que implementa la capa de comunicación utilizando QUIC y serialización.

**Viabilidad**: Alta. Existe soporte en C# para networking asíncrono (sockets, QUIC mediante librerías externas). Será necesario portar los mensajes y protocolos definiendo estructuras equivalentes.

**Notas**:
- Traducir las estructuras de mensajes definidas con `serde` a clases C# usando `System.Text.Json` o similar.
- Evaluar uso de bibliotecas de QUIC en C# o migrar a WebSockets si es más conveniente para Unity.

Se añadieron las clases `ConnectAddr`, `ListenAddr` y `ParticipantEvent` en `Src/` junto con la definicion de la assembly `Network`.
Se agregaron tambien `Pid`, `Sid`, `Promises`, `StreamParams` y `Message` para comenzar a manejar identificadores y serializacion de mensajes de forma basica.
Se sumaron las estructuras `Channel` y `Participant` para gestionar colas de mensajes simuladas.
Se recomienda avanzar por fases, migrando primero las definiciones de mensajes y manteniendo una capa de compatibilidad con el servidor en Rust. El resto de la lógica de networking puede portarse gradualmente para facilitar las pruebas.
Se añadió igualmente un esqueleto `Network` con métodos asíncronos de `ListenAsync` y `ConnectAsync` para orquestar las conexiones.

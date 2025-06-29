# Network Port Missing Features

Este documento resume las funcionalidades del crate `network` de Rust que aún no se han portado a C# o están incompletas.

- **Gestión avanzada de streams**: la clase `Stream` cuenta con colas de prioridad *weighted round robin*, reenvío con confirmaciones, ventana de congestión, cierre explícito de streams con reconocimiento y métodos genéricos de envío/recepción para mensajes serializados. Aún faltan controles más finos de fiabilidad.
- **Métricas de red**: se añadieron contadores y gauges para monitorizar la carga del `Scheduler`, los participantes activos y el número de trabajadores. Sigue pendiente instrumentar todas las rutas de datos.
- **Planificador y concurrencia**: el `Scheduler` soporta un número configurable de trabajadores y puede detenerse de forma ordenada. Aún faltan estrategias avanzadas de balanceo como en Rust.
- **Configuración del planificador**: la API permite elegir la cantidad de trabajadores al instanciarse, pero no ajusta dinámicamente la carga como la versión de Rust.
- **Actualizaciones de ancho de banda**: los `Participant` emiten un evento cuando cambia la estimación de ancho de banda. Resta añadir controles de QoS más precisos.
- **Eventos de desconexión**: ahora los `Participant` notifican mediante un evento cuando se desconectan y sus flujos se limpian automáticamente.
- **Eventos de conexión**: la `Network` y la `Api` exponen notificaciones cuando un participante se conecta o se desconecta.
- **Gestión de la API**: la capa `Api` expone métodos para desconectar participantes y controlar el servidor de métricas.
- **Consultas de estado**: se añadió una interfaz para listar participantes conectados y obtener un resumen de métricas.
- **Estadísticas de participantes**: se pueden consultar los bytes enviados y recibidos por cada participante y recibir notificaciones cuando se abren o cierran streams.
- **Validación de mensajes**: los `Message` verifican en modo debug que la compresión sea coherente con las promesas del `Stream`.
- **Control de escucha**: el `Network` puede detener sus sockets con `StopListening` y la `Api` permite consultar participantes por id.
- **Cobertura de protocolos**: el transporte UDP es experimental y no reproduce las garantías de QUIC del proyecto original. La negociación de protocolos es mínima.
- **Compatibilidad con el servidor Rust**: el servidor C# sólo acepta conexiones de prueba; no se intercambian mensajes reales ni se replican los pasos de autenticación y cifrado.
- **Interoperabilidad FFI/Wasm**: aún no existe una capa que permita comunicarse con código Rust o WebAssembly.
- **Pruebas automatizadas**: sólo se han portado algunos tests locales de MPSC; faltan pruebas integrales comparables a la suite de Rust.

La migración continuará abordando estas áreas para alcanzar la paridad con la implementación original.

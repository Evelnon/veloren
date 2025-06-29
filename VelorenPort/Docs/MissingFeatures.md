# Características faltantes en la versión C#

Este documento describe con detalle las funcionalidades del servidor de Veloren en Rust que no se han portado o están implementadas de forma parcial en la rama C#. La lista se basa en una inspección directa del código `VelorenPort/Server` comparado con `server/src` del proyecto Rust.

## 1. Infraestructura principal

- Falta un sistema ECS completo (`State`) y la integración del `Dispatcher` de sistemas.
- `GameServer` solo gestiona conexiones y un ciclo de ticks; no inicializa ni ejecuta sistemas del juego.
- La inserción de recursos globales y la configuración de mundo que realiza Rust en `initialize_world` no tiene equivalencia.

## 2. Sistemas de `server/src/sys` sin equivalente

La carpeta `server/src/sys` en Rust define más de 15 sistemas que orquestan la simulación y la sincronización. Ninguno ha sido migrado:

- `agent::Sys` para la IA de entidades.
- `chunk_send::Sys` implementado de forma básica para reenviar chunks.
- `chunk_serialize::Sys` implementado para serializar chunks generados.
- `entity_sync::Sys` ahora envía posiciones simples de jugadores.
- `terrain_sync::Sys` implementado para sincronizar terreno visible.
- `invite_timeout::Sys` gestión de invitaciones. ✅ Implementado de forma básica.
- `chat::Sys` registro y difusión de mensajes. ✅ Implementado de forma básica.
 - `item::Sys` y `loot::Sys` relativos a objetos y botín. ✅ `loot::Sys` implementado de forma básica.
- `object::Sys` para la interacción con objetos. ✅ Implementado de forma básica.
- `pets::Sys` que controla las mascotas del jugador. ✅ Implementado de forma básica.
- `sentinel::Sys` responsable de la IA de centinelas. ✅ Implementado de forma básica.
- `teleporter::Sys` y `waypoint::Sys` para los puntos de viaje rápido. ✅ Implementados de forma básica.
- `subscription::Sys` mantiene las regiones observadas por cada jugador. ✅ Implementado de forma básica.
- `persistence::Sys` ahora guarda los cambios en disco cada minuto, aunque sin base de datos.
- `server_info::Sys` ahora envía información básica del servidor a los clientes cada minuto. ✅ Implementado de forma básica.
- `wiring::Sys` para el sistema de cableado y señales. ✅ Implementado de forma básica.

## 3. Persistencia y migraciones

- No se ha portado la base de datos SQLite ni los esquemas de migración.
- `CharacterLoader` ahora persiste las listas de personajes en disco.
- `TerrainPersistence` incluye una pequeña caché LRU y serialización binaria, pero sin compresión.

## 4. Plugins y extensibilidad

- Se añadió un `PluginManager` que carga ensamblados `.dll` desde la carpeta `plugins` y ejecuta su método `Initialize`. Aún no hay soporte para WebAssembly ni API pública estable.

## 5. Métricas y monitorización

- `sys/metrics.rs` expone métricas Prometheus. La versión en C# ahora incluye un
  `PrometheusExporter` que sirve estadísticas básicas a través de HTTP.

## 6. Simulaciones en tiempo real

- `rtsim` en Rust gestiona cálculos pesados de IA y entorno. El stub `Rtsim/RtSim.cs` solo guarda la fecha de inicio.
- El módulo `weather` actualiza nubosidad y precipitaciones; en C# ahora existe un `WeatherSystem` sencillo que envía actualizaciones aleatorias e interpola transiciones de clima.
- Se añadió `Nature` con un mapa simplificado de `ChunkResource` por chunk, pero la simulación de recursos sigue incompleta.
- Se incorporó `HumidityMap` para rastrear la humedad por chunk y se añadió una
  difusión básica; sigue faltando la erosión y el modelo avanzado del original.
- Se añadió un esqueleto `LayerManager` para futuras capas dinámicas y se
  implementó una primera capa `Scatter` que coloca puntos de interés simples.
  Aún faltan cuevas y otros tipos de capas.
 - `Canvas` detecta bloques con recursos y posiciones de aparición, copiándolos
   a `ChunkSupplement`, aunque la generación todavía no usa esta información.

## 7. Módulos de juego no migrados

- Lógica de combate y control de NPC, incluidas mascotas, teleporters y waypoints.
- Estados de personaje ahora incluyen la mayoría de variantes comunes (bloqueos, ataques avanzados y estados de movimiento), aunque faltan transformaciones y otras lógicas complejas.
- Se incorporó AttackSource y AttackFilters con utilidades de combate y se ampliaron los helpers de CharacterState (dodge, follow look, etc.).
- Se añadió ForcedMovement y MovementDirection para replicar empujes y saltos dirigidos.
- Se incorporaron los componentes `Alignment` y `Group` junto con `CharacterItem` para listas de personajes. Se añadió un `GroupManager` básico sin notificaciones ni mascotas.
- Se añadieron las enumeraciones `SiteKind`, `PoiKind` y `MarkerKind`, y el mensaje de mapa ahora expone esta información. `SiteKindMeta` permite clasificar asentamientos y mazmorras.
- Mecanismos de moderación y comandos avanzados solo tienen esqueletos (‘Automod’, etc.).

- Los mensajes definidos en `sys/msg` para serializar actualizaciones no existen en la versión C#.
- `ObjectSystem` elimina objetos temporales y ahora incluye un `PortalSystem` que teletransporta al jugador tras un breve tiempo de espera. Aún faltan mecánicas de no-agresión y otras comprobaciones avanzadas.

## 8. Otros componentes incompletos

- Falta una CLI de administración completa y ajustes detallados de configuración.
- No hay pruebas automáticas comparables a las de Rust.
- Se añadieron `VolGrid2d` y `VolGrid3d` como contenedores de volúmenes simples, aunque sigue faltando el soporte completo de metadatos.
- Se añadió `NavGrid` para definir celdas bloqueadas dentro de la malla de navegación y se actualizó `Searcher` para usarlo.
- `RegionMap` ahora puede guardar y cargar su historial, aunque siguen faltando políticas de descarte avanzadas y un registro permanente.
- Se implementó `SlowJobPool` y un módulo de figuras con `MatCell` y
  `DynaUnionizer`, aunque faltan herramientas de animación y cargado de modelos.
- No hay implementación en C# de `common/query_server` para el descubrimiento de servidores.
- El módulo `server/src/events` no está portado; faltan tipos de eventos y la lógica del bus de eventos.
- El proveedor de inicio de sesión es simplificado y no maneja listas de baneos, listas blancas ni roles de administrador.

## Resumen

Aunque la estructura de carpetas replica la del servidor original, muchas clases son marcadores de posición o implementaciones reducidas. Para alcanzar la paridad con el código en Rust debe integrarse un ECS real, implementar todos los sistemas listados, agregar persistencia en disco, exponer métricas de forma equivalente y completar la simulación del mundo.

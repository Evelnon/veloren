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
- `item::Sys` y `loot::Sys` relativos a objetos y botín.
- `object::Sys` para la interacción con objetos.
- `pets::Sys` que controla las mascotas del jugador.
- `sentinel::Sys` responsable de la IA de centinelas.
- `teleporter::Sys` y `waypoint::Sys` para los puntos de viaje rápido.
- `subscription::Sys` mantiene las regiones observadas por cada jugador.
- `persistence::Sys` ahora guarda los cambios en disco cada minuto, aunque sin base de datos.
- `server_info::Sys` ahora envía información básica del servidor a los clientes cada minuto.
- `wiring::Sys` para el sistema de cableado y señales.

## 3. Persistencia y migraciones

- No se ha portado la base de datos SQLite ni los esquemas de migración.
- `CharacterLoader` sólo almacena personajes en memoria.
- `TerrainPersistence` carece de la caché LRU y de la serialización o compresión de chunks.

## 4. Plugins y extensibilidad

- El servidor Rust permite cargar plugins (WebAssembly) mediante `PluginMgr`. Actualmente no existe un sistema equivalente en C#.

## 5. Métricas y monitorización

- `sys/metrics.rs` expone métricas Prometheus. En C# existe una clase `Metrics` con contadores y gauges básicos, pero aún no exporta datos a Prometheus.

## 6. Simulaciones en tiempo real

- `rtsim` en Rust gestiona cálculos pesados de IA y entorno. El stub `Rtsim/RtSim.cs` solo guarda la fecha de inicio.
- El módulo `weather` actualiza nubosidad y precipitaciones; en C# ahora existe un `WeatherSystem` sencillo que envía actualizaciones aleatorias.
- Se añadió `Nature` con un mapa simplificado de `ChunkResource` por chunk, pero la simulación de recursos sigue incompleta.
- Se incorporó `HumidityMap` para rastrear la humedad por chunk y se añadió una
  difusión básica; sigue faltando la erosión y el modelo avanzado del original.
- Se añadió un esqueleto `LayerManager` para futuras capas dinámicas, pero aún
  no existen cuevas ni dispersión de objetos.

## 7. Módulos de juego no migrados

- Lógica de combate y control de NPC, incluidas mascotas, teleporters y waypoints.
- Estados de personaje ahora incluyen la mayoría de variantes comunes (bloqueos, ataques avanzados y estados de movimiento), aunque faltan transformaciones y otras lógicas complejas.
- Se incorporó AttackSource y AttackFilters con utilidades de combate y se ampliaron los helpers de CharacterState (dodge, follow look, etc.).
- Se añadió ForcedMovement y MovementDirection para replicar empujes y saltos dirigidos.
- Se incorporaron los componentes `Alignment` y `Group` junto con `CharacterItem` para listas de personajes. Se añadió un `GroupManager` básico sin notificaciones ni mascotas.
- Se añadieron las enumeraciones `SiteKind`, `PoiKind` y `MarkerKind`, y el mensaje de mapa ahora expone esta información. `SiteKindMeta` permite clasificar asentamientos y mazmorras.
- Mecanismos de moderación y comandos avanzados solo tienen esqueletos (‘Automod’, etc.).
- Los mensajes definidos en `sys/msg` para serializar actualizaciones no existen en la versión C#.

## 8. Otros componentes incompletos

- Falta una CLI de administración completa y ajustes detallados de configuración.
- No hay pruebas automáticas comparables a las de Rust.
- Se añadieron `VolGrid2d` y `VolGrid3d` como contenedores de volúmenes simples, aunque sigue faltando el soporte completo de metadatos.
- Se implementó `SlowJobPool` y un módulo de figuras con `MatCell` y
  `DynaUnionizer`, aunque faltan herramientas de animación y cargado de modelos.

## Resumen

Aunque la estructura de carpetas replica la del servidor original, muchas clases son marcadores de posición o implementaciones reducidas. Para alcanzar la paridad con el código en Rust debe integrarse un ECS real, implementar todos los sistemas listados, agregar persistencia en disco, exponer métricas de forma equivalente y completar la simulación del mundo.

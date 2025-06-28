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
- `invite_timeout::Sys` gestión de invitaciones.
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

- `sys/metrics.rs` expone métricas Prometheus. En C# solo hay una clase `Metrics` que imprime los ticks por consola.

## 6. Simulaciones en tiempo real

- `rtsim` en Rust gestiona cálculos pesados de IA y entorno. El stub `Rtsim/RtSim.cs` solo guarda la fecha de inicio.
- El módulo `weather` actualiza nubosidad y precipitaciones; en C# `WeatherJob.cs` contiene un contador sin lógica de simulación.

## 7. Módulos de juego no migrados

- Lógica de combate y control de NPC, incluidas mascotas, teleporters y waypoints.
- Mecanismos de moderación y comandos avanzados solo tienen esqueletos (‘Automod’, etc.).
- Los mensajes definidos en `sys/msg` para serializar actualizaciones no existen en la versión C#.

## 8. Otros componentes incompletos

- Falta una CLI de administración completa y ajustes detallados de configuración.
- No hay pruebas automáticas comparables a las de Rust.

## Resumen

Aunque la estructura de carpetas replica la del servidor original, muchas clases son marcadores de posición o implementaciones reducidas. Para alcanzar la paridad con el código en Rust debe integrarse un ECS real, implementar todos los sistemas listados, agregar persistencia en disco, exponer métricas de forma equivalente y completar la simulación del mundo.

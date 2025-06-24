# Plan de Acción Detallado

Este documento describe paso a paso el port del código de Veloren a C# y Unity. Se listan los ficheros fuente principales de cada sistema y se sugieren tareas iniciales para la migración. La idea es usar este plan como referencia continua durante todo el proceso.

## Estructura general
- Cada sistema de Rust se convertirá en una **Assembly Definition** en Unity.
- Se respetará la jerarquía de carpetas actual para mantener la organización.
- Las pruebas unitarias se reescribirán usando el framework de pruebas de Unity.

Hasta ahora se han creado las assemblies `CoreEngine` y `Network`, con sus primeros archivos de código en C#, incluyendo direcciones, eventos e identificadores de red. El módulo de red incorpora un esqueleto de clase `Network` con participantes y canales simulados para comenzar a probar conexiones. Se añadieron además módulos auxiliares (`Metrics`, `Scheduler`, `Util` y un `Api` público) para preparar la funcionalidad completa. Recientemente se añadieron los enums de error (`NetworkError`, `NetworkConnectError`, `ParticipantError`, `StreamError`) y la clase `Stream` para cubrir la señalización básica de fallos y el flujo de mensajes.
Se suma la assembly `World` con estructuras de terreno simplificadas para iniciar el port del crate `world`.

Actualmente el port incluye `Uid`, `CharacterId`, `RtSimEntity`, `Calendar`, `DayPeriod`, `Clock`, los recursos de tiempo, `Consts` y `ViewDistances`. Se sumaron `GameMode`, `PlayerEntity`, `PlayerPhysicsSettings`, `MapKind`, `BattleMode`, una versión con datos de `Actor`, `ServerConstants`, `Pos` y `EntitiesDiedLastTick`. Se agregó `Grid` para manejar datos bidimensionales, seguido de `Presence` con el enumerado `PresenceKind` y un campo opcional `CharacterId` para las variantes `LoadingCharacter` y `Character`, además de `ViewDistance` para la visibilidad. Si la presencia cambia a otra variante, este identificador se borra. Ahora se añadió `SpatialGrid` y el recurso `CachedSpatialGrid` para agilizar consultas de proximidad. También se implementaron `Path`, `AStar` y `Ray` para el cálculo de rutas y recorridos de voxels.

## 1. CoreEngine (crate `common`)
### Ficheros relevantes
- astar.rs
- cached_spatial_grid.rs
- calendar.rs
- character.rs
- clock.rs
- cmd.rs
- combat.rs
- consts.rs
- depot.rs
- effect.rs
- event.rs
- explosion.rs
- generation.rs
- grid.rs
- interaction.rs
- lib.rs
- link.rs
- lod.rs
- lottery.rs
- mounting.rs
- npc.rs
- outcome.rs
- path.rs
- ray.rs
- recipe.rs
- region.rs
- resources.rs
- rtsim.rs
- shared_server_config.rs
- skillset_builder.rs
- slowjob.rs
- spiral.rs
- spot.rs
- store.rs
- tether.rs
- time.rs
- trade.rs
- typed.rs
- uid.rs
- view_distances.rs
- vol.rs
- weather.rs

### Pasos recomendados
1. Crear un proyecto de biblioteca en Unity llamado **CoreEngine**.
2. Convertir cada módulo en un espacio de nombres de C# manteniendo la funcionalidad.
3. Reemplazar macros y rasgos genéricos por clases base y composición.
4. Utilizar `Unity.Mathematics` para operaciones de vectores y matrices.
5. Implementar pruebas de comportamiento equivalentes en el entorno de Unity.

## 2. Network (crate `network`)
### Ficheros relevantes
- api.rs
- channel.rs
- lib.rs
- message.rs
- metrics.rs
- participant.rs
- scheduler.rs
- util.rs

### Pasos recomendados
1. Crear la **assembly** `Network` en Unity.
2. Definir estructuras de mensajes en C# (clases serializables).
3. Usar `System.Net.Sockets` o una librería QUIC para las conexiones.
4. Implementar un sistema de serialización eficiente (por ejemplo `System.Text.Json`).
5. Mantener la arquitectura asíncrona mediante `async`/`await`.

- Evaluar si es viable migrar todo el crate de una sola vez o abordar el port por fases, priorizando primero la mensajería básica y la compatibilidad con el servidor en Rust.

## 3. World (crate `world`)
### Ficheros relevantes
- all.rs
- block.rs
- canvas.rs
- column.rs
- config.rs
- index.rs
- land.rs
- lib.rs
- pathfinding.rs
- sim2.rs
- directorios: `civ/`, `layer/`, `sim/`, `site/`, `util/`

### Pasos recomendados
1. Crear la **assembly** `World`.
2. Mapear las estructuras de terreno y generación procedural a clases C#.
3. Aprovechar `Burst` y `Jobs` de Unity para cálculos de terreno intensivos.
4. Revisar dependencias de crates externos y buscar equivalentes en C#.
5. Asegurar la compatibilidad con el sistema de guardado de Unity.

## 4. Server (crate `server`)
### Ficheros relevantes
- automod.rs
- character_creator.rs
- chat.rs
- chunk_generator.rs
- chunk_serialize.rs
- client.rs
- cmd.rs
- connection_handler.rs
- data_dir.rs
- error.rs
- input.rs
- lib.rs
- location.rs
- lod.rs
- login_provider.rs
- metrics.rs
- pet.rs
- presence.rs
- state_ext.rs
- terrain_persistence.rs
- test_world.rs
- wiring.rs
- directorios: `events/`, `migrations/`, `persistence/`, `rtsim/`, `settings/`, `sys/`, `weather/`

### Pasos recomendados
1. Implementar la **assembly** `Server` y separar lógica de juego del código de red.
2. Traducir los sistemas ECS de Rust a componentes de Unity o un framework ECS propio.
3. Portar scripts de migración y persistencia utilizando bases de datos compatibles con C# (por ejemplo SQLite).
4. Integrar el servidor con el cliente Unity a través del módulo de red creado previamente.
5. Automatizar pruebas de carga para validar el rendimiento en C#.

## 5. Client (crates `client` y `voxygen`)
### Ficheros relevantes
- client/addr.rs
- client/error.rs
- client/lib.rs
- voxygen/cli.rs
- voxygen/cmd.rs
- voxygen/controller.rs
- voxygen/credits.rs
- voxygen/discord.rs
- voxygen/error.rs
- voxygen/game_input.rs
- voxygen/key_state.rs
- voxygen/lib.rs
- voxygen/main.rs
- voxygen/panic_handler.rs
- voxygen/profile.rs
- voxygen/run.rs
- voxygen/window.rs
- directorios: `client/src/bin/`

### Pasos recomendados
1. Crear la **assembly** `Client` y migrar la lógica de entrada y UI usando el sistema de escenas de Unity.
2. Utilizar el Input System de Unity para reemplazar `game_input.rs` y `key_state.rs`.
3. Reescribir la ventana y el ciclo principal con `MonoBehaviour` y escenas.
4. Portar el renderizado y shaders a la tubería de render de Unity (URP o HDRP).
5. Integrar Discord y perfiles mediante plugins C# específicos.

## 6. Simulation (crate `rtsim`)
### Ficheros relevantes
- event.rs
- lib.rs
- directorios: `ai/`, `data/`, `gen/`, `rule/`

### Pasos recomendados
1. Evaluar si es viable reescribir todo el simulador en C# o mantenerlo como biblioteca Rust vía FFI.
2. En caso de portarlo, crear una **assembly** `Simulation` en Unity.
3. Traducir los sistemas de IA y generación al Job System de Unity para paralelizar.
4. Asegurar que las reglas se puedan modificar fácilmente desde C#.

## 7. CLI (crate `server-cli`)
### Ficheros relevantes
- cli.rs
- main.rs
- settings.rs
- shutdown_coordinator.rs
- tui_runner.rs
- tuilog.rs

### Pasos recomendados
1. Reescribir las herramientas de línea de comandos usando `System.CommandLine` en C#.
2. Mantener la misma estructura de comandos para no romper scripts existentes.
3. Permitir la ejecución del servidor en modo headless desde la CLI de Unity.

## 8. Plugins
### Ficheros relevantes
- plugin/wit/veloren.wit

### Pasos recomendados
1. Diseñar un sistema de carga de plugins en C# compatible con `wasmtime` o con el sistema de paquetes de Unity.
2. Generar bindings a partir del archivo `veloren.wit` para exponer las APIs a los plugins.
3. Documentar cómo crear y compilar plugins externos.

## 9. Assets
Aunque no requieren conversión de código, se deben migrar los recursos a los formatos soportados por Unity y reorganizar las rutas.

---
Con este listado exhaustivo de ficheros y pasos, se puede iniciar la migración siguiendo buenas prácticas de C# y Unity. Cada módulo debe probarse de forma independiente antes de integrarse con el resto del proyecto.

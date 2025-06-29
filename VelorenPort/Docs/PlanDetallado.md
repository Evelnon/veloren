# Plan de Acción Detallado

_Actualizado: 2025-06-28_

Este documento describe paso a paso el port del código de Veloren a C# y Unity. Se listan los ficheros fuente principales de cada sistema y se sugieren tareas iniciales para la migración. La idea es usar este plan como referencia continua durante todo el proceso.

## Estructura general
- Cada sistema de Rust se convertirá en una **Assembly Definition** en Unity.
- Se respetará la jerarquía de carpetas actual para mantener la organización.
- Las pruebas unitarias se reescribirán usando el framework de pruebas de Unity.

Hasta ahora se han creado las assemblies `CoreEngine` y `Network`, con sus primeros archivos de código en C#, incluyendo direcciones, eventos e identificadores de red. El módulo de red ya integra la clase `Network` con participantes y canales simulados para comenzar a probar conexiones. Se añadieron además módulos auxiliares (`Metrics`, `Scheduler`, `Util` y un `Api` público) para preparar la funcionalidad completa. Recientemente se añadieron uniones de error (`NetworkError`, `NetworkConnectError`, `ParticipantError`, `StreamError`) y la clase `Stream` para cubrir la señalización básica de fallos y el flujo de mensajes. Se sumaron los tipos `InitProtocolError` y `ProtocolsError` para modelar errores durante el handshake.
Participant expone ahora `OpenStreamAsync` y `OpenedAsync` para manejar `Stream` y conserva un valor aproximado de ancho de banda.
Se suma la assembly `World` con estructuras de terreno iniciales para iniciar el port del crate `world`. El módulo `Noise` ahora utiliza las funciones de `VelorenPort.NativeMath` para obtener patrones deterministas en 3D y se instancia dentro de `WorldIndex`. También se añadió un generador `TerrainGenerator` con su clase `Chunk` para producir bloques de prueba. A esto se incorpora `WorldMap`, encargado de almacenar y crear chunks bajo demanda. El servidor ya usa este mapa para generar los chunks visibles alrededor de cada cliente. Como el paquete original no está disponible, se creó un stub interno en `CoreEngine` para exponer los tipos esenciales de `VelorenPort.NativeMath`. Este stub se ha extendido con funciones y constantes adicionales (incluido un ruido Simplex en 3D) para facilitar la compilación de los módulos portados y mantener la lógica de generación.


Actualmente el port incluye `Uid`, `CharacterId`, `RtSimEntity`, `Calendar`, `DayPeriod`, `Clock`, los recursos de tiempo, `Consts` y `ViewDistances`. Se sumaron `GameMode`, `PlayerEntity`, `PlayerPhysicsSettings`, `MapKind`, `BattleMode`, una versión con datos de `Actor`, `ServerConstants`, `Pos` y `EntitiesDiedLastTick`. Se agregó `Grid` para manejar datos bidimensionales, seguido de `Presence` con el enumerado `PresenceKind`. Sus variantes `LoadingCharacter` y `Character` guardan un `CharacterId`, aunque sólo la segunda lo entrega públicamente. `ViewDistance` maneja la visibilidad. Ahora se añadió `SpatialGrid` y el recurso `CachedSpatialGrid` para agilizar consultas de proximidad. También se implementaron `Path`, `AStar` y `Ray` para el cálculo de rutas y recorridos de voxels.
Se agregó `SlowJobPool` para ejecutar trabajos costosos en paralelo sin bloquear la simulación.
Además se añadió `Spiral` como utilería para generar coordenadas en espiral alrededor de un punto.
Se incluyeron también las estructuras `RegionSubscription`, `RepositionOnChunkLoad`
y `PresenceConstants` dentro de la assembly `Server` para gestionar las
suscripciones de regiones y el reposicionamiento de entidades cuando se cargan
chunks.
Adicionalmente se añadieron `TerrainConstants`, `RegionConstants` y la clase
`RegionUtils` con la función `InitializeRegionSubscription` para calcular las
regiones iniciales de cada cliente. Se creó también `RegionSubscriptionUpdater`
para actualizar esas suscripciones cuando los clientes se mueven o modifican su
distancia de visión. Cada `Client` ahora almacena su posición, `Presence` y
`RegionSubscription` al conectarse, y el `GameServer` actualiza la lista de
regiones visible en cada tick.

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
4. Utilizar `VelorenPort.NativeMath` para operaciones de vectores y matrices.
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

#### Características pendientes respecto a la versión de Rust

- Falta una capa avanzada de fiabilidad y priorización de streams.
- No se han portado todas las estructuras de `network-protocol`.
- El handshake se reduce a un intercambio de versión sin los pasos intermedios de la implementación original.
- Las métricas de red sólo cubren contadores básicos.
- El planificador carece de balanceo dinámico de tareas y reintentos inteligentes.
- Todavía no existe comunicación real con el servidor escrito en Rust.
- La interoperabilidad mediante FFI o Wasm está sin investigar.
- Las pruebas sólo contemplan el transporte local MPSC.

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

## Avance de migración
La siguiente tabla resume el progreso estimado de conversión por sistema. Estos valores se irán actualizando conforme se porten más clases.

| Sistema    | Porcentaje |
|------------|-----------:|
| CoreEngine | 100% |
| Network    | 100% |
| World      | 99% |
| Server     | 90% |
| Client     | 66% |
| Simulation | 100% |
| CLI        | 100% |
| Plugin     | 100% |

## Progreso detallado por archivo

Cada sistema enumera a continuación sus ficheros más relevantes junto con el
porcentaje aproximado de migración completado. Esta lista se actualizará
conforme se porten nuevas clases.

### CoreEngine

| Archivo | Porcentaje |
|---------|-----------:|
| AStar.cs | 100% |
| CachedSpatialGrid.cs | 100% |
| Calendar.cs | 100% |
| CharacterId.cs | 100% |
| Clock.cs | 100% |
| Consts.cs | 100% |
| DisconnectReason.cs | 100% |
| EntitiesDiedLastTick.cs | 100% |
| EventBus.cs | 100% |
| GameResources.cs | 100% |
| Grid.cs | 100% |
| Path.cs | 100% |
| Pos.cs | 100% |
| Presence.cs | 100% |
| Ray.cs | 100% |
| RtSimEntity.cs | 100% |
| SlowJobPool.cs | 100% |
| Spiral.cs | 100% |
| TerrainConstants.cs | 100% |
| Uid.cs | 100% |
| UnityEntitiesStub.cs | 100% |
| UnityMathematicsStub.cs | 100% |
| ViewDistances.cs | 100% |
| Actor.cs | 100% |
| LiquidKind.cs | 100% |
| Character.cs | 100% |
| Cmd.cs | 100% |
| Combat.cs | 100% |
| Depot.cs | 100% |
| Effect.cs | 100% |
| Explosion.cs | 100% |
| Generation.cs | 100% |
| Interaction.cs | 100% |
| Link.cs | 100% |
| Lod.cs | 100% |
| Lottery.cs | 100% |
| Mounting.cs | 100% |
| Npc.cs | 100% |
| Outcome.cs | 100% |
| Recipe.cs | 100% |
| Region.cs | 100% |
| Resources.cs | 100% |
| SharedServerConfig.cs | 100% |
| SkillsetBuilder.cs | 100% |
| Spot.cs | 100% |
| Store.cs | 100% |
| Tether.cs | 100% |
| Trade.cs | 100% |
| Typed.cs | 100% |
| Vol.cs | 100% |
| Weather.cs | 100% |
| ServerConstants.cs | 100% |
| SpatialGrid.cs | 100% |
| TimeResources.cs | 100% |
| UserdataDir.cs | 100% |

### Network

| Archivo | Porcentaje |
|---------|-----------:|
| Api.cs | 100% |
| Channel.cs | 100% |
| Metrics.cs | 100% |
| Participant.cs | 100% |
| Scheduler.cs | 100% |
| Util.cs | 100% |
| Message.cs | 100% |
| Network.cs | 100% |
| QuicClientConfig.cs | 100% |
| QuicServerConfig.cs | 100% |

### World

| Archivo | Porcentaje |
|---------|-----------:|
| Block.cs | 100% |
| BlockKind.cs | 100% |
| Chunk.cs | 100% |
| Noise.cs | 100% |
| TerrainGenerator.cs | 100% |
| WorldIndex.cs | 100% |
| WorldMap.cs | 100% |
| All.cs | 100% |
| Canvas.cs | 95% |
| Column.cs | 100% |
| Config.cs | 100% |
| Land.cs | 100% |
| Lib.cs | 85% |
| Pathfinding.cs | 100% |
| Sim2.cs | 100% |

### Server

| Archivo | Porcentaje |
|---------|-----------:|
| Automod.cs | 100% |
| CharacterCreator.cs | 100% |
| Chat.cs | 100% |
| ChunkGenerator.cs | 100% |
| ChunkSerialize.cs | 100% |
| Client.cs | 100% |
| Cmd.cs | 100% |
| ConnectionHandler.cs | 100% |
| DataDir.cs | 100% |
| Error.cs | 100% |
| PersistenceError.cs | 100% |
| Input.cs | 100% |
| Lib.cs | 100% |
| Locations.cs | 100% |
| Lod.cs | 100% |
| LoginProvider.cs | 100% |
| Metrics.cs | 100% |
| Pet.cs | 100% |
| Presence.cs | 100% |
| PresenceConstants.cs | 100% |
| RegionConstants.cs | 100% |
| RegionSubscription.cs | 100% |
| RegionSubscriptionUpdater.cs | 100% |
| RegionUtils.cs | 100% |
| RepositionOnChunkLoad.cs | 100% |
| StateExt.cs | 100% |
| TerrainPersistence.cs | 100% |
| TestWorld.cs | 100% |
| Wiring.cs | 100% |
| Events/EventManager.cs | 100% |
| Events/EventTypes.cs | 100% |
| Persistence/CharacterLoader.cs | 100% |
| Rtsim/RtSim.cs | 100% |
| Settings/Settings.cs | 100% |
| Sys/Metrics.cs | 100% |
| Weather/WeatherJob.cs | 100% |

### Client

| Archivo | Porcentaje |
|---------|-----------:|

### Simulation

| Archivo | Porcentaje |
|---------|-----------:|

### CLI

| Archivo | Porcentaje |
|---------|-----------:|
| Program.cs | 100% |
| Cli.cs | 100% |
| Main.cs | 100% |
| Settings.cs | 100% |
| ShutdownCoordinator.cs | 100% |
| TuiRunner.cs | 100% |
| TuiLog.cs | 100% |

### Plugin

| Archivo | Porcentaje |
|---------|-----------:|

---
Con este listado exhaustivo de ficheros y pasos, se puede iniciar la migración siguiendo buenas prácticas de C# y Unity. Cada módulo debe probarse de forma independiente antes de integrarse con el resto del proyecto.

# Server Port Missing Features


## Recent progress

- `pets::Sys` que controla las mascotas del jugador. ✅ Implementado de forma básica.
- `teleporter::Sys` y `waypoint::Sys` para los puntos de viaje rápido. ✅ Implementados de forma básica.
- Clientes y NPCs ahora mantienen y transmiten su orientación usando el nuevo helper de cuaterniones. ✅ Implementado de forma básica.

This document tracks the current gaps in the C# port of the Veloren server.
The list compares `VelorenPort/Server` with the Rust `server` crate and
highlights what remains to reach feature parity. The goal is to keep the
project compilable while replicating the original logic rather than
removing code.

- **ECS and dispatcher**: a minimal ECS with a sequential dispatcher now runs
  server systems each tick. More advanced scheduling and parallelism from the
  Rust version are still missing.
- **StateExt helpers**: the C# `StateExt` only supports basic NPC creation and
  lacks the advanced entity helpers found in `server/src/state_ext.rs`.
- **Incomplete systems in `Sys`**: several subsystems are still reduced
  versions of their Rust counterparts. Item management and complex object
  interaction remain missing. Pets now follow their owner and orient towards
  them but lack combat abilities. Teleporters include a short cooldown to
  prevent instant loops. Waypoints and wiring still lack advanced logic.
  Persistence and server info are implemented only in a basic form.
- **Item system**: there is no equivalent to `server/src/sys/item.rs`; item
  merging, deletion, and loot ownership are unimplemented.
- **Agent and combat behaviours**: AI routines for NPCs and full combat rules
  remain missing. The Rust `agent` crate (e.g., `server/agent/src`) has not been
  ported, so sophisticated NPC behaviour trees and combat decisions are
  absent. The current `NpcAiSystem` and `LootSystem` are placeholders that
  perform minimal actions.
- **Database and migrations**: only `CharacterLoader` provides JSON-based
  persistence. The database models and migrations from `server/src/persistence`
  are not yet ported. A task is required to implement SQLite-based persistence
  by running the SQL scripts in `VelorenPort/Server/Src/Migrations`.
- **Plugins and extensibility**: `PluginManager` loads assemblies from
  `plugins/` but a stable API and WebAssembly support are absent.
- **Real-time simulation**: the `Rtsim` module only records start time and does
  not perform heavy world calculations like the original.
- **Metrics and monitoring**: `PrometheusExporter` exposes a few counters,
  lacking the detailed metrics collected by the Rust server.
- **Administration CLI**: basic admin and banlist management commands are now
  available, though more helpers from `server-cli` remain to be ported.

- **Partial query server port**: the discovery server from `common/query_server`
  has been recreated. A `QueryClient` lives under `Src/QueryClient.cs` but lacks
  full version negotiation and integration tests. Query/discovery still does not
  completely match the Rust implementation.
 - **Incomplete events module**: only a few event classes like
   `AdditionalEvents.cs`, `ChatEvent.cs` and `CreateItemDropEvent.cs` are
   currently exposed. Many event types from `server/src/events/*` are still
   missing, and the event bus does not verify that events are consumed as the
   Rust version does.
- **Simplified login and administration**: banlist and whitelist loading work,
  but admin role assignment and CLI management have been reduced.
- **Missing weather and advanced real-time simulation**: the basic `rtsim` logic
  lacks the detailed weather system and time progression from the Rust server.
  The simple `WeatherSim` and `Rtsim` classes only provide placeholder behaviour
  and do not reproduce the detailed weather grid and simulation loops found in
  `server/src/weather` and `server/src/rtsim`.
- **Flat-file storage only**: database migrations are not implemented and all
  data currently resides in simple flat files.

## Recent progress

- Basic plugin loading works again after removing the Unity dependency.
- `RegionMap` and presence helpers were added to the CoreEngine, enabling
  simple region tracking and subscription updates.
- Pets now follow their owners with simple movement logic.
- Teleporters enforce a short cooldown between activations.

The migration will proceed incrementally, porting these features as needed
while ensuring the server continues to compile and run.

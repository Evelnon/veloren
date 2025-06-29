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
- **Incomplete systems in `Sys`**: several subsystems are still reduced
  versions of their Rust counterparts. Item management and complex object
  interaction remain missing. Pets now follow their owner and orient towards
  them but lack combat abilities. Teleporters include a short cooldown to
  prevent instant loops. Waypoints and wiring still lack advanced logic.
  Persistence and server info are implemented only in a basic form.
- **Agent and combat behaviours**: AI routines for NPCs and full combat rules
  remain missing. The current `NpcAiSystem` and `LootSystem` are placeholders
  that perform minimal actions.
- **Database and migrations**: migration scripts exist but the SQLite database
  layer from Rust is not yet ported. Character data is saved to disk only with
  simple serialization.
- **Plugins and extensibility**: `PluginManager` loads assemblies from
  `plugins/` but a stable API and WebAssembly support are absent.
- **Real-time simulation**: the `Rtsim` module only records start time and does
  not perform heavy world calculations like the original.
- **Metrics and monitoring**: `PrometheusExporter` exposes a few counters,
  lacking the detailed metrics collected by the Rust server.
- **Administration CLI**: advanced commands and configuration helpers from
  `server-cli` are not fully translated.
- **Server discovery**: there is no C# implementation of `common/query_server`,
  so clients must manually specify server addresses.
- **Event system**: the `server/src/events` module has not been ported; event
  types and the event bus logic remain missing.
- **Login provider**: authentication uses a simplified provider without
  banlists, whitelists or admin role checks.

## Recent progress

- Basic plugin loading works again after removing the Unity dependency.
- `RegionMap` and presence helpers were added to the CoreEngine, enabling
  simple region tracking and subscription updates.
- Pets now follow their owners with simple movement logic.
- Teleporters enforce a short cooldown between activations.

The migration will proceed incrementally, porting these features as needed
while ensuring the server continues to compile and run.

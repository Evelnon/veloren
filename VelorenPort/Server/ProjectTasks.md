# Server Project Tasks

This document tracks the main work items required to bring the C# server up to feature parity with the original Rust implementation. Each task references the primary C# files involved and the Rust source for guidance.


## 1. Advanced ECS scheduling and parallel dispatcher
- **C# files**: `Server/Src/Dispatcher.cs`, `Server/Src/GameServer.cs`
- **Rust reference**: `server/src/lib.rs` (dispatcher setup) and `server/src/events/mod.rs`
- **Summary**: Replace the sequential dispatcher with a scheduler that allows systems to run in parallel. Follow the Rust `SendDispatcher` logic using thread pools to maximize performance.

## 2. Full `StateExt` helpers
- **C# files**: `Server/Src/StateExt.cs`
- **Rust reference**: `server/src/state_ext.rs`
- **Summary**: Port all helper methods for entity initialization including players, objects and NPCs so that systems can spawn entities consistently.

## 3. Complete item system
- **C# files**: `Server/Src/Sys/ObjectSystem.cs`, `Server/Src/Sys/LootSystem.cs`
- **Rust reference**: `server/src/sys/item.rs`
- **Summary**: Implement item creation, merging and deletion rules. Support loot ownership and dropping behaviour as defined in the Rust system.

## 4. Port the `agent` crate for combat and AI
- **C# files**: `Server/Src/Sys/NpcAiSystem.cs`
- **Rust reference**: `server/agent/src/*`
- **Summary**: Recreate combat logic and behaviour trees used by NPCs. Replace the temporary `NpcAiSystem` with a full port of the Rust agent crate.

## 5. SQLite persistence with migrations
- **C# files**: `Server/Src/Persistence/CharacterLoader.cs`, `Server/Src/Migrations/*.sql`
- **Rust reference**: `server/src/persistence/*`
- **Summary**: Introduce a SQLite backend and apply the SQL scripts found under `Server/Src/Migrations` to manage schema updates. Replace the JSON loader with database models.

## 6. Stable plugin API and WebAssembly support
- **C# files**: `Plugin/` and `Server/Src/PluginManager.cs`
- **Rust reference**: `server/src/plugin.rs`
- **Summary**: Define a versioned API for external plugins. Investigate optional execution of plugins through WebAssembly to allow sandboxing similar to the Rust implementation.

## 7. Extend `Rtsim` with real-time simulation and weather
- **C# files**: `Server/Src/Rtsim/*`, `Server/Src/Weather/WeatherJob.cs`
- **Rust reference**: `server/src/rtsim/*`, `server/src/weather/*`
- **Summary**: Add missing world simulation rules and integrate weather logic so that time progression and environmental effects mirror the Rust server.

## 8. Detailed metrics mirroring the Rust server
- **C# files**: `Server/Src/Metrics.cs`, `Server/Src/Sys/Metrics.cs`
- **Rust reference**: `server/src/metrics.rs`
- **Summary**: Collect the same counters, gauges and histograms as the original server. Expose them via Prometheus exporter for monitoring.

## 9. Expanded administration CLI
- **C# files**: `CLI/Src/Cli.cs`, `Server/Src/Cmd.cs`
- **Rust reference**: `server-cli/src/*`
- **Summary**: Implement additional commands for ban management, statistics and server configuration following the Rust CLI tool.

## 10. Query server compatibility and tests
- **C# files**: `Server/Src/QueryClient.cs`, `Server/Src/Sys` 
- **Rust reference**: `common/query_server` and related tests
- **Summary**: Finish version negotiation and add integration tests to ensure the discovery server works with the Rust client and server.

## 11. Complete event type coverage
- **C# files**: `Server/Src/Events/*`
- **Rust reference**: `server/src/events/*`
- **Summary**: Port all remaining event classes so that `EventManager` exposes the full set used by server systems. Ensure debug checks match the Rust behaviour.

## 12. Enhanced moderation features
- **C# files**: `Server/Src/Automod.cs`
- **Rust reference**: `server/src/automod.rs`
- **Summary**: Add message queues and per-channel policies to replicate the advanced moderation logic including spam throttling and filters.

## 13. Persist `RegionMap` and dynamic LOD
- **C# files**: `CoreEngine/RegionMap.cs`, `Server/Src/Lod.cs`
- **Rust reference**: `server/src/terrain_persistence.rs`, `server/src/lod.rs`
- **Summary**: Store `RegionMap` on disk and implement dynamic level-of-detail adjustments for clients as they move through the world.

## 14. Layered client network protocol
- **C# files**: `Server/Src/Client.cs`, `Network/`
- **Rust reference**: `server/src/client.rs`
- **Summary**: Recreate the layered message protocol used by the Rust client to support reliable, lossy and ordered channels.

## 15. Message parsing modules
- **C# files**: `Server/Src/Sys/Msg/*`
- **Rust reference**: `server/src/sys/msg/*`
- **Summary**: Implement all message handlers including character screen and ping messages so the server can communicate with legacy clients.

## 16. Advanced group roles and loot sharing
- **C# files**: `Server/Src/InviteManager.cs`, `CoreEngine/Src/comp/GroupManager.cs`
- **Rust reference**: `server/src/events/invite.rs`, `server/src/sys/loot.rs`
- **Summary**: Implement group officer permissions, loot distribution modes and invite timeouts using these modules as starting points.

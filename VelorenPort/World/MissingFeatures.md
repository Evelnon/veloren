# World Port Missing Features

This document lists the major subsystems from the original Rust `world` crate that remain incomplete or missing in the C# port. The goal is to track what is needed so that the `World` module can compile and behave similarly to the Rust version.

## Implemented
- Basic chunk generation with `TerrainGenerator` and `WorldMap`.
- Simplified simulation via `WorldSim` including a humidity map and a very small erosion step.
- Initial civilisation generator creating sites with a few houses and NPCs.
- Minimal economy simulation with a `Caravan` example.
- Basic layer system with a `Scatter` layer for points of interest.
- Layer implementations for caves, rock strata, vegetation growth,
  wildlife spawning and basic resource deposits.
- Basic airship network with a simplified economy context.
- Pathfinding support with optional custom costs and map edge penalties.
- `Searcher` accepts passability and navigation grids for more accurate
  pathfinding.
- Site generation statistics recorded via `SitesGenMeta`.
- `CivGenerator` now reports statistics for created plots.
- Region maps can be saved and loaded through `WorldSim.SaveRegions` and
  `WorldSim.LoadRegions`.
- Humidity maps can be persisted via `WorldSim.SaveHumidity` and
  `WorldSim.LoadHumidity`.
- Natural resource maps (`Nature`) can be persisted with `Save` and `Load`.
- Basic sink filling prevents tiny inland basins after erosion.
- Weather grid tracks cloud cover and rain via `WeatherMap` with save/load helpers.
- River data for chunks can be persisted via `WorldSim.SaveRivers` and `WorldSim.LoadRivers`.

## Missing or Incomplete Features

### Civilisation and Sites (`civ`, `site`)
- Full economy stages, population events and trading logic.
- Detailed settlement generation (`site/gen`) with building templates and decorations.
- Expanded airship routes and the full economy system from `civ/econ`.
- Advanced settlement generation with `site/gen`, stats tracking via `site/genstat`,
  and the full set of building templates from `site/plot`.
- Site economy context beyond the basic implementation (`site/economy/context.rs` and `map_types.rs`).
- Tile and sprite handling for `site/tile` and `site/util`.
- Advanced logging of generation statistics beyond the basic `SitesGenMeta`.

### Layers (`layer`)
- Behavioural simulation for fauna and advanced resource handling remains
  unimplemented.

### Simulation (`sim`)
- Advanced humidity diffusion and river erosion models.
- Additional modules from `sim/util` and integration with `WorldSim`.

### Utilities
- `SeedExpan`, `StructureGenCache`, `WGrid` and `SmallCache` are available for world data management.

### Weather
- Storms with lightning are implemented, but regional climate simulation remains pending.

### Resources and Pathfinding
- Integration of chunk resources with AI and navigation data.
- More complex heuristics for pathfinding.
- Modules from `sim/map` and extended pathfinding helpers.

### Testing
- Unit tests cover diffusion, erosion, economy, layers, and storms, but large-scale integration tests covering full world creation are still missing.

### Build Status
- All remaining references to `UnityEngine` stubs have been removed. Server and plugin modules now log directly to the console. Further work is required to run the full Unity client but compilation no longer fails due to missing references.

The migration will continue incrementally, porting features as they become necessary for gameplay and ensuring the project compiles successfully.

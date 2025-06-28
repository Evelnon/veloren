# World Port Missing Features

This document tracks notable subsystems from the original Rust `world` crate
that are not yet implemented in the C# port. The list is not exhaustive
but highlights major areas that still require work.

- **Civilization generation**: modules under `civ` that create towns,
  points of interest and NPC placement are only stubbed.
- **Erosion and diffusion simulation**: advanced terrain shaping from
  `sim` is largely unported.
- **Layer and structure systems**: dynamic layers and structures used
  when generating chunks are simplified.
- **Region events**: entity tracking across regions lacks persistence
  and removal logic for inactive areas.
- **Chunk resources**: the rich resource system of the original world
  is represented by placeholders only.
- **Site economy**: full trading and economic simulation has not been
  migrated.
- **Pathfinding**: the A*-based search is present but lacks optimisations
  and integration with dynamic world data.
- **Testing**: unit tests cover only a fraction of the world pipeline.

The migration will continue incrementally, porting features as they
become necessary for gameplay.

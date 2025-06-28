# World Port Missing Features

This document tracks notable subsystems from the original Rust `world` crate
that are not yet implemented or only partially ported to C#. The list below
expands on all the components identificados hasta la fecha.

- **Generación de civilizaciones** (`civ`): sólo se crean sitios de forma
  aleatoria. Faltan la economía de civilizaciones, sus etapas de generación y
  la asignación de NPCs y eventos.
- **Capas dinámicas** (`layer`): cuevas, dispersión de objetos, arbustos,
  árboles y fauna no cuentan con implementaciones reales.
- **Simulación detallada** (`sim`): módulos de difusión, mapa de humedad,
  utilidades de localización y la erosión iterativa están ausentes.
- **Conjunto de sitios** (`site/gen` y `site/plot`): no se han portado los
  generadores de poblados ni la gran variedad de edificaciones y decoraciones.
- **Economía compleja** (`site/economy`): carecemos de mercados, oferta y
  demanda y rutas de caravanas.
- **Tiles y utilidades** (`site/tile`, `site/util`): sólo existen stubs para
  nombrar lugares y datos mínimos de paisajes.
- **Eventos de regiones** y políticas de descarte de entidades: la gestión de
  miembros de región se mantiene simple y sin persistencia histórica.
- **Recursos de chunk** (`ChunkResource` y asociadas) apenas se reflejan en la
  generación.
- **Pathfinding avanzado**: faltan heurísticas de coste dinámico y la
  integración con datos de navegación modificables.
- **Cobertura de pruebas**: muchas rutas de generación no están validadas por
  pruebas unitarias o de integración.

The migration will continue incrementally, porting features as they
become necessary for gameplay.

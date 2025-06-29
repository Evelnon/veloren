# World Port Missing Features

This document tracks notable subsystems from the original Rust `world` crate
that are not yet implemented or only partially ported to C#. The list below
expands on all the components identificados hasta la fecha.

- **Generación de civilizaciones** (`civ`): sólo se crean sitios de forma
  aleatoria. Faltan la economía de civilizaciones, sus etapas de generación y
  la asignación de NPCs y eventos.
**Capas dinámicas** (`layer`): `LayerManager` ahora puede esparcir puntos de interés simples mediante la capa `Scatter`, pero la generación de cuevas, arbustos, árboles y fauna continúa sin lógica real.
- **Simulación detallada** (`sim`): continúan faltando los módulos de difusión,
  el mapa de humedad y la erosión iterativa. Ya se ha incorporado una versión
  inicial de `sim/location` para nombrar lugares. Se añadieron utilidades
  básicas en `sim/util` como `MapEdgeFactor` y `cdf_irwin_hall`, junto al módulo
  `sim/way` y la clase `RandomPerm` para apoyar futuros caminos y
  aleatoriedad determinista.
  Se agregó un contenedor `HumidityMap` para registrar la humedad por chunk y
  se implementó una difusión básica integrada en `WorldSim.Tick`, aunque falta
  el modelo avanzado usado por el original.
- **Sistema de clima**: `WeatherSystem` ahora interpola transiciones y cuenta con zonas temporales, pero sigue sin el modelo físico completo de tormentas y rayos.
- **Conjunto de sitios** (`site/gen` y `site/plot`): no se han portado los
  generadores de poblados ni la gran variedad de edificaciones y decoraciones.

- **Economía compleja** (`site/economy`): carecemos de mercados, oferta y
  demanda y rutas de caravanas.
- Se añadieron `SiteKind` y `PoiKind` para clasificar sitios y puntos de
  interés, y los mensajes de mapa ahora exponen esta información.
- Se añadió `SiteKindMeta` junto con utilidades para convertir desde
  `SiteKind`, permitiendo a otros subsistemas identificar castillos,
  asentamientos y mazmorras.
- Los mensajes de mapa ahora incluyen una lista de posibles sitios
  iniciales para facilitar la selección de ubicaciones de inicio.
- Los sitios ahora almacenan su origen y exponen un cálculo aproximado
  de "radio" y límites para usos simples. Falta integrar estos datos en
  la generación avanzada y los sistemas de colisión.
- **Tiles y utilidades** (`site/tile`, `site/util`): se incorporaron las
  enumeraciones de dirección (`Dir`, `Dir3`) con utilidades de matrices de
  orientación y un módulo básico de gradientes. Se añadió una versión
  simplificada de `Tile` y `TileKind` para etiquetar terrenos. El contenedor
  `TileGrid` almacena dichas baldosas de forma dispersa y ahora dispone de
  utilidades básicas para ampliar áreas rectangulares u orgánicas (`GrowAabr`
  y `GrowOrganic`). Aún faltan los sprites y la lógica completa de generación
  de baldosas.
- **Spots** (`layer/spot`): se añadió la enumeración `Spot` y estructuras
  relacionadas para describir puntos de interés simples, pero aún no existe
  la generación detallada ni la carga de manifestos.
 - **Eventos de regiones** y políticas de descarte de entidades: las regiones ahora
   conservan un historial breve de eventos para depuración, pero carecen de
   persistencia a largo plazo.
 - **Recursos de chunk** (`ChunkResource` y asociadas) apenas se reflejan en la
   generación. Los puntos de aparición marcados con `Canvas.Spawn` se copian al
   suplemento del chunk pero aún no se utilizan.
- Se incorporó un contenedor `VolGrid3d` para manejar volúmenes 3D simples.

- **Pathfinding avanzado**: `Searcher` acepta delegados para coste y celdas válidas. Se añadió `NavGrid` para bloquear celdas específicas, pero sigue pendiente integrar datos de navegación editables.
- **Cobertura de pruebas**: muchas rutas de generación no están validadas por
  pruebas unitarias o de integración.

The migration will continue incrementally, porting features as they
become necessary for gameplay.

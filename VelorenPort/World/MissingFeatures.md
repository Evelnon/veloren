# World Port Missing Features

This document tracks notable subsystems from the original Rust `world` crate
that are not yet implemented or only partially ported to C#. The list below
expands on all the components identificados hasta la fecha.

- **Generación de civilizaciones** (`civ`): sólo se crean sitios de forma
  aleatoria. Faltan la economía de civilizaciones, sus etapas de generación y
  la asignación de NPCs y eventos.
- **Capas dinámicas** (`layer`): cuevas, dispersión de objetos, arbustos,
  árboles y fauna no cuentan con implementaciones reales.
- **Simulación detallada** (`sim`): continúan faltando los módulos de difusión,
  el mapa de humedad y la erosión iterativa. Ya se ha incorporado una versión
  inicial de `sim/location` para nombrar lugares. Se añadieron utilidades
  básicas en `sim/util` como `MapEdgeFactor` y `cdf_irwin_hall`, junto al módulo
  `sim/way` y las clases `RandomPerm` y `UnitChooser` para apoyar futuros
  caminos y aleatoriedad determinista.
- **Conjunto de sitios** (`site/gen` y `site/plot`): se añadió una estructura
  básica `Plot` con el tipo `PlotKind` y el generador ahora crea unas pocas
  casas por asentamiento. Aún faltan los generadores detallados y la variedad de
  edificaciones y decoraciones.
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
- **Eventos de regiones** y políticas de descarte de entidades: la gestión de
  miembros de región se mantiene simple. Ahora `Region` conserva un historial
  limitado de eventos recientes, pero aún no existe persistencia a largo plazo.
- **Recursos de chunk** (`ChunkResource` y asociadas) se empezaron a detectar al
  escribir bloques mediante `Canvas.SetBlock`, exponiendo una API mínima para
  marcar recursos recolectables. Ahora `ChunkSupplement` guarda las posiciones
  de estos bloques mediante `Canvas.WriteSupplementData`, aunque sigue faltando
  su uso real en la generación de terreno.
- **Pathfinding avanzado**: faltan heurísticas de coste dinámico y la
  integración con datos de navegación modificables.
- **Cobertura de pruebas**: muchas rutas de generación no están validadas por
  pruebas unitarias o de integración.

The migration will continue incrementally, porting features as they
become necessary for gameplay.

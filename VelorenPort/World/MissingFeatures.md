# World Port Missing Features

This document tracks notable subsystems from the original Rust `world` crate
that are not yet implemented or only partially ported to C#. The list below
expands on all the components identificados hasta la fecha.

- **Generación de civilizaciones** (`civ`): sólo se crean sitios de forma
  aleatoria. Ahora cada casa genera un NPC básico que se almacena en el índice
  global. Faltan la economía de civilizaciones, sus etapas de generación y
  los eventos de población avanzados.
- **Capas dinámicas** (`layer`): ahora existen implementaciones básicas para la
  generación de cuevas, dispersión de objetos, depósitos de recursos y
  vegetación (arbustos y árboles). Se añadió una capa de fauna que registra
  posiciones de criaturas simples, pero todavía falta la simulación de
  comportamientos y especies avanzadas.
- **Simulación detallada** (`sim`): se añadió un mapa de humedad con difusión
  básica y un módulo de erosión simplificado que rebaja la altitud de los chunks
  cada tick. El mapa de humedad puede guardarse y cargarse en JSON, pero todavía
  falta el modelo completo de erosión fluvial y la difusión avanzada. Ya se ha
  incorporado una versión inicial de `sim/location` para nombrar lugares. Se
  añadieron utilidades básicas en `sim/util` como `MapEdgeFactor` y
  `cdf_irwin_hall`, junto al módulo `sim/way` y la clase `RandomPerm` para
  apoyar futuros caminos y aleatoriedad determinista.
- **Conjunto de sitios** (`site/gen` y `site/plot`): no se han portado los
  generadores de poblados ni la gran variedad de edificaciones y decoraciones.
- **Conjunto de sitios** (`site/gen` y `site/plot`): se añadió una estructura
  básica `Plot` con el tipo `PlotKind` y el generador ahora crea unas pocas
  casas por asentamiento. Se incluyó un trazado mínimo que ubica una plaza
  central, caminos en cruz y campos alrededor de las casas. Ahora cada casa
  se conecta con el centro mediante un pequeño camino. Aún faltan los
  generadores detallados y la variedad de edificaciones y decoraciones.
- **Economía compleja** (`site/economy`): ahora existe una clase `Caravan` que
  recorre rutas simples entre sitios transportando alimentos. Se añadió un mercado básico por sitio que ajusta precios en función de la demanda y el stock disponible. Siguen faltando los mercados avanzados, la gestión completa de oferta y demanda y las rutas de caravanas con IA sofisticada.
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
  orientación y un módulo básico de gradientes, pero siguen faltando sprites y
  la lógica de baldosas. Se añadió una versión simplificada de `Tile` y
  `TileKind` para etiquetar terrenos. El contenedor `TileGrid` almacena dichas
  baldosas de forma dispersa y ahora dispone de utilidades básicas para ampliar
  áreas rectangulares u orgánicas (`GrowAabr` y `GrowOrganic`). Aún faltan los
  sprites y la lógica completa de generación de baldosas.
- **Spots** (`layer/spot`): se añadió la enumeración `Spot` y un generador
  preliminar que coloca un punto de interés en función del ruido. Aún falta la
  carga de manifestos y la variedad completa de estructuras.
- **Eventos de regiones** y políticas de descarte de entidades: `Region` ahora
  conserva un historial limitado de eventos y `RegionMap` puede guardarse y
  cargarse en disco mediante JSON. Aún falta una base de datos real para la
  persistencia a largo plazo.
- **Recursos de chunk** (`ChunkResource` y asociadas) apenas se reflejan en la
  generación. `Region` conserva un historial limitado de eventos recientes,
  pero todavía no existe persistencia a largo plazo fuera del archivo JSON.
- **Recursos de chunk** (`ChunkResource` y asociadas) se empezaron a detectar al
  escribir bloques mediante `Canvas.SetBlock`, exponiendo una API mínima para
  marcar recursos recolectables. Ahora `ChunkSupplement` guarda las posiciones
  de estos bloques mediante `Canvas.WriteSupplementData`, junto con los puntos
  de aparición de fauna. Se añadió una capa de recursos que inserta vetas
  simples de minerales durante la generación de terreno. Ahora la generación
  produce un `ChunkSupplement` con dichos recursos y spawns de fauna básicos. El
  `WorldMap` conserva estos suplementos para que otras capas puedan
  consultarlos, pero aún falta integrarlo con sistemas de IA y generación
  avanzada.
- **Pathfinding avanzado**: el buscador acepta ahora una función opcional de
  coste adicional, penaliza los bordes del mapa con `MapEdgeFactor` y reduce el
  coste al recorrer caminos designados. Siguen pendientes heurísticas más
  complejas y la integración con datos de navegación modificables.
- **Cobertura de pruebas**: muchas rutas de generación no están validadas por
  pruebas unitarias o de integración.

The migration will continue incrementally, porting features as they
become necessary for gameplay.

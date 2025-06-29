# World Port Missing Features

This document tracks notable subsystems from the original Rust `world` crate
that are not yet implemented or only partially ported to C#. The list below
expands on all the components identificados hasta la fecha.

- **Generación de civilizaciones** (`civ`): ahora se crean asentamientos
  simples con una plaza central, casas y caminos gracias al nuevo
  `SiteGenerator`. Todavía faltan la economía de civilizaciones, sus
  etapas de generación completas y la asignación de NPCs y eventos.
- **Capas dinámicas** (`layer`): ahora existen versiones básicas de cuevas,
  dispersión de rocas, arbustos, árboles, vetas de mineral y puntos de
  aparición de fauna. La generación de chunks aplica estas capas y
  almacena los bloques de recursos detectados, pero aún distan de la
  complejidad del proyecto en Rust.
- **Simulación detallada** (`sim`): se añadió difusión iterativa de humedad
  (`HumidityMap.RunDiffusion`) y una forma básica de erosión múltiple
  (`Erosion.IterativeApply`). Aún faltan el modelo de erosión complejo y otras
  utilidades. Ya se ha incorporado una versión
  inicial de `sim/location` para nombrar lugares. Se añadieron utilidades
  básicas en `sim/util` como `MapEdgeFactor` y `cdf_irwin_hall`, junto al módulo
  `sim/way` y la clase `RandomPerm` para apoyar futuros caminos y
  aleatoriedad determinista.
- **Conjunto de sitios** (`site/gen` y `site/plot`): existe un generador
  básico de asentamientos que crea plazas, casas, caminos y ahora parcelas de
  cultivo sencillas. Aún falta la variedad completa de edificaciones y
  decoraciones.
  - **Economía compleja** (`site/economy`): la clase `Market` ahora ajusta sus
    precios según la oferta disponible y se actualiza cada tick. Las rutas de
    caravanas transportan bienes entre asentamientos, pero siguen pendientes las
    tablas completas de recursos y comportamientos avanzados.
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
  la lógica de baldosas.
- **Spots** (`layer/spot`): se añadió la enumeración `Spot` y estructuras
  relacionadas. El generador ahora puede cargar manifestos JSON para
  determinar qué tipos aparecen y con qué frecuencia. Aún falta la
  generación detallada de cada estructura.
- **Eventos de regiones** y políticas de descarte de entidades: ahora cada región
  registra un historial breve de entradas y salidas mediante `RegionMap`, pero
  sigue faltando persistencia a largo plazo y políticas avanzadas.
- **Recursos de chunk** (`ChunkResource` y asociadas): las vetas de mineral
  generan bloques especiales que se registran en `ChunkSupplement`, pero
  su explotacion todavia no afecta a la simulacion.
- **Pathfinding avanzado**: ahora `SearchCfg` admite una función de coste
  dinámico para ajustar caminos en tiempo de ejecución. Aún falta la
  integración completa con datos de navegación modificables.
- **Cobertura de pruebas**: muchas rutas de generación no están validadas por
  pruebas unitarias o de integración.

The migration will continue incrementally, porting features as they
become necessary for gameplay.

# World Port Missing Features

This document tracks notable subsystems from the original Rust `world` crate
that are not yet implemented or only partially ported to C#. The list below
expands on all the components identificados hasta la fecha.

- **Generación de civilizaciones** (`civ`): sólo se crean sitios de forma
  aleatoria. Faltan la economía de civilizaciones, sus etapas de generación y
  la asignación de NPCs y eventos.
**Capas dinámicas** (`layer`): se añadió un esqueleto `LayerManager` con tipos de capa básicos, pero la aplicación de cuevas, dispersión de objetos, arbustos, árboles y fauna aún carece de lógica real.
- **Simulación detallada** (`sim`): continúan faltando los módulos de difusión,
  el mapa de humedad y la erosión iterativa. Ya se ha incorporado una versión
  inicial de `sim/location` para nombrar lugares. Se añadieron utilidades
  básicas en `sim/util` como `MapEdgeFactor` y `cdf_irwin_hall`, junto al módulo
  `sim/way` y la clase `RandomPerm` para apoyar futuros caminos y
  aleatoriedad determinista.
  Se agregó un contenedor `HumidityMap` para registrar la humedad por chunk y
  se implementó una función de difusión básica, aunque falta el modelo
  avanzado usado por el original.
- **Conjunto de sitios** (`site/gen` y `site/plot`): no se han portado los
  generadores de poblados ni la gran variedad de edificaciones y decoraciones.
- **Economía compleja** (`site/economy`): carecemos de mercados, oferta y
  demanda y rutas de caravanas.
- **Tiles y utilidades** (`site/tile`, `site/util`): se incorporaron las
  enumeraciones de dirección (`Dir`, `Dir3`) con utilidades de matrices de
  orientación y un módulo básico de gradientes, pero siguen faltando sprites y
  la lógica de baldosas.
- **Eventos de regiones** y políticas de descarte de entidades: la gestión de
  miembros de región se mantiene simple y sin persistencia histórica.
- **Recursos de chunk** (`ChunkResource` y asociadas) apenas se reflejan en la
  generación.
- Se incorporó un contenedor `VolGrid3d` para manejar volúmenes 3D simples.
- **Pathfinding avanzado**: faltan heurísticas de coste dinámico y la
  integración con datos de navegación modificables.
- **Cobertura de pruebas**: muchas rutas de generación no están validadas por
  pruebas unitarias o de integración.

The migration will continue incrementally, porting features as they
become necessary for gameplay.

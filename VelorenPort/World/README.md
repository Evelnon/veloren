# World

Incluye la lógica de generación de mundo procedimental y estructuras de terreno (crate `world`).

**Viabilidad**: Media. El algoritmo utiliza intensivamente características de Rust y puede depender de crates específicos. Se puede portar la lógica a C#, pero puede requerir optimización para funcionar con Unity.

**Notas**:
- Analizar dependencias externas como `image`, `noise` o `simd` para buscar equivalentes en C#.
- Valorar si parte de la lógica puede migrarse a librerías de Unity (por ejemplo `VelorenPort.NativeMath`).

Se añadió la assembly `World` con clases iniciales `Block`, `BlockKind` y `WorldIndex` como base para comenzar a portar la generación de terreno. `WorldIndex` integra un módulo `Noise` respaldado por `VelorenPort.NativeMath` que genera valores deterministas y servirá para las rutinas de creación de mapas. Ahora también cuenta con `TerrainGenerator` y `Chunk` para experimentar con la construcción de datos de terreno. A ello se suma `WorldMap`, un contenedor de chunks que los genera bajo demanda. Este mapa es empleado por el `GameServer` para preparar el terreno cercano a cada jugador. El enumerado `BlockKind` refleja ahora todos los valores originales de Rust para mantener la compatibilidad con la lógica de generación. `Block` permite convertir sus datos a un entero de 32 bits o reconstruirse desde dicho valor, igual que en el proyecto fuente.

## Estado actual

Aunque existe una base funcional para generar terreno y manejar regiones, buena
parte del crate `world` original aún no se ha migrado. Entre las ausencias más
relevantes destacan:

- Sistemas de **capas** (`layer`) para cuevas, vegetación, fauna y otros
  elementos dinámicos.
- Módulos de **simulación** avanzados (`sim/diffusion`, `sim/util`, etc.) y el
  modelo completo de erosión de ríos. El submódulo `sim/location` ya cuenta con
  una versión básica para generar nombres de lugares. Se añadieron funciones de
  `sim/util` como `MapEdgeFactor` y `cdf_irwin_hall`, junto a nuevas utilidades
  de caminos (`sim/way`) y el generador determinista `RandomPerm`. Además se
  incorporó `site/util` con enumeraciones de dirección, matrices de orientación
  y un sistema de gradientes para futuros módulos.
- **Generación de civilizaciones** y poblados complejos definida en `civ` y
  `site/gen`.
- El catálogo de **edificaciones** de `site/plot` y la lógica de baldosas de
  `site/tile`.
- Economía detallada y rutas de comercio (`site/economy`) más allá de un
  esqueleto mínimo.
- Gestión extensiva de eventos y recursos de chunk.
- Pruebas unitarias e integración de muchas de estas características.

El archivo [MissingFeatures.md](MissingFeatures.md) contiene una lista más
detallada de tareas pendientes.



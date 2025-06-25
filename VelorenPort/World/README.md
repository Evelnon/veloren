# World

Incluye la lógica de generación de mundo procedimental y estructuras de terreno (crate `world`).

**Viabilidad**: Media. El algoritmo utiliza intensivamente características de Rust y puede depender de crates específicos. Se puede portar la lógica a C#, pero puede requerir optimización para funcionar con Unity.

**Notas**:
- Analizar dependencias externas como `image`, `noise` o `simd` para buscar equivalentes en C#.
- Valorar si parte de la lógica puede migrarse a librerías de Unity (por ejemplo `Unity.Mathematics`).

 Se añadió la assembly `World` con clases iniciales `Block`, `BlockKind` y `WorldIndex` como base para comenzar a portar la generación de terreno. `WorldIndex` integra un módulo `Noise` respaldado por `Unity.Mathematics` que genera valores deterministas y servirá para las rutinas de creación de mapas. Ahora también cuenta con `TerrainGenerator` y `Chunk` para experimentar con la construcción de datos de terreno. A ello se suma `WorldMap`, un contenedor de chunks que los genera bajo demanda. Este mapa es empleado por el `GameServer` para preparar el terreno cercano a cada jugador. El enumerado `BlockKind` refleja ahora todos los valores originales de Rust para mantener la compatibilidad con la lógica de generación. `Block` permite convertir sus datos a un entero de 32 bits o reconstruirse desde dicho valor, igual que en el proyecto fuente.



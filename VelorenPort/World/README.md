# World

Incluye la lógica de generación de mundo procedimental y estructuras de terreno (crate `world`).

**Viabilidad**: Media. El algoritmo utiliza intensivamente características de Rust y puede depender de crates específicos. Se puede portar la lógica a C#, pero puede requerir optimización para funcionar con Unity.

**Notas**:
- Analizar dependencias externas como `image`, `noise` o `simd` para buscar equivalentes en C#.
- Valorar si parte de la lógica puede migrarse a librerías de Unity (por ejemplo `Unity.Mathematics`).

Se añadió la assembly `World` con clases iniciales `Block`, `BlockKind` y `WorldIndex` como base para comenzar a portar la generación de terreno. Estas estructuras son simplificadas respecto al código de Rust pero permiten experimentar con la lógica en Unity.

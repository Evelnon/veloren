# CoreEngine Port Missing Features

Este archivo detalla los módulos del crate `common` que todavía no tienen equivalente en la versión C#.

- **ECS y sistema de estados**: falta portar `ecs`, `state` y `systems`, base de la arquitectura en Rust.
- **`states` de combate y movimiento**: se añadieron muchas variantes de
  `CharacterState` junto con `AttackSource`, `AttackFilters` y la estructura
  `ForcedMovement`, pero aún faltan lógicas avanzadas y transiciones complejas.
- **Utilidades de terreno y volúmenes** (`terrain`, `volumes`): sólo se trasladaron constantes, sin el manejo completo de biomas y bloques.
- **Componentes (`comp`)**: la mayoría de componentes no se han implementado; existen únicamente algunos stubs (`BuffKind`, `Player`, etc.).
- Se añadieron `Alignment`, `Group` y `CharacterItem` para reflejar hostilidad y
  listas de personajes, y ahora existe un `GroupManager` básico para crear y
  abandonar grupos, aunque faltan notificaciones avanzadas y soporte de
  mascotas.
- **Funciones de `util`**: quedan por migrar varios helpers de proyecciones, compresión y colores avanzados.
- **Submódulos adicionales**: se añadió `SlowJobPool` y se portaron contenedores
  `Store` y `Trade` con funcionalidades básicas. El módulo `figure` ahora
  incluye `MatCell` y `DynaUnionizer`, aunque faltan herramientas avanzadas y
  el submódulo `bin` continúa pendiente.
- **Sistema de clima**: la implementación actual de `weather` es mínima comparada con la lógica de Rust.
- Se incluyó un contenedor `VolGrid3d` para volúmenes 3D básico.
- **Cobertura de pruebas**: aún no hay pruebas unitarias equivalentes para buena parte de estos módulos.

Se continuará importando funcionalidad a medida que sea necesaria para el resto del proyecto.

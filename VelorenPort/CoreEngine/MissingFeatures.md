# CoreEngine Port Missing Features

Este archivo detalla los módulos del crate `common` que todavía no tienen equivalente en la versión C#.

- **ECS y sistema de estados**: falta portar `ecs`, `state` y `systems`, base de la arquitectura en Rust.
- **`states` de combate y movimiento**: el submódulo completo sigue ausente.
- **Utilidades de terreno y volúmenes** (`terrain`, `volumes`): sólo se trasladaron constantes, sin el manejo completo de biomas y bloques.
- **Componentes (`comp`)**: la mayoría de componentes no se han implementado; existen únicamente algunos stubs (`BuffKind`, `Player`, etc.).
- **Funciones de `util`**: quedan por migrar varios helpers de proyecciones, compresión y colores avanzados.
- **Submódulos adicionales**: `slowjob`, `store`, `trade`, `figure` y `bin` permanecen sin portar.
- **Sistema de clima**: la implementación actual de `weather` es mínima comparada con la lógica de Rust.
- **Cobertura de pruebas**: aún no hay pruebas unitarias equivalentes para buena parte de estos módulos.

Se continuará importando funcionalidad a medida que sea necesaria para el resto del proyecto.

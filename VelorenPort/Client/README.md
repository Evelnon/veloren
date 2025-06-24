# Client

Implementación de la interfaz y lógica de cliente (`client` y `voxygen`).

**Viabilidad**: Alta. Unity reemplazará muchos subsistemas gráficos, por lo que el cliente actual en Rust (basado en wgpu y egui) será recreado con las herramientas de Unity. Gran parte de la lógica de juego se puede portar a scripts de C#.

**Notas**:
- Estudiar si ciertas utilidades (i18n, animaciones, HUD) pueden integrarse con paquetes de Unity.
- Los recursos cargados se adaptarán al formato compatible con Unity.

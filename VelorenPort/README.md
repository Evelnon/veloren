# VelorenPort

Este directorio contiene la planeación inicial para portar el proyecto [Veloren](https://gitlab.com/veloren/veloren) escrito en Rust a C# para integrarlo con el motor Unity.

La estructura se organiza por sistemas principales y cada subcarpeta describe brevemente su función y el grado de dificultad estimada para la conversión a C#.

Los sistemas identificados son:

- **CoreEngine**: componentes compartidos, ECS y utilidades generales.
- **Network**: manejo de protocolos y mensajes.
- **World**: generación y persistencia del mundo.
- **Server**: lógica de servidor y control de partidas.
- **Client**: interfaz de usuario y representación visual (Voxygen).
- **Simulation**: subsistema `rtsim` de simulación a gran escala.
- **Plugin**: soporte de plugins mediante WebAssembly.
- **CLI**: herramientas de servidor y línea de comandos.
- **Assets**: recursos de arte y datos.

Para detalles de cada archivo y pasos a seguir consulte `Docs/PlanDetallado.md`.
En la carpeta `Docs` se irán añadiendo guías y notas de migración.


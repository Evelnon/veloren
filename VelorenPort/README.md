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

El sistema `Network` cuenta ahora con una clase `Network` minimal y tipos para `Participant` y `Channel` que
sirven como esqueleto para futuras implementaciones de sockets y gestión de participantes.
Además se añadieron los enums de error (`NetworkError`, `NetworkConnectError`, `ParticipantError`, `StreamError`)
y la clase `Stream` para mantener la API similar a la del crate original.

Se creó además la assembly `World` con definiciones básicas de terreno (`Block`, `BlockKind`) e índices (`WorldIndex`) para comenzar el traslado de la lógica de generación procedimental.

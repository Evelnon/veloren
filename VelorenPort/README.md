# VelorenPort

Estructura base para iniciar el port de **Veloren** a C# usando Unity.

Este repositorio agrupa los sistemas originales del proyecto en carpetas para planificar su conversión y facilitar la organización inicial del código.

## Carpetas

- `Core/` – Lógica principal del juego y dependencias comunes.
  - `Common/` – Sistemas de base, ECS y utilidades compartidas.
  - `Network/` – Protocolo de red y serialización de mensajes.
  - `World/` – Generación y manejo del mundo y economía.
  - `Server/` – Lógica del servidor dedicado.
  - `Client/` – Cliente de red y utilidades de conexión.
  - `Voxygen/` – Cliente gráfico y motor de renderizado escrito en Rust.
- `Tools/` – Herramientas complementarias.
  - `ServerCli/` – Ejecución del servidor mediante línea de comandos y scripts.
  - `Rtsim/` – Simulador en tiempo real para pruebas.
  - `Plugin/` – Sistema de extensiones y pruebas con WebAssembly.
- `Assets/` – Recursos de arte, sonidos y datos.

Cada carpeta debería contener el código equivalente en C# con la estructura necesaria para Unity.

Para más información sobre cada sistema consulte el archivo `docs/Plan.md`.

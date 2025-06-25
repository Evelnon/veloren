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

Se creó además la assembly `World` con definiciones básicas de terreno (`Block`, `BlockKind`) e índices (`WorldIndex`) para comenzar el traslado de la lógica de generación procedimental. El índice incluye ahora un generador `Noise` sencillo que permitirá replicar comportamientos aleatorios de forma determinista durante las pruebas.
Por último, en `CoreEngine` se implementaron los módulos `Calendar`, `DayPeriod` y `Clock` que permiten gestionar eventos estacionales, las fases del día y un reloj básico para controlar el tiempo de juego. Se añadieron también las estructuras de recursos de tiempo (`TimeOfDay`, `Time`, `DeltaTime`, etc.) para empezar a replicar la administración del ciclo día/noche y el avance de ticks. Además se añadieron las constantes de `Consts` y `ViewDistances` para limitar interacciones y distancias. Tras revisar el código se agregaron métodos faltantes como `SetTargetDt` y la búsqueda de entidades por `Actor` en `IdMaps`.
Tambien se portaron recursos como `GameMode`, `PlayerEntity` y `PlayerPhysicsSettings`, junto a los enums `MapKind` y `BattleMode`. El tipo `Actor` ahora almacena el identificador correspondiente.
Se añadieron igualmente `ServerConstants`, la estructura `Pos`, el recurso `EntitiesDiedLastTick` y la enumeración `DisconnectReason` para ampliar las utilidades disponibles.
Se añadió un contenedor generico `Grid` para manejar mapas bidimensionales de forma sencilla.
Posteriormente se integró el componente `Presence` para controlar la sincronización de cada entidad mediante un enumerado `PresenceKind` y un `CharacterId` opcional. También se añadieron `SpatialGrid` y `CachedSpatialGrid` para reutilizar consultas espaciales. Finalmente se incorporaron `Path`, el algoritmo `AStar` y la utilidad `Ray` como base para futuros sistemas de navegación.
Adicionalmente se creó `SlowJobPool` para procesar en paralelo tareas intensivas sin detener la lógica principal.
Se agregó `Spiral` como utilidad para iterar posiciones en espiral, útil en sistemas de generación y exploración.
Se añadió también la assembly `Server` con la clase `GameServer`, un esqueleto
para orquestar conexiones y avanzar los ticks de juego de forma asíncrona.
Además se crearon `Client` y `ConnectionHandler` para registrar las conexiones entrantes y administrar la lista de clientes.

## Proyectos de C#
Se añadieron archivos `*.csproj` en cada carpeta de sistema (CoreEngine, Network, World, Server, Client, CLI, Plugin y Simulation). Todos ellos se agrupan en la solución `VelorenPort.sln` para poder abrir y compilar el proyecto desde herramientas de .NET o Visual Studio.

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

El sistema `Network` cuenta con una clase `Network` y tipos para `Participant` y `Channel` que
permiten iniciar conexiones básicas y administrar participantes conectados.
Además se añadieron uniones discriminadas de error (`NetworkError`, `NetworkConnectError`,
`ParticipantError`, `StreamError`) junto con tipos auxiliares (`InitProtocolError`,
`ProtocolsError`, `ProtocolError`) para conservar la información de fallos.
La clase `Stream` mantiene la API similar a la del crate original.
`Participant` incluye funciones para abrir y recibir `Stream` de forma asíncrona y lleva un registro simple del ancho de banda.

### Limitaciones actuales del módulo `Network`

- Las colas de prioridad y el sistema avanzado de fiabilidad aún no se han implementado.
- Faltan muchas estructuras del subcrate `network-protocol`.
- La conexión con el servidor original en Rust está reducida a registros de prueba.
- El planificador carece de balanceo dinámico y las métricas solo abarcan algunos contadores básicos.
- Las pruebas unitarias se limitan al transporte local MPSC.

Se creó además la assembly `World` con definiciones básicas de terreno (`Block`, `BlockKind`) e índices (`WorldIndex`) para comenzar el traslado de la lógica de generación procedimental. Dicho índice expone un generador `Noise` basado en `Unity.Mathematics` que produce valores deterministas en 3D. También se implementó `TerrainGenerator` y la clase `Chunk` como primer paso para construir el mundo. Posteriormente se añadió `WorldMap` para almacenar los chunks generados en memoria. El `GameServer` utiliza este mapa para poblar el terreno alrededor de cada cliente. El enum `BlockKind` se ha completado con todos los valores del proyecto original.
Para suplir la falta del paquete `Unity.Mathematics` en este entorno, `CoreEngine` incluye un stub con funciones básicas y la constante `math.PI`. Dicho stub ahora implementa un algoritmo de ruido Simplex en 3D, por lo que los generadores de terreno producen patrones más cercanos al proyecto original.

Por último, en `CoreEngine` se implementaron los módulos `Calendar`, `DayPeriod` y `Clock` que permiten gestionar eventos estacionales, las fases del día y un reloj básico para controlar el tiempo de juego. Se añadieron también las estructuras de recursos de tiempo (`TimeOfDay`, `Time`, `DeltaTime`, etc.) para empezar a replicar la administración del ciclo día/noche y el avance de ticks. Además se añadieron las constantes de `Consts` y `ViewDistances` para limitar interacciones y distancias. Tras revisar el código se agregaron métodos faltantes como `SetTargetDt` y la búsqueda de entidades por `Actor` en `IdMaps`.
Tambien se portaron recursos como `GameMode`, `PlayerEntity` y `PlayerPhysicsSettings`, junto a los enums `MapKind` y `BattleMode`. El tipo `Actor` ahora almacena el identificador correspondiente.
Se añadieron igualmente `ServerConstants`, la estructura `Pos`, el recurso `EntitiesDiedLastTick` y la enumeración `DisconnectReason` para ampliar las utilidades disponibles.
Se añadió un contenedor generico `Grid` para manejar mapas bidimensionales de forma sencilla.
 Posteriormente se integró el componente `Presence` para controlar la sincronización de cada entidad mediante un enumerado `PresenceKind`. Las variantes `LoadingCharacter` y `Character` guardan un `CharacterId`, pero solamente la segunda lo expone una vez cargado el personaje. También se añadieron `SpatialGrid` y `CachedSpatialGrid` para reutilizar consultas espaciales. Finalmente se incorporaron `Path`, el algoritmo `AStar` y la utilidad `Ray` como base para futuros sistemas de navegación.
Adicionalmente se creó `SlowJobPool` para procesar en paralelo tareas intensivas sin detener la lógica principal.
Se agregó `Spiral` como utilidad para iterar posiciones en espiral, útil en sistemas de generación y exploración.
Se añadió también la assembly `Server` con la clase `GameServer`, encargada de
orquestar conexiones y avanzar los ticks de juego de forma asíncrona.
Además se crearon `Client` y `ConnectionHandler` para registrar las conexiones entrantes y administrar la lista de clientes.

## Proyectos de C#
Se añadieron archivos `*.csproj` en cada carpeta de sistema (CoreEngine, Network, World, Server, Client, CLI, Plugin y Simulation). Todos ellos se agrupan en la solución `VelorenPort.sln` para poder abrir y compilar el proyecto desde herramientas de .NET o Visual Studio.

## Limitaciones actuales y módulos pendientes

A pesar de contar con varios archivos clave, el port a C# todavía está lejos de cubrir todo el crate `common` de Rust. Los siguientes subsistemas no se han migrado o sólo existen como stubs muy básicos:

- **`states`**: faltan todos los archivos de lógica de combate y movimiento.
- **`terrain` y `volumes`**: sólo se incluyó `TerrainConstants`; el manejo completo de biomas, bloques y volúmenes sigue pendiente.
- **`util`**: aparte de `MathUtil` no se han migrado las utilidades de color, proyección ni compresión.
- **Componentes (`comp`)**: en Rust existen decenas de componentes (inventario, habilidades, físicas, etc.); aquí solo se implementaron `BuffKind`, `Chat`, `Group`, `Player` y varios stubs.
- **`weather`**: el port actual es un stub sin la lógica de interpolación ni compresión presentes en Rust.
- **Otros módulos**: no hay traslados de `slowjob`, `store`, `trade`, `figure`, ni del submódulo `bin`.

Estas ausencias muestran que la funcionalidad en C# es aún limitada y queda trabajo considerable para alcanzar la paridad con el código original.

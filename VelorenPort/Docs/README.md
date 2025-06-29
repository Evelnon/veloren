# Documentación

En esta carpeta se recopilan guías y decisiones de diseño tomadas durante el proceso de port.

Archivos previstos:
- `PlanDetallado.md`: Detalle completo de ficheros a migrar y pasos específicos.
- `Plan.md`: Plan de acción y tareas iniciales.
  La antigua guía `Interoperabilidad.md` fue eliminada porque el proyecto ya no depende de código Rust.
Tambien se documentará el progreso de cada sistema portado. El primero en migrarse es `CoreEngine`, con sus definiciones base en `../CoreEngine/Src`.

El sistema `Network` ya cuenta con una assembly y tipos básicos de dirección en `../Network/Src`.
Ahora se incluyen también estructuras para mensajes (`Message`), parámetros de stream (`StreamParams`) y flags (`Promises`). Se añadieron `Channel` y `Participant` para simular el envío de mensajes.
Se sumaron `Metrics`, `Scheduler`, `Util` y un `Api` de alto nivel para gestionar tareas de red.
La clase `Network` sirve como punto de entrada para las conexiones durante las primeras pruebas. Se añadieron `Stream` y uniones de error (junto con `InitProtocolError` y `ProtocolsError`) para mantener una API parecida a la del crate original.
`Participant` implementa ahora la apertura y recepción de `Stream` con métodos asincrónicos y un seguimiento básico del ancho de banda.
También se añadió una assembly `World` que define las estructuras de terreno principales, sirviendo como base para portar el generador procedural. El módulo `Noise`, integrado en `WorldIndex`, aprovecha `VelorenPort.NativeMath` para generar patrones deterministas en 3D. Se agregó asimismo `TerrainGenerator` junto con la clase `Chunk` para crear datos de terreno iniciales. Además se introdujo `WorldMap` como contenedor de chunks generados dinámicamente. El `GameServer` ya utiliza este mapa para generar los `Chunk` alrededor de cada cliente. Dado que el paquete `VelorenPort.NativeMath` no está disponible en el entorno de compilación, se mantiene un stub dentro de `CoreEngine` que ahora expone operaciones y constantes adicionales (como `math.PI`) y una versión del algoritmo de ruido Simplex para evitar errores de compilación y conservar patrones de terreno coherentes.

Se incorporó además `Calendar`, la enumeración `DayPeriod` y el módulo `Clock`
dentro de `CoreEngine` para gestionar eventos estacionales, determinar las
fases del día y controlar el tiempo de ejecución al estilo del código en Rust.
Junto a ellos se añadieron los recursos de tiempo (`TimeOfDay`, `Time`,
`ProgramTime`, `DeltaTime`, `Secs` y `TimeScale`) que son utilizados en la lógica
original para representar el paso del tiempo y escalas de simulación.
Se añadieron constantes en `Consts` y el tipo `ViewDistances` para controlar el alcance de dibujo.
Se revisaron los módulos existentes para cubrir funciones faltantes, añadiendo `SetTargetDt` al `Clock` y soporte de `Actor` en `IdMaps`.
Adicionalmente se agregaron los recursos `GameMode`, `PlayerEntity` y `PlayerPhysicsSettings`, junto con los enums `MapKind` y `BattleMode`, y se redefinió `Actor` para almacenar sus identificadores. También se implementaron `ServerConstants`, `Pos`, el recurso `EntitiesDiedLastTick` y la enumeración `DisconnectReason`.
Se añadio un contenedor `Grid` para manejar areas 2D de forma simple.
Finalmente se incorporó `Presence` con el enumerado `PresenceKind`. Las
variantes `LoadingCharacter` y `Character` almacenan internamente un
`CharacterId`, pero sólo la segunda expone dicho valor de forma pública para
evitar asumir que el personaje ya está disponible. La estructura `ViewDistance`
regula la visibilidad y sincronización de cada entidad.
También se añadieron `SpatialGrid` y el recurso `CachedSpatialGrid` para acelerar la búsqueda de entidades cercanas.
Se integraron `Path` y el algoritmo `AStar` y `Ray` para soportar cálculos de rutas y recorridos de voxels.
Finalmente se incluyó `SlowJobPool` para ejecutar trabajos costosos en paralelo sin afectar el hilo principal.
Se añadió `Spiral` para generar espirales de coordenadas en utilidades de terreno y navegación.

Se creó igualmente la assembly `Server` con una clase `GameServer` que mantiene un bucle asincrónico y acepta participantes mediante el módulo de red.
Se añadieron las clases `Client` y `ConnectionHandler` para manejar nuevas conexiones y clientes en el servidor.

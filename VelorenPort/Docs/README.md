# Documentación

En esta carpeta se recopilan guías y decisiones de diseño tomadas durante el proceso de port.

Archivos previstos:
- `PlanDetallado.md`: Detalle completo de ficheros a migrar y pasos específicos.
- `Plan.md`: Plan de acción y tareas iniciales.
- `Interoperabilidad.md`: Experimentos para comunicar código Rust existente con Unity.
Tambien se documentará el progreso de cada sistema portado. El primero en migrarse es `CoreEngine`, con sus definiciones base en `../CoreEngine/Src`.

El sistema `Network` ya cuenta con una assembly y tipos básicos de dirección en `../Network/Src`.
Ahora se incluyen también estructuras para mensajes (`Message`), parámetros de stream (`StreamParams`) y flags (`Promises`). Se añadieron `Channel` y `Participant` para simular el envío de mensajes.
Se sumaron `Metrics`, `Scheduler`, `Util` y un `Api` de alto nivel para gestionar tareas de red.
La clase `Network` sirve como punto de entrada para las conexiones durante las primeras pruebas. Se añadieron `Stream` y las enumeraciones de error para mantener una API parecida a la del crate original.
También se añadió una assembly `World` que empieza a definir tipos simplificados de terreno, sirviendo como base para portar el generador procedural.

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


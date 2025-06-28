# CoreEngine

Contiene los crates bajo `common` que agrupan la lógica compartida: ECS, definiciones de componentes, utilidades de red y estado.

**Viabilidad**: Media. Estos módulos usan conceptos avanzados de Rust como traits genéricos y macros. En C# se pueden recrear usando patrones de composición y generics, pero requiere reescribir gran parte del código.

**Notas**:
- Revisar cada submódulo (`ecs`, `base`, `state`, `systems`) y mapear a sistemas de Unity (ej. utilizar `ECS` de Unity si se desea, o implementar estructura propia).

### Módulos migrados
- `Uid`, `CharacterId` y `RtSimEntity` para identificación de entidades.
- `Calendar` con detección básica de eventos estacionales.
- `DayPeriod` para calcular la fase del día.
- `Clock` con control de ticks y estadísticas.
- Recursos de tiempo como `TimeOfDay`, `Time`, `ProgramTime`, `DeltaTime`, `TimeScale` y `Secs`.
- `Consts` con valores de físicas y rangos por defecto.
- `ViewDistances` para limitar la distancia de terreno y entidades.
- Se añadieron métodos para igualar la funcionalidad de Rust, por ejemplo `SetTargetDt` en `Clock` y eliminación con múltiples IDs en `IdMaps`.
- `Actor` se redefinió como tipo discriminado con IDs.
- Nuevos recursos: `GameMode`, `PlayerEntity`, `PlayerPhysicsSettings`, `MapKind` y `BattleMode`.
- `ServerConstants` para configurar el ciclo día/noche en servidor.
- `Pos` y `EntitiesDiedLastTick` como utilidades básicas de simulación.
- `DisconnectReason` enumera los motivos de desconexión del jugador.
- `AdminRole` define los niveles de privilegios y el componente `Admin` almacena
  el rol de cada jugador. Incluye utilidades `AdminRoleHelper` para convertir de
  cadena a rol y viceversa, equivalente a los métodos `FromStr` y `ToString` en
  Rust.
- `Grid` para contenedores bidimensionales genericos.
 - `Presence` define un enumerado `PresenceKind` y almacena un `CharacterId` opcional cuando el tipo es `LoadingCharacter` o `Character`. La estructura `ViewDistance` regula la visibilidad y el estado de sincronización de cada entidad. Si se cambia a otra variante, el identificador se descarta automáticamente.
- `SpatialGrid` y `CachedSpatialGrid` aceleran la consulta de entidades en un área.
- `Path`, `AStar` y `Ray` ofrecen utilidades de rutas y recorrido de voxels.
- `SlowJobPool` administra trabajos costosos en segundo plano sin bloquear el ciclo principal.
- `Spiral` permite recorrer posiciones alrededor de un punto siguiendo un patrón en espiral.
- Se añadieron utilidades de geometría y color: `ColorUtil`, `Dir`, `Plane`,
  `Projection`, `Lines`, `FindDist` y `GridHasher` junto con la estructura
  `Rgba`.
- Nuevas interfaces de volumen (`IReadVol`, `IWriteVol`, `ISizedVol`) con los
  enumeradores `DefaultPosEnumerator` y `DefaultVolEnumerator` facilitan
  recorrer y modificar volúmenes genéricos.
- `VersionInfo` genera cadenas de versión leyendo variables de entorno de Git.
- `Weather` incluye ahora cuadrículas interpoladas y conversión comprimida para
  sincronización eficiente.
- `EventBus` y el enum `LocalEvent` simplifican la comunicación entre sistemas.
- Estructuras básicas de inteligencia artificial (`RtSimController`, `NpcAction`, `NpcActivity`)
  permiten emular comportamiento de NPC sin depender de Unity.
  `RtSimController` ahora incluye métodos para iniciar diálogos y mover NPCs de
  forma similar al código Rust original.

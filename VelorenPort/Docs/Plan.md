# Plan de acción para el port a C#
Para un desglose completo de ficheros y tareas consulte [PlanDetallado.md](PlanDetallado.md).


1. **Análisis profundo del código**
   - Revisar cada crate de Rust para identificar dependencias y responsabilidades.
   - Documentar en `Docs` cualquier hallazgo relevante.

2. **Diseño de arquitectura en Unity**
   - Definir cómo se mapeará la estructura actual a proyectos de Unity (assemblies por sistema).
   - Evaluar uso de `ECS` de Unity o implementación propia.

3. **Portar sistema de redes**
   - Crear módulos C# que reproduzcan el comportamiento de `veloren-network`.
   - Se añadieron utilidades iniciales (`Metrics`, `Scheduler`, `Util` y un `Api` de alto nivel) junto a `Network`.
   - Se incorporó una pequeña jerarquía de errores (`NetworkError`, `NetworkConnectError`, `ParticipantError`, `StreamError`) y la clase `Stream` para continuar la equivalencia con Rust.
   - Probar comunicación cliente-servidor básica dentro de Unity.
   - Evaluar si conviene migrar todo el crate de una vez o avanzar por partes, comenzando por las estructuras de mensajes.

4. **Portar lógica de mundo y simulación**
   - Se comenzó con una assembly `World` que contiene bloques y un Índice básicos.
   - Probar comunicación cliente-servidor básica dentro de Unity.
   - Evaluar si conviene migrar todo el crate de una vez o avanzar por partes, comenzando por las estructuras de mensajes.
   - Adaptar generador de mundo y datos persistentes.
   - Decidir si `rtsim` se reescribe o se mantiene en Rust mediante FFI.

5. **Migrar interfaz y cliente**
   - Reemplazar `voxygen` con escenas y UI de Unity.
   - Integrar sistemas de animación y controladores.

6. **Herramientas y CLI**
   - Reescribir scripts de servidor y utilidades.

7. **Pruebas y validación**
   - Implementar pruebas unitarias y de integración en C#.
   - Verificar compatibilidad multiplataforma.

## Estatus
- Se migraron tipos base en `CoreEngine` y se añadieron `Calendar`, `DayPeriod`,
  `Clock` y los recursos de tiempo (`TimeOfDay`, `Time`, `DeltaTime`, etc.).
- La assembly `Network` cuenta con `Api`, `Metrics`, `Scheduler`, `Stream` y tipos asociados.
- Nuevos módulos `Consts` y `ViewDistances` completan el port de constantes y configuraciones de visibilidad.
- Se creó `World` con estructuras de bloque iniciales.
- Revisadas utilidades de tiempo e identificación para agregar métodos faltantes como `SetTargetDt` en `Clock` y soporte de `Actor` en `IdMaps`.
- Se añadieron `GameMode`, `PlayerEntity`, `PlayerPhysicsSettings`, `MapKind` y `BattleMode` para continuar la migración de recursos.
- Se implementaron `ServerConstants`, `Pos` y `EntitiesDiedLastTick` como parte de las estructuras básicas del juego.
- Se añadió `DisconnectReason` para registrar los motivos de desconexión.
- Se añadio `Grid` como utilitario para datos bidimensionales.
 - Se incorporó `Presence` con el enumerado `PresenceKind` y un campo opcional `CharacterId` usado sólo cuando el tipo es `LoadingCharacter` o `Character`. Se mantiene la estructura `ViewDistance` para gestionar la visibilidad y sincronización de los jugadores. Cuando la presencia cambia a otro tipo, este identificador se elimina.
- Se creó `SpatialGrid` y el recurso `CachedSpatialGrid` para reutilizar consultas espaciales entre sistemas.
- Se añadieron `Path`, `AStar` y `Ray` para calcular rutas y recorridos de voxels.
- Se añadió `SlowJobPool` para ejecutar trabajos costosos en segundo plano.
- Se implementó `Spiral` como iterador de coordenadas en espiral para utilidades de generación.
- Se añadió la assembly `Server` con un esqueleto `GameServer` que integra el módulo de red y un reloj de juego.
- Se agregaron `Client` y `ConnectionHandler` en la assembly `Server` para gestionar conexiones y clientes.

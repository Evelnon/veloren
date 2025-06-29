# Estado de Migración a C#

Este documento resume el estado actual del port de Veloren a C# (carpeta `VelorenPort`). Se enumeran subsistemas y clases que aún no alcanzan la paridad con el código Rust.

## CoreEngine

- Falta un ECS completo. Solo existen componentes básicos y utilidades de estado.
- Varios submódulos del crate `common` siguen sin migrar: `states`, `terrain`, `volumes`, `util` y `figure` solo exponen enumeraciones o stubs básicos. Faltan sistemas como `store`, `trade` y partes de `slowjob`.
- El manejo de volúmenes (`VolGrid3d`, editores de terreno y biomas) solo cubre estructuras mínimas; no se han portado volúmenes escalados ni utilidades de compresión.
- El sistema de grupos no sincroniza eventos a través de la red ni gestiona privilegios avanzados.
- La persistencia de regiones guarda un historial breve en disco sin políticas de rotación.
- `WeatherJob` gestiona zonas temporales e interpola transiciones básicas entre estados, pero sigue sin efectos visuales ni modelos físicos completos.
- `Searcher` admite un `NavGrid` para celdas bloqueadas, pero no hay una malla de navegación completa.
- `ChunkSupplement` registra recursos y posiciones de aparición que todavía no se integran en la generación.
- La estructura `Aabb` ahora incluye utilidades de unión y translación,
  pero siguen faltando más operaciones geométricas avanzadas.
- La cobertura de pruebas es limitada.
- `CharacterState` ahora compila y cuenta con pruebas básicas, pero faltan muchas acciones complejas (blink, combos, transformaciones).
- Ya no se incluyen stubs de `UnityEngine`; el proyecto se compila sin dependencias de Unity.
- `VelorenPort.NativeMath` está parcialmente implementado. Se añadieron `bool2`,
  `math.distance`, `math.isfinite`, `math.any`, `clamp` para vectores,
  normalización y multiplicación de `quaternion`, rotación básica por eje
  (`axisAngle`), propiedades `float3.xy`, constructores simplificados y
  operaciones con `int3`. Aún faltan tipos como `int4` y funciones de
  trigonometría avanzada.
- La estructura `quaternion` implementa `normalize`, `mul`, `axisAngle` y
  `rotate`, pero faltan conversiones completas con matrices y Euler angles.
- Las funciones de `Store` y `Trade` están reducidas; faltan catálogos dinámicos y tarifas basadas en reputación.
- No se han portado las utilidades de `common/src/bin` (por ejemplo `asset_migrate`, `csv_export`).
- `SlowJobPool` solo cubre la cola de trabajos y carece de configuración de prioridades y de una limitación adecuada de trabajadores.

## World

 - La generación de civilizaciones (módulo `civ`) crea sitios básicos con asignación limitada de NPC.
- Faltan capas dinámicas: cuevas, flora y fauna realistas.
- `WorldSim` carece de erosión y difusión de humedad fiel a la versión Rust.
- Los generadores de sitios (`site/gen` y `site/plot`) solo crean unas pocas casas de ejemplo.
- Los tiles (`site/tile`) y utilidades de gradientes se encuentran en versiones simplificadas.

## Network

- El protocolo incluye solo mensajes básicos; faltan muchos tipos usados por el servidor Rust.
- No hay autenticación ni cifrado de paquetes.

## Server

- Los sistemas de `sys` (IA, sincronización de entidades, objetos, mascotas, etc.) están presentes solo en forma mínima.
- El servidor no inicializa un conjunto completo de sistemas de juego.
- Persistencia y migraciones no usan base de datos; se guardan ficheros simples.
- Métricas y monitorización se limitan a un exportador Prometheus reducido.

## Simulation

- `Rtsim` mantiene un esqueleto sin la lógica de IA avanzada ni cálculos físicos.
  - Faltan árboles de comportamiento de IA.
  - No se han portado los trabajos de física.
  - Interacciones con el entorno ausentes.

## Client

- El proyecto Unity apenas contiene escenas de prueba; falta la mayoría de interfaces y controladores.

## Herramientas y otros

- La CLI de administración carece de comandos avanzados.
- `PluginManager` carga ensamblados pero no dispone de una API estable.
- La cobertura de pruebas automáticas es baja comparada con el proyecto Rust.

La migración continúa de forma incremental. Se completarán estos módulos según se vayan necesitando para reproducir la jugabilidad del original.

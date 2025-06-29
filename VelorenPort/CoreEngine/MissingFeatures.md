# Estado de Migración a C#

Este documento resume el estado actual del port de Veloren a C# (carpeta `VelorenPort`). Se enumeran subsistemas y clases que aún no alcanzan la paridad con el código Rust.

## CoreEngine

- Falta un ECS completo. Solo existen componentes básicos y utilidades de estado.
- El manejo de volúmenes (`VolGrid3d`, editores de terreno y biomas) solo cubre estructuras mínimas.
- El sistema de grupos no sincroniza eventos a través de la red ni gestiona privilegios avanzados.
- La persistencia de regiones guarda un historial breve en disco sin políticas de rotación.
- `WeatherJob` gestiona zonas temporales e interpola transiciones básicas entre estados, pero sigue sin efectos visuales ni modelos físicos completos.
- `Searcher` admite un `NavGrid` para celdas bloqueadas, pero no hay una malla de navegación completa.
- `ChunkSupplement` registra recursos y posiciones de aparición que todavía no se integran en la generación.
- La cobertura de pruebas es limitada.

## World

- La generación de civilizaciones (módulo `civ`) se reduce a sitios aleatorios, sin economía ni asignación de NPC.
- Faltan capas dinámicas: cuevas, flora y fauna realistas.
- `WorldSim` carece de erosión y difusión de humedad fiel a la versión Rust.
- Los generadores de sitios (`site/gen` y `site/plot`) solo crean unas pocas casas de ejemplo.
- No hay economía (`site/economy`) ni rutas comerciales.
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

## Client

- El proyecto Unity apenas contiene escenas de prueba; falta la mayoría de interfaces y controladores.

## Herramientas y otros

- La CLI de administración carece de comandos avanzados.
- `PluginManager` carga ensamblados pero no dispone de una API estable.
- La cobertura de pruebas automáticas es baja comparada con el proyecto Rust.

La migración continúa de forma incremental. Se completarán estos módulos según se vayan necesitando para reproducir la jugabilidad del original.

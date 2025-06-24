# Plan de Port a C# / Unity

Este documento resume la viabilidad de convertir cada sistema de Veloren a C# y propone un plan de acción inicial.

## Viabilidad por sistema

### Common
Contiene la mayoría de componentes de ECS y utilidades compartidas. Su lógica es mayormente independiente del motor gráfico, por lo que puede migrarse a C# como bibliotecas estáticas o scripts de Unity. **Viable**, aunque requiere reescritura manual de muchas estructuras y sistemas.

### Network
Implementa el protocolo de red (mensajes binarios, compresión, etc.). La traducción a C# es factible usando `System.Net.Sockets` y bibliotecas de serialización. **Viable**, pero debe revisarse la compatibilidad de funciones asincrónicas.

### World
Generación procedural, economía y datos del mundo. Utiliza numerosas estructuras y algoritmos propios. **Viable**, aunque implica portar complejos algoritmos matemáticos y de generación procedural a C#.

### Server
Servidor dedicado que usa Tokio y actix en Rust. En C#, podría apoyarse en `System.Threading.Tasks` y frameworks de red como `ASP.NET` o soluciones personalizadas. **Viable**, pero requiere rediseñar la concurrencia.

### Client
Biblioteca de comunicación para el cliente. Es relativamente pequeña y depende de `network`. **Viable**.

### Voxygen
Cliente gráfico basado en `wgpu`/`winit`. Su código es extenso y muy acoplado a librerías de Rust. Para Unity, se reemplazará por scripts C# que utilicen la API propia del engine. **Parcialmente viable**: mucha funcionalidad se sustituirá por componentes nativos de Unity en lugar de portarse directamente.

### Tools
- **ServerCli**: scripts de lanzamiento y utilidades de administración. **Viable** y simple de portar.
- **Rtsim**: simulación en tiempo real. Dependerá de gráficas y utilidades de Rust; en Unity parte de esto puede descartarse. **Parcialmente viable**.
- **Plugin**: experimentos con WebAssembly y plugins. En Unity podría reemplazarse por un sistema de `Assembly Load` o paquetes. **Cuestionable**, puede requerir un enfoque distinto.

### Assets
Los recursos artísticos se pueden reutilizar en Unity importándolos al formato adecuado. **Viable**.

## Plan de acción

1. **Preparación del repositorio**
   - Crear la estructura de carpetas de este proyecto (`Core`, `Tools`, `Assets`) para organizar el código C#.
   - Documentar la función de cada sistema (este documento).
2. **Traducción inicial de Common y Network**
   - Reescribir las estructuras ECS básicas en C# o aprovechar frameworks existentes.
   - Portar el protocolo de red y establecer comunicación básica entre cliente y servidor en C#.
3. **Portar World y Server**
   - Adaptar las rutinas de generación procedural y lógica del servidor.
   - Garantizar la compatibilidad con el protocolo de red ya traducido.
4. **Implementar cliente en Unity**
   - Sustituir Voxygen por escenas y scripts Unity que consuman la lógica ya portada.
   - Reaprovechar recursos de `Assets` dentro del motor.
5. **Migrar o recrear herramientas**
   - Decidir caso por caso si `Rtsim` y `Plugin` son necesarios o si se reemplazan por soluciones específicas de Unity.
6. **Pruebas y optimización**
   - Establecer pruebas unitarias en C#.
   - Comparar el rendimiento con la versión original en Rust.

## Tareas iniciales

1. Configurar el repositorio `VelorenPort` con proyectos de Unity y C#.
2. Crear proyecto base de servidor y cliente en C#.
3. Portar componentes básicos de `common` (ECS, tipos compartidos).
4. Traducir las definiciones de mensajes de `network`.
5. Implementar un prototipo simple que conecte cliente y servidor.
6. Definir el pipeline de assets desde los archivos existentes a Unity.

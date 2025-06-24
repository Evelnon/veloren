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

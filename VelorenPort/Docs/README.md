# Documentación

En esta carpeta se recopilan guías y decisiones de diseño tomadas durante el proceso de port.

Archivos previstos:
- `PlanDetallado.md`: Detalle completo de ficheros a migrar y pasos específicos.
- `Plan.md`: Plan de acción y tareas iniciales.
- `Interoperabilidad.md`: Experimentos para comunicar código Rust existente con Unity.
Tambien se documentará el progreso de cada sistema portado. El primero en migrarse es `CoreEngine`, con sus definiciones base en `../CoreEngine/Src`.

El sistema `Network` ya cuenta con una assembly y tipos basicos de direccion en `../Network/Src`.
Ahora se incluyen tambien estructuras para mensajes (Message), parametros de stream (StreamParams) y flags (Promises). Se añadieron `Channel` y `Participant` para simular el envio de mensajes.
La clase `Network` sirve como punto de entrada para las conexiones durante las primeras pruebas.

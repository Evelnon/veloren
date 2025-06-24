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
   - Probar comunicación cliente-servidor básica dentro de Unity.
   - Evaluar si conviene migrar todo el crate de una vez o avanzar por partes, comenzando por las estructuras de mensajes.

4. **Portar lógica de mundo y simulación**
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

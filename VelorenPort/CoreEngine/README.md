# CoreEngine

Contiene los crates bajo `common` que agrupan la lógica compartida: ECS, definiciones de componentes, utilidades de red y estado.

**Viabilidad**: Media. Estos módulos usan conceptos avanzados de Rust como traits genéricos y macros. En C# se pueden recrear usando patrones de composición y generics, pero requiere reescribir gran parte del código.

**Notas**:
- Revisar cada submódulo (`ecs`, `base`, `state`, `systems`) y mapear a sistemas de Unity (ej. utilizar `ECS` de Unity si se desea, o implementar estructura propia).

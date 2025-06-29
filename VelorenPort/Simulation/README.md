# Simulation

Contiene `rtsim`, un simulador global que ejecuta comportamientos del mundo y de poblaciones.

**Viabilidad**: Baja-Media. Esta parte hace uso intensivo de estructuras de datos en Rust y macros. El proyecto optó por reescribir el simulador completamente en C#, sin interop con Rust.

**Notas**:
- Portar este subsistema por completo a C#, eliminando cualquier dependencia de Rust o FFI.

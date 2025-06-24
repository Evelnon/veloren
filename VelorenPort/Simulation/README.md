# Simulation

Contiene `rtsim`, un simulador global que ejecuta comportamientos del mundo y de poblaciones.

**Viabilidad**: Baja-Media. Esta parte hace uso intensivo de estructuras de datos en Rust y macros. Se puede portar a C#, pero podría implicar reescribir gran parte de la lógica. Tal vez se implemente como un servicio externo o en Rust interop.

**Notas**:
- Evaluar si realmente es necesario portar este subsistema completo o mantenerlo como módulo Rust separado usando interop (por ejemplo, a través de FFI o WebAssembly).

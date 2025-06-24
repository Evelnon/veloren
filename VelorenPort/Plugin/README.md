# Plugin

Sistema de plugins escrito en Rust que utiliza WebAssembly (`plugin`).

**Viabilidad**: Media. Unity soporta C# nativo y tiene sistemas de paquetes y scripting. El soporte actual en Rust se basa en WASM y puede mantenerse mediante interoperabilidad o reescribirse como API de C#.

**Notas**:
- Considerar mantener la compatibilidad con WASM para scripts ligeros.
- Revisar cómo exponer la API de juego a otros desarrolladores.

# Plugin

Sistema de plugins reescrito en C# que utiliza opcionalmente WebAssembly (`plugin`).

**Viabilidad**: Media. Unity soporta C# nativo y tiene sistemas de paquetes y scripting. El antiguo soporte en Rust se ha eliminado y se prioriza la implementación directa en C#.

**Notas**:
- Considerar mantener la compatibilidad con WASM para scripts ligeros.
- Revisar cómo exponer la API de juego a otros desarrolladores.

## Plugin Manager

Se añadió una implementación inicial del cargador de plugins en C#. El archivo
`PluginManager` busca ensamblados `.dll` en un directorio y crea instancias de
todas las clases que implementen `IGamePlugin`. Cada plugin expone un nombre y
un método `Initialize` que se ejecuta después de cargarse.

También se incluyó un archivo `veloren.wit` a modo de ejemplo para las
interfaces WebAssembly que se planean soportar en el futuro.

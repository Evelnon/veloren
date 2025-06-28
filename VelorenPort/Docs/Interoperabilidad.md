# Interoperabilidad con código Rust

Este documento recogerá pruebas para integrar módulos existentes en Rust mediante FFI o Wasm, en caso de que ciertos subsistemas se mantengan en Rust.

## Estado actual

Aún no se ha realizado ningún experimento de interoperabilidad. El código en C#
no expone bindings ni define la ABI necesaria para enlazar con bibliotecas
compiladas en Rust.

## Tareas pendientes

- Evaluar el uso de `DllImport` para cargar funciones compiladas en Rust con
  `extern "C"`.
- Considerar una capa Wasm para aprovechar la portabilidad y la seguridad del
  entorno sandbox.
- Definir un protocolo de comunicación estable entre ambos entornos (tipos,
  codificación de mensajes y gestión de memoria).
- Crear ejemplos mínimos que demuestren intercambio de datos entre C# y Rust.
- Documentar la configuración del proyecto y los pasos de compilación cruzada.

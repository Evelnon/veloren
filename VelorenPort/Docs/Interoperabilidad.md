# Interoperabilidad con código Rust

Este documento recogerá pruebas para integrar módulos existentes en Rust mediante FFI o Wasm, en caso de que ciertos subsistemas se mantengan en Rust.

## Estado actual

Se añadió un pequeño experimento para exponer funciones de red escritas en
Rust.  El crate `network-ffi` compila como biblioteca compartida y ofrece la
función `vp_send_udp` para enviar datagramas.  En C# se creó la clase
`RustBindings` que utiliza `DllImport` para invocar dicha función desde la
assembly `Network`.

## Tareas pendientes

- Evaluar el uso de `DllImport` para cargar funciones compiladas en Rust con
  `extern "C"`.
- Considerar una capa Wasm para aprovechar la portabilidad y la seguridad del
  entorno sandbox.
- Definir un protocolo de comunicación estable entre ambos entornos (tipos,
  codificación de mensajes y gestión de memoria).
- Crear ejemplos mínimos que demuestren intercambio de datos entre C# y Rust.
- Documentar la configuración del proyecto y los pasos de compilación cruzada.

## Compilación de la biblioteca FFI

```bash
# Desde la raíz del repositorio
cargo build --release -p network-ffi

# En Windows se generará `network_ffi.dll` y en Linux `libnetwork_ffi.so`
```

### Ejemplo de uso en C#

```csharp
bool ok = UdpWrapper.Send("127.0.0.1:9876", "ping");
```

## Exploración Wasm

Otra opción es compilar `network-ffi` a `wasm32-unknown-unknown` y utilizar
`wasm-bindgen` para exponer una API JavaScript. Esta variante permitiría que un
cliente ligero en navegadores reciba y envíe mensajes sin depender de plugins
nativos. El empaquetado final podría integrarse en una aplicación WebGL de
Unity o en un cliente ligero fuera del motor.

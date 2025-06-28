# Network Port Missing Features

Este documento resume las funcionalidades del crate `network` de Rust que aún no se han portado a C# o están incompletas.

- **Gestión avanzada de streams**: faltan las colas de prioridad y la capa de fiabilidad completa. La clase `Stream` sólo implementa retransmisiones básicas.
- **Métricas de red**: se exponen contadores simples mediante `prometheus-net`, pero no se registran estadísticas detalladas ni carga del planificador.
- **Planificador y concurrencia**: el `Scheduler` carece de balanceo dinámico y control de carga como en Rust. La desconexión de tareas pendientes es limitada.
- **Cobertura de protocolos**: el transporte UDP es experimental y no reproduce las garantías de QUIC del proyecto original. La negociación de protocolos es mínima.
- **Compatibilidad con el servidor Rust**: el servidor C# sólo acepta conexiones de prueba; no se intercambian mensajes reales ni se replican los pasos de autenticación y cifrado.
- **Interoperabilidad FFI/Wasm**: aún no existe una capa que permita comunicarse con código Rust o WebAssembly.
- **Pruebas automatizadas**: sólo se han portado algunos tests locales de MPSC; faltan pruebas integrales comparables a la suite de Rust.

La migración continuará abordando estas áreas para alcanzar la paridad con la implementación original.

# Network Port Missing Features

This document tracks notable systems from the original Rust `network` crate that are not yet implemented or only partially ported to C#. The list below expands on all the components identificados hasta la fecha.

- **Protocolo de handshake avanzado**: el intercambio actual se limita a `Pid`, secreto y banderas básicas. Falta replicar el estado completo de negociación y versiones compatible con Rust.
- **Gestión completa de streams**: el control de congestión AIMD y la retransmisión existen, pero faltan colas múltiples, prioridades por mensaje y confirmaciones selectivas.
- **Soporte de QUIC real**: se utiliza UDP confiable de forma experimental; no se ha portado la capa QUIC que emplea el servidor Rust.
- **Autenticación y roles**: la conexión no valida credenciales ni tipos de cliente; la lógica de permisos está incompleta.
- **Instrumentación avanzada del `Scheduler`**: el autoescalado es básico y no se miden latencias ni tiempos de espera como en Rust.
- **Capa de interoperabilidad (FFI/Wasm)**: aún no existe una forma de integrar código nativo o WebAssembly con la red en C#.
- **Cobertura de pruebas**: sólo hay pruebas unitarias básicas; faltan tests de integración que aseguren compatibilidad con el servidor de Rust.

The migration will continue incrementally, porting features as they become necessary for gameplay.

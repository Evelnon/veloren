# Network Port Migration Status

This document outlines the current state of the Network module inside `VelorenPort` and enumerates the missing pieces required to reach feature parity with the original Rust crate.

## Current implementation

- `Network` class supports TCP, UDP (with optional reliability) and rudimentary QUIC.
- Basic handshake exchanges a header with the network version followed by an initialization packet containing `Pid`, secret and `HandshakeFeatures`.
- Handshake now returns the exact remote network version to aid in future compatibility checks. `Participant` instances store this version for inspection and tests verify it is populated.
- Handshake validation updated to rely on `SupportedVersion` length when detecting incoming packets. The intersection of `HandshakeFeatures` is computed so only mutually supported options remain active.
- The local stream identifier offset is determined during the handshake so both peers start numbering streams from different ranges.
- A final confirmation byte is exchanged so both sides wait until the handshake fully completes before opening streams.
- The handshake logic now runs through an explicit series of steps (`SendHeader`, `ReceiveHeader`, `SendInit`, etc.) mirroring the Rust state machine.
- Metrics are collected using `prometheus-net` and the scheduler can autoscale workers.
- Streams implement reliability via retransmissions and congestion control (AIMD).
- Local MPSC transport is available for tests.

## Features still to port

- **Extended handshake negotiation**
  Aunque la máquina de estados básica ya está implementada, faltan rutas de compatibilidad con versiones antiguas y negociación detallada de banderas opcionales.
- **Advanced stream scheduling**  
  Streams have only a single congestion window and priority weights. Multiple queues with per-message priority and selective acknowledgments should be implemented.
- **Full QUIC transport**  
  Current support wraps the .NET `QuicConnection` API but omits connection migration, 0‑RTT and other options used by the Rust server.
- **Authentication and role validation**  
  `Participant` creation accepts any client and does not enforce credentials or role based permissions.
- **Detailed scheduler instrumentation**  
  The scheduler exposes its load but does not track latencies or timeouts like the Rust implementation.
- **FFI or Wasm interoperability layer**  
  There is no mechanism to interact with Rust code or WebAssembly for networking.
- **Compatibility tests with the Rust server**  
  Only unit tests for the MPSC transport exist; integration tests covering real protocol interaction are missing.

The port should gradually implement these components without removing existing files. Completing them will allow the module to compile alongside other projects and remain faithful to the Rust logic.

# Server

Código del servidor principal (`server` y `server-cli`). Maneja sesiones de juego, IA de NPC y persistencia.

**Viabilidad**: Media-Alta. Unity puede ejecutar servidores en C#, pero será necesario reescribir la lógica de concurrencia y manejo de ECS.

**Notas**:
- Portar la gestión de conexiones y eventos.
- Considerar usar `Task` y `async` de C# para las operaciones concurrentes.

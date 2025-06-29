# Server

Código del servidor principal (`server` y `server-cli`). Maneja sesiones de juego, IA de NPC y persistencia.

**Viabilidad**: Media-Alta. Unity puede ejecutar servidores en C#, pero será necesario reescribir la lógica de concurrencia y manejo de ECS.

**Notas**:
- Portar la gestión de conexiones y eventos.
- Considerar usar `Task` y `async` de C# para las operaciones concurrentes.
- Se añadieron las estructuras `RegionSubscription` y `RepositionOnChunkLoad`
  junto a `PresenceConstants` para comenzar a migrar la lógica de presencia y
  suscripciones de regiones.
- Se definieron `RegionConstants`, `TerrainConstants` y utilidades en
  `RegionUtils`, incluida la función `InitializeRegionSubscription`.
- Nuevo `RegionSubscriptionUpdater` mantiene la lista de regiones
  actualizada conforme cambian la posición o la distancia de visión.
- Los `Client` registran posición y `Presence`; el `GameServer` actualiza
  su `RegionSubscription` en cada ciclo y ahora genera los `Chunk` cercanos
  mediante `WorldMap` para comenzar a poblar el mundo.
  Se implementó `DataDir` para indicar la ruta de datos del servidor, la
  enumeración `Error` que centraliza los fallos de red, y
  `PersistenceError` para los errores de almacenamiento.
  También se añadió un manejador de eventos simple basado en `EventBus` que
  permite a los sistemas emitir y consumir eventos tipados. El `Dispatcher`
  entrega la instancia de `EventManager` a cada sistema en cada ciclo y así se
  propagan los eventos como en el original en Rust.

## Compilación y ejecución

Para probar el servidor durante el desarrollo se puede ejecutar:

```bash
dotnet run --project Server.csproj
```

Para generar el binario de distribución:

```bash
dotnet publish Server.csproj -c Release -o build
```

El script [`build.sh`](build.sh) automatiza este último paso.

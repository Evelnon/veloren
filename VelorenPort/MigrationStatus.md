# Migration Status

This document tracks progress of the Rust to C# port. Las cifras previas no reflejaban la realidad y se han actualizado de forma aproximada.

## Overall progress

| Sistema | Porcentaje |
|---------|-----------:|
| CoreEngine | 30% |
| Network | 25% |
| World | 20% |
| Server | 15% |
| Client | 10% |
| Simulation | 5% |
| CLI | 5% |
| Plugin | 5% |

## Modulos faltantes

Aunque el port cuenta con varios ficheros iniciales, muchos subsistemas del crate `common` no se han migrado o solo estan presentes como stubs:
- `states` para comportamientos de combate y movimiento.
- `terrain` y `volumes` con utilidades de bloques.
- gran parte de `util` (colores, proyecciones, compresion, etc.).
- la mayoria de componentes en `comp`, salvo unos pocos como `BuffKind` o `Player`.
- modulo `weather` solo con una version minima.
- otros submodulos como `slowjob`, `store`, `trade`, `figure` y `bin`.


## Per-file progress

### CoreEngine

| Archivo | Porcentaje |
|---------|-----------:|
| AStar.cs | 100% |
| CachedSpatialGrid.cs | 100% |
| Calendar.cs | 100% |
| CharacterId.cs | 100% |
| Clock.cs | 100% |
| Consts.cs | 100% |
| DisconnectReason.cs | 100% |
| EntitiesDiedLastTick.cs | 100% |
| EventBus.cs | 100% |
| GameResources.cs | 100% |
| Grid.cs | 100% |
| Path.cs | 100% |
| Pos.cs | 100% |
| Presence.cs | 100% |
| Ray.cs | 100% |
| RtSimEntity.cs | 100% |
| RtSim.cs | 100% |
| Event.cs | 100% |
| SlowJobPool.cs | 100% |
| Spiral.cs | 100% |
| TerrainConstants.cs | 100% |
| Rgb.cs | 100% |
| Rgb8.cs | 100% |
| MathUtil.cs | 100% |
| InvSlotId.cs | 100% |
| IInventory.cs | 100% |
| ItemDefinitionIdOwned.cs | 100% |
| ReducedInventory.cs | 100% |
| MaterialUse.cs | 100% |
| MaterialFrequency.cs | 100% |
| Uid.cs | 100% |
| UnityEntitiesStub.cs | 100% |
| UnityMathematicsStub.cs | 100% |
| ViewDistances.cs | 100% |
| Actor.cs | 100% |
| LiquidKind.cs | 100% |
| Character.cs | 100% |
| Cmd.cs | 100% |
| Combat.cs | 100% |
| Depot.cs | 100% |
| Effect.cs | 100% |
| Explosion.cs | 100% |
| Generation.cs | 100% |
| Interaction.cs | 100% |
| Link.cs | 100% |
| Lod.cs | 100% |
| Lottery.cs | 100% |
| Mounting.cs | 100% |
| Npc.cs | 100% |
| Outcome.cs | 100% |
| Recipe.cs | 100% |
| Region.cs | 100% |
| SkillsetBuilder.cs | 100% |
| Resources.cs | 100% |
| SharedServerConfig.cs | 100% |
| Spot.cs | 100% |
| Store.cs | 100% |
| Tether.cs | 100% |
| TradePricing.cs | 100% |
| Trade.cs | 100% |
| Typed.cs | 100% |
| Vol.cs | 100% |
| VolumeInterfaces.cs | 100% |
| GoodIndex.cs | 100% |
| GoodMap.cs | 100% |
| Weather.cs | 100% |
| AdminRole.cs | 100% |
| DayPeriod.cs | 100% |
| ServerConstants.cs | 100% |
| SpatialGrid.cs | 100% |
| TimeResources.cs | 100% |
| UserdataDir.cs | 100% |
| Content.cs | 100% |
| comp/BuffKind.cs | 100% |
| comp/Group.cs | 100% |
| comp/Chat.cs | 100% |
| comp/Player.cs | 100% |
| comp/item/Reagent.cs | 100% |
| comp/item/ToolKind.cs | 100% |
| Result.cs | 100% |
| QuadraticBezier2.cs | 100% |
| RandomField.cs | 100% |
| InputKind.cs | 100% |
| InputAttr.cs | 100% |
| ControllerInputs.cs | 100% |
| Controller.cs | 100% |
| StageSection.cs | 100% |

### Network

| Archivo | Porcentaje |
|---------|-----------:|
| Api.cs | 100% |
| AuthClient.cs | 100% |
| Channel.cs | 100% |
| Metrics.cs | 100% |
| Participant.cs | 100% |
| Scheduler.cs | 100% |
| Util.cs | 100% |
| Message.cs | 100% |
| Network.cs | 100% |
| QuicClientConfig.cs | 100% |
| QuicServerConfig.cs | 100% |
| BanInfo.cs | 100% |
| ClientMessages.cs | 100% |
| ConnectAddr.cs | 100% |
| InitProtocolError.cs | 100% |
| ListenAddr.cs | 100% |
| MpscError.cs | 100% |
| NetworkConnectError.cs | 100% |
| NetworkError.cs | 100% |
| ParticipantError.cs | 100% |
| ParticipantEvent.cs | 100% |
| Pid.cs | 100% |
| Promises.cs | 100% |
| ProtocolError.cs | 100% |
| ProtocolsError.cs | 100% |
| QuicError.cs | 100% |
| RegisterError.cs | 100% |
| ServerDisconnectReason.cs | 100% |
| Sid.cs | 100% |
| Stream.cs | 100% |
| StreamError.cs | 100% |
| StreamParams.cs | 100% |

#### Pending features in `Network`

- Advanced stream prioritization and congestion handling
- Complete reliability layer for UDP and QUIC
- Comprehensive Prometheus metrics
- Scheduler parity with the Rust implementation
- Real communication with the original Rust server
- Extended handshake steps and protocol negotiation
- Extensive unit and integration tests
- Investigation of FFI or Wasm interoperability

### World

| Archivo | Porcentaje |
|---------|-----------:|
| Block.cs | 100% |
| BlockKind.cs | 100% |
| BiomeKind.cs | 100% |
| Chunk.cs | 100% |
| Noise.cs | 100% |
| TerrainChunkSize.cs | 100% |
| MapSizeLg.cs | 100% |
| WorldUtil.cs | 100% |
| FastNoise2d.cs | 100% |
| StructureGen2d.cs | 100% |
| ChunkResource.cs | 100% |
| ChunkSupplement.cs | 100% |
| StructureBlock.cs | 100% |
| TerrainGenerator.cs | 100% |
| WorldIndex.cs | 100% |
| WorldMap.cs | 100% |
| World.cs | 90% |
| All.cs | 100% |
| Canvas.cs | 95% |
| Column.cs | 100% |
| ColumnSample.cs | 100% |
| Config.cs | 100% |
| Land.cs | 100% |
| ColumnGen.cs | 95% |
| Economy.cs | 100% |
| Site/Economy/Cache.cs | 100% |
| Site/Economy/GoodIndex.cs | 100% |
| Site/Economy/GoodMap.cs | 100% |
| Site/Site.cs | 100% |
| SimChunk.cs | 100% |
| WorldSim.cs | 90% |
| RegionInfo.cs | 100% |
| Sim/Way.cs | 100% |
| Sim/River.cs | 100% |
| Lib.cs | 90% |
| Pathfinding.cs | 100% |
| Sim2.cs | 100% |

### Server

| Archivo | Porcentaje |
|---------|-----------:|
| Automod.cs | 100% |
| CharacterCreator.cs | 100% |
| Chat.cs | 100% |
| ChunkGenerator.cs | 100% |
| ChunkSerialize.cs | 100% |
| Client.cs | 100% |
| Cmd.cs | 100% |
| ConnectionHandler.cs | 100% |
| DataDir.cs | 100% |
| ModerationSettings.cs | 100% |
| Error.cs | 100% |
| PersistenceError.cs | 100% |
| Input.cs | 100% |
| Lib.cs | 100% |
| Locations.cs | 100% |
| Lod.cs | 100% |
| LoginProvider.cs | 100% |
| Metrics.cs | 100% |
| Pet.cs | 100% |
| PreparedMsg.cs | 100% |
| Presence.cs | 100% |
| PresenceConstants.cs | 100% |
| RegionConstants.cs | 100% |
| RegionSubscription.cs | 100% |
| RegionSubscriptionUpdater.cs | 100% |
| RegionUtils.cs | 100% |
| RepositionOnChunkLoad.cs | 100% |
| StateExt.cs | 100% |
| TerrainPersistence.cs | 100% |
| TestWorld.cs | 100% |
| Wiring.cs | 100% |
| GameServer.cs | 100% |
| ServerDescription.cs | 100% |
| ServerPhysicsForceList.cs | 100% |

### Client

| Archivo | Porcentaje |
|---------|-----------:|
| ConnectionArgs.cs | 100% |
| Error.cs | 100% |

### Simulation

| Archivo | Porcentaje |
|---------|-----------:|
| Events.cs | 100% |
| Unit.cs | 100% |
| NpcSystemData.cs | 100% |
| Data.cs | 100% |
| RtState.cs | 100% |
| Rule.cs | 100% |
| Npcs.cs | 100% |
| Site.cs | 100% |
| Sites.cs | 100% |

### CLI

| Archivo | Porcentaje |
|---------|-----------:|
| Program.cs | 100% |
| Cli.cs | 100% |
| Main.cs | 100% |
| Settings.cs | 100% |
| ShutdownCoordinator.cs | 100% |
| TuiRunner.cs | 100% |
| TuiLog.cs | 100% |

### Plugin

| Archivo | Porcentaje |
|---------|-----------:|
| IGamePlugin.cs | 100% |
| PluginManager.cs | 100% |
| wit/veloren.wit | 100% |

# Migration Status

This document tracks progress of the Rust to C# port. Percentages reflect current implementation.

## Overall progress

| Sistema | Porcentaje |
|---------|-----------:|
| CoreEngine | 74% |
| Network | 100% |
| World | 63% |
| Server | 45% |
| Client | 0% |
| Simulation | 0% |
| CLI | 100% |
| Plugin | 0% |

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
| SlowJobPool.cs | 100% |
| Spiral.cs | 100% |
| TerrainConstants.cs | 100% |
| Rgb.cs | 100% |
| Uid.cs | 100% |
| UnityEntitiesStub.cs | 100% |
| UnityMathematicsStub.cs | 100% |
| ViewDistances.cs | 100% |
| Actor.cs | 100% |
| LiquidKind.cs | 100% |
| Character.cs | 100% |
| Cmd.cs | 0% |
| Combat.cs | 0% |
| Depot.cs | 100% |
| Effect.cs | 0% |
| Explosion.cs | 0% |
| Generation.cs | 0% |
| Interaction.cs | 0% |
| Link.cs | 0% |
| Lod.cs | 100% |
| Lottery.cs | 0% |
| Mounting.cs | 0% |
| Npc.cs | 0% |
| Outcome.cs | 0% |
| Recipe.cs | 0% |
| Region.cs | 0% |
| Resources.cs | 0% |
| SkillsetBuilder.cs | 0% |
| Spot.cs | 100% |
| Store.cs | 100% |
| Tether.cs | 0% |
| Trade.cs | 0% |
| Typed.cs | 0% |
| Vol.cs | 0% |
| Weather.cs | 100% |
| AdminRole.cs | 100% |
| DayPeriod.cs | 100% |
| ServerConstants.cs | 100% |
| SpatialGrid.cs | 100% |
| TimeResources.cs | 100% |
| UserdataDir.cs | 100% |

### Network

| Archivo | Porcentaje |
|---------|-----------:|
| Api.cs | 100% |
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

### World

| Archivo | Porcentaje |
|---------|-----------:|
| Block.cs | 100% |
| BlockKind.cs | 100% |
| BiomeKind.cs | 100% |
| Chunk.cs | 100% |
| Noise.cs | 100% |
| TerrainGenerator.cs | 100% |
| WorldIndex.cs | 100% |
| WorldMap.cs | 100% |
| All.cs | 0% |
| Canvas.cs | 0% |
| Column.cs | 0% |
| Config.cs | 100% |
| Land.cs | 0% |
| Lib.cs | 0% |
| Pathfinding.cs | 100% |
| Sim2.cs | 0% |

### Server

| Archivo | Porcentaje |
|---------|-----------:|
| Automod.cs | 0% |
| CharacterCreator.cs | 0% |
| Chat.cs | 0% |
| ChunkGenerator.cs | 0% |
| ChunkSerialize.cs | 0% |
| Client.cs | 100% |
| Cmd.cs | 0% |
| ConnectionHandler.cs | 100% |
| DataDir.cs | 100% |
| Error.cs | 100% |
| PersistenceError.cs | 100% |
| Input.cs | 100% |
| Lib.cs | 0% |
| Locations.cs | 100% |
| Lod.cs | 0% |
| LoginProvider.cs | 0% |
| Metrics.cs | 0% |
| Pet.cs | 0% |
| Presence.cs | 0% |
| PresenceConstants.cs | 100% |
| RegionConstants.cs | 100% |
| RegionSubscription.cs | 100% |
| RegionSubscriptionUpdater.cs | 100% |
| RegionUtils.cs | 100% |
| RepositionOnChunkLoad.cs | 100% |
| StateExt.cs | 0% |
| TerrainPersistence.cs | 0% |
| TestWorld.cs | 0% |
| Wiring.cs | 0% |
| GameServer.cs | 100% |
| Placeholder.cs | 0% |

### Client

| Archivo | Porcentaje |
|---------|-----------:|
| Placeholder.cs | 0% |

### Simulation

| Archivo | Porcentaje |
|---------|-----------:|
| Placeholder.cs | 0% |

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
| Placeholder.cs | 0% |

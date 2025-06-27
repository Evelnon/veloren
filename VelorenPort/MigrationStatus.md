# Migration Status

This document tracks progress of the Rust to C# port. Percentages reflect current implementation.

## Overall progress

| Sistema | Porcentaje |
|---------|-----------:|
| CoreEngine | 95% |

| Network | 100% |
| World | 98% |
| Server | 68% |
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
| Combat.cs | 80% |
| Depot.cs | 100% |
| Effect.cs | 100% |
| Explosion.cs | 100% |
| Generation.cs | 70% |
| Interaction.cs | 80% |
| Link.cs | 100% |
| Lod.cs | 100% |
| Lottery.cs | 100% |
| Mounting.cs | 80% |
| Npc.cs | 80% |
| Outcome.cs | 100% |
| Recipe.cs | 90% |
| Region.cs | 100% |
| SkillsetBuilder.cs | 80% |
| Resources.cs | 100% |
| SharedServerConfig.cs | 100% |
| Spot.cs | 100% |
| Store.cs | 100% |
| Tether.cs | 100% |
| TradePricing.cs | 100% |
| Trade.cs | 100% |
| Typed.cs | 80% |
| Vol.cs | 90% |
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
| TerrainChunkSize.cs | 100% |
| MapSizeLg.cs | 100% |
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
| Economy.cs | 90% |
| Site/Economy/Cache.cs | 100% |
| Site/Economy/GoodIndex.cs | 100% |
| Site/Economy/GoodMap.cs | 100% |
| Site/Site.cs | 100% |
| SimChunk.cs | 100% |
| WorldSim.cs | 80% |
| RegionInfo.cs | 100% |
| Sim/Way.cs | 100% |
| Sim/River.cs | 100% |
| Lib.cs | 85% |
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
| Lib.cs | 0% |
| Locations.cs | 100% |
| Lod.cs | 100% |
| LoginProvider.cs | 0% |
| Metrics.cs | 100% |
| Pet.cs | 0% |
| PreparedMsg.cs | 100% |
| Presence.cs | 100% |
| PresenceConstants.cs | 100% |
| RegionConstants.cs | 100% |
| RegionSubscription.cs | 100% |
| RegionSubscriptionUpdater.cs | 100% |
| RegionUtils.cs | 100% |
| RepositionOnChunkLoad.cs | 100% |
| StateExt.cs | 0% |
| TerrainPersistence.cs | 0% |
| TestWorld.cs | 100% |
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

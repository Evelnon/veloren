using VelorenPort.CoreEngine;
using VelorenPort.NativeMath;

namespace VelorenPort.Server.Events;

/// <summary>
/// Placeholder definitions for many server events from the Rust codebase.
/// Only a few carry data at the moment. The rest are empty markers until
/// their associated systems are ported.
/// </summary>
public readonly record struct ClientDisconnectEvent;
public readonly record struct ClientDisconnectWithoutPersistenceEvent;
public readonly record struct CommandEvent;
public readonly record struct CreateSpecialEntityEvent;
public readonly record struct CreateNpcEvent;
public readonly record struct CreateShipEvent;
public readonly record struct CreateObjectEvent;
public readonly record struct ExplosionEvent;
public readonly record struct BonkEvent;
public readonly record struct HealthChangeEvent;
public readonly record struct KillEvent;
public readonly record struct HelpDownedEvent;
public readonly record struct DownedEvent;
public readonly record struct PoiseChangeEvent;
public readonly record struct DeleteEvent;
public readonly record struct DestroyEvent;
public readonly record struct InventoryManipEvent;
public readonly record struct GroupManipEvent;
public readonly record struct RespawnEvent;
public readonly record struct ShootEvent;
public readonly record struct ThrowEvent;
public readonly record struct ShockwaveEvent;
public readonly record struct KnockbackEvent;
public readonly record struct LandOnGroundEvent;
public readonly record struct SetLanternEvent;
public readonly record struct NpcInteractEvent;
public readonly record struct DialogueEvent;
public readonly record struct InviteResponseEvent;
public readonly record struct InitiateInviteEvent;
public readonly record struct ProcessTradeActionEvent;
public readonly record struct MountEvent;
public readonly record struct SetPetStayEvent;
public readonly record struct PossessEvent;
public readonly record struct InitializeCharacterEvent;
public readonly record struct InitializeSpectatorEvent;
public readonly record struct UpdateCharacterDataEvent;
public readonly record struct ExitIngameEvent;
public readonly record struct AuraEvent;
public readonly record struct BuffEvent;
public readonly record struct EnergyChangeEvent;
public readonly record struct ComboChangeEvent;
public readonly record struct ParryHookEvent;
public readonly record struct RequestSiteInfoEvent;
public readonly record struct MineBlockEvent;
public readonly record struct TeleportToEvent;
public readonly record struct SoundEvent;
public readonly record struct CreateSpriteEvent;
public readonly record struct TamePetEvent;
public readonly record struct EntityAttackedHookEvent;
public readonly record struct ChangeAbilityEvent;
public readonly record struct UpdateMapMarkerEvent;
public readonly record struct MakeAdminEvent;
public readonly record struct DeleteCharacterEvent;
public readonly record struct ChangeStanceEvent;
public readonly record struct ChangeBodyEvent;
public readonly record struct RemoveLightEmitterEvent;
public readonly record struct StartTeleportingEvent;
public readonly record struct ToggleSpriteLightEvent;
public readonly record struct TransformEvent;
public readonly record struct StartInteractionEvent;
public readonly record struct RequestPluginsEvent;
public readonly record struct CreateAuraEntityEvent;
public readonly record struct RegrowHeadEvent;
public readonly record struct SetBattleModeEvent;

/// <summary>Event used by teleporters to move an entity instantly.</summary>
public readonly record struct TeleportToPositionEvent(Uid Entity, float3 Position);

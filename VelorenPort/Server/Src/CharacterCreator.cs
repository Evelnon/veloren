using System;
using System.Collections.Generic;
using Unity.Entities;
using VelorenPort.CoreEngine.comp;
using VelorenPort.CoreEngine;

namespace VelorenPort.Server {
    /// <summary>
    /// Utilities for constructing and editing characters. Mirrors
    /// <c>server/src/character_creator.rs</c>.
    /// </summary>
    public static class CharacterCreator {
        private static readonly string?[][] VALID_STARTER_ITEMS = {
            new string?[] { null, null },
            new string?[] { "common.items.weapons.hammer.starter_hammer", null },
            new string?[] { "common.items.weapons.bow.starter", null },
            new string?[] { "common.items.weapons.axe.starter_axe", null },
            new string?[] { "common.items.weapons.staff.starter_staff", null },
            new string?[] { "common.items.weapons.sword.starter", null },
            new string?[] { "common.items.weapons.sword_1h.starter", "common.items.weapons.sword_1h.starter" },
        };

        public static CreationError? CreateCharacter(
            Entity entity,
            string playerUuid,
            string characterAlias,
            string? characterMainhand,
            string? characterOffhand,
            Body body,
            bool hardcore,
            CharacterUpdater characterUpdater,
            Persistence.CharacterLoader? characterLoader,
            Waypoint? waypoint)
        {
            if (!body.IsHumanoid())
                return CreationError.InvalidBody;

            bool valid = false;
            foreach (var pair in VALID_STARTER_ITEMS) {
                if (pair[0] == characterMainhand && pair[1] == characterOffhand) { valid = true; break; }
            }
            if (!valid)
                return CreationError.InvalidWeapon;

            var loadout = LoadoutBuilder.Empty()
                .Defaults()
                .ActiveMainhand(characterMainhand != null ? Item.NewFromAssetExpect(characterMainhand) : null)
                .ActiveOffhand(characterOffhand != null ? Item.NewFromAssetExpect(characterOffhand) : null)
                .Build();
            var inventory = Inventory.WithLoadoutHumanoid(loadout);

            var stats = new Stats(Content.Plain(characterAlias), body);
            var skillSet = SkillSet.Default();

            inventory.Push(Item.NewFromAssetExpect("common.items.consumable.potion_minor"))
                .Expect("Inventory has at least 2 slots left!");
            inventory.Push(Item.NewFromAssetExpect("common.items.food.cheese"))
                .Expect("Inventory has at least 1 slot left!");
            inventory.PushRecipeGroup(Item.NewFromAssetExpect("common.items.recipes.default"))
                .Expect("New inventory should not already have default recipe group.");

            MapMarker? mapMarker = null;

            var id = characterUpdater.CreateCharacter(entity, playerUuid, characterAlias, new PersistedComponents {
                Body = body,
                Hardcore = hardcore ? new Hardcore() : null,
                Stats = stats,
                SkillSet = skillSet,
                Inventory = inventory,
                Waypoint = waypoint,
                Pets = new List<Pet>(),
                ActiveAbilities = ActiveAbilities.DefaultLimited(Player.BASE_ABILITY_LIMIT),
                MapMarker = mapMarker,
            });
            characterLoader?.AddCharacter(playerUuid, id);
            return null;
        }

        public static CreationError? EditCharacter(
            Entity entity,
            string playerUuid,
            CharacterId id,
            string characterAlias,
            Body body,
            CharacterUpdater characterUpdater)
        {
            if (!body.IsHumanoid())
                return CreationError.InvalidBody;

            characterUpdater.EditCharacter(entity, playerUuid, id, characterAlias, body);
            return null;
        }
    }

    public enum CreationError {
        InvalidWeapon,
        InvalidBody,
    }

    public static class CreationErrorExt {
        public static string Message(this CreationError err) => err switch {
            CreationError.InvalidWeapon => "Invalid weapon.\nServer and client might be partially incompatible.",
            CreationError.InvalidBody => "Invalid Body.\nServer and client might be partially incompatible",
            _ => err.ToString(),
        };
    }
}

using System;
using Unity.Entities;

namespace VelorenPort.Server {
    /// <summary>
    /// Basic pet management utilities. This mirrors the intent of
    /// <c>server/src/pet.rs</c> from the Rust codebase. The implementation
    /// keeps state locally until the ECS based systems are fully migrated.
    /// </summary>
    public static class Pet {
        /// <summary>
        /// Restore a pet after loading it from persistence.
        /// </summary>
        public static void RestorePet(Entity petEntity, Entity ownerEntity, PetData pet) {
            TamePetInternal(petEntity, ownerEntity, pet);
        }

        /// <summary>
        /// Tame a wild pet and assign it to the owner.
        /// </summary>
        public static void TamePet(Entity petEntity, Entity ownerEntity) {
            TamePetInternal(petEntity, ownerEntity, null);
        }

        private static void TamePetInternal(Entity petEntity, Entity ownerEntity, PetData? pet) {
            if (!_ownerPets.TryGetValue(ownerEntity, out var list)) {
                list = new System.Collections.Generic.List<Entity>();
                _ownerPets[ownerEntity] = list;
            }
            if (!list.Contains(petEntity))
                list.Add(petEntity);

            _petData[petEntity] = pet ?? new PetData("Pet", 1);
        }

        private static readonly System.Collections.Generic.Dictionary<Entity, PetData> _petData = new();
        private static readonly System.Collections.Generic.Dictionary<Entity, System.Collections.Generic.List<Entity>> _ownerPets = new();

        public static bool TryGetPetData(Entity petEntity, out PetData data) => _petData.TryGetValue(petEntity, out data);

        /// <summary>
        /// Enumerate all pets along with their owners. Used by systems that
        /// operate on tamed pets without exposing the internal dictionaries.
        /// </summary>
        public static System.Collections.Generic.IEnumerable<(Entity Owner, Entity Pet)> EnumeratePets()
        {
            foreach (var (owner, list) in _ownerPets)
                foreach (var pet in list)
                    yield return (owner, pet);
        }
    }

    /// <summary>
    /// Pet data stored for each tamed creature.
    /// </summary>
    public readonly struct PetData {
        public string Name { get; }
        public int Level { get; }
        public DateTime LastFed { get; }

        public PetData(string name, int level, DateTime? lastFed = null) {
            Name = name;
            Level = level;
            LastFed = lastFed ?? DateTime.UtcNow;
        }
    }
}

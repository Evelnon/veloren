using System;
using System.Collections.Generic;
using System.Linq;
using VelorenPort.CoreEngine;
using VelorenPort.NativeMath;

namespace VelorenPort.World.Civ
{
    internal readonly struct ProximitySpec
    {
        public int2 Location { get; }
        public int? MinDistance { get; }
        public int? MaxDistance { get; }

        public ProximitySpec(int2 location, int? minDistance, int? maxDistance)
        {
            Location = location;
            MinDistance = minDistance;
            MaxDistance = maxDistance;
        }

        public bool SatisfiedBy(int2 site)
        {
            var diff = site - Location;
            var distSq = diff.x * diff.x + diff.y * diff.y;
            bool minOk = !MinDistance.HasValue || distSq > MinDistance.Value * MinDistance.Value;
            bool maxOk = !MaxDistance.HasValue || distSq < MaxDistance.Value * MaxDistance.Value;
            return minOk && maxOk;
        }

        public static ProximitySpec Avoid(int2 location, int minDistance) =>
            new ProximitySpec(location, minDistance, null);

        public static ProximitySpec BeNear(int2 location, int maxDistance) =>
            new ProximitySpec(location, null, maxDistance);
    }

    internal class ProximityRequirementsBuilder
    {
        private readonly List<ProximitySpec> _allOf = new();
        private readonly List<ProximitySpec> _anyOf = new();

        public ProximityRequirementsBuilder AvoidAllOf(IEnumerable<int2> locations, int distance)
        {
            foreach (var l in locations)
                _allOf.Add(ProximitySpec.Avoid(l, distance));
            return this;
        }

        public ProximityRequirementsBuilder CloseToOneOf(IEnumerable<int2> locations, int distance)
        {
            foreach (var l in locations)
                _anyOf.Add(ProximitySpec.BeNear(l, distance));
            return this;
        }

        public ProximityRequirements Finalize(Aabr worldDims)
        {
            var hint = LocationHint(worldDims);
            return new ProximityRequirements(_allOf.ToList(), _anyOf.ToList(), hint);
        }

        private static Aabr BoundingBox(int2 point, int maxDistance)
        {
            return new Aabr(point - maxDistance, point + maxDistance);
        }

        private Aabr LocationHint(Aabr worldDims)
        {
            Aabr? anyOfHint = null;
            foreach (var spec in _anyOf)
            {
                if (spec.MaxDistance.HasValue)
                {
                    var box = BoundingBox(spec.Location, spec.MaxDistance.Value);
                    anyOfHint = anyOfHint.HasValue ? anyOfHint.Value.Union(box) : box;
                }
            }
            var hint = anyOfHint.HasValue ? anyOfHint.Value.Intersection(worldDims) : worldDims;

            foreach (var spec in _allOf)
            {
                if (spec.MaxDistance.HasValue)
                {
                    var box = BoundingBox(spec.Location, spec.MaxDistance.Value);
                    hint = hint.Intersection(box);
                }
            }
            return hint;
        }
    }

    internal class ProximityRequirements
    {
        public IReadOnlyList<ProximitySpec> AllOf { get; }
        public IReadOnlyList<ProximitySpec> AnyOf { get; }
        public Aabr LocationHint { get; }

        public ProximityRequirements(List<ProximitySpec> allOf, List<ProximitySpec> anyOf, Aabr hint)
        {
            AllOf = allOf;
            AnyOf = anyOf;
            LocationHint = hint;
        }

        public bool SatisfiedBy(int2 site)
        {
            if (!LocationHint.ContainsPoint(site))
                return false;

            bool allOk = AllOf.All(s => s.SatisfiedBy(site));
            bool anyOk = AnyOf.Count == 0 || AnyOf.Any(s => s.SatisfiedBy(site));
            return allOk && anyOk;
        }
    }
}
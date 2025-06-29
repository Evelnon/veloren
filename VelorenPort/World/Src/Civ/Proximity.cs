using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using VelorenPort.CoreEngine;

namespace VelorenPort.World.Civ
{
    /// <summary>
    /// Proximity specification for site placement.
    /// </summary>
    public readonly record struct ProximitySpec(int2 Location, int? MinDistance, int? MaxDistance)
    {
        public bool SatisfiedBy(int2 site)
        {
            int2 diff = site - Location;
            int distSq = diff.x * diff.x + diff.y * diff.y;
            bool minOk = !MinDistance.HasValue || distSq > MinDistance.Value * MinDistance.Value;
            bool maxOk = !MaxDistance.HasValue || distSq < MaxDistance.Value * MaxDistance.Value;
            return minOk && maxOk;
        }

        public static ProximitySpec Avoid(int2 location, int minDistance) =>
            new(location, minDistance, null);

        public static ProximitySpec BeNear(int2 location, int maxDistance) =>
            new(location, null, maxDistance);
    }

    /// <summary>
    /// Builder for a set of proximity requirements.
    /// </summary>
    public class ProximityRequirementsBuilder
    {
        private readonly List<ProximitySpec> _allOf = new();
        private readonly List<ProximitySpec> _anyOf = new();

        public ProximityRequirementsBuilder AvoidAllOf(IEnumerable<int2> locations, int distance)
        {
            foreach (var loc in locations)
                _allOf.Add(ProximitySpec.Avoid(loc, distance));
            return this;
        }

        public ProximityRequirementsBuilder CloseToOneOf(IEnumerable<int2> locations, int distance)
        {
            foreach (var loc in locations)
                _anyOf.Add(ProximitySpec.BeNear(loc, distance));
            return this;
        }

        public ProximityRequirements Finalize(Aabr worldDims)
        {
            var hint = LocationHint(worldDims);
            return new ProximityRequirements(_allOf, _anyOf, hint);
        }

        private Aabr LocationHint(Aabr worldDims)
        {
            static Aabr BoxOfPoint(int2 point, int maxDist) =>
                new Aabr(point - new int2(maxDist, maxDist), point + new int2(maxDist, maxDist));

            Aabr? anyHint = null;
            foreach (var spec in _anyOf)
            {
                if (spec.MaxDistance.HasValue)
                {
                    var box = BoxOfPoint(spec.Location, spec.MaxDistance.Value);
                    anyHint = anyHint.HasValue ? anyHint.Value.Union(box) : box;
                }
            }
            var anyFinal = anyHint.HasValue ? anyHint.Value.Intersection(worldDims) : worldDims;
            var hint = anyFinal;
            foreach (var spec in _allOf)
            {
                if (spec.MaxDistance.HasValue)
                {
                    var box = BoxOfPoint(spec.Location, spec.MaxDistance.Value);
                    hint = hint.Intersection(box);
                }
            }
            return hint;
        }
    }

    /// <summary>
    /// Final set of proximity requirements.
    /// </summary>
    public class ProximityRequirements
    {
        private readonly List<ProximitySpec> _allOf;
        private readonly List<ProximitySpec> _anyOf;
        public Aabr LocationHint { get; }

        public ProximityRequirements(List<ProximitySpec> allOf, List<ProximitySpec> anyOf, Aabr hint)
        {
            _allOf = allOf;
            _anyOf = anyOf;
            LocationHint = hint;
        }

        public bool SatisfiedBy(int2 site)
        {
            if (!LocationHint.Contains(site))
                return false;
            bool all = _allOf.All(s => s.SatisfiedBy(site));
            bool any = _anyOf.Count == 0 || _anyOf.Any(s => s.SatisfiedBy(site));
            return all && any;
        }
    }
}

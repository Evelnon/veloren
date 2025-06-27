using System;
using System.Collections.Generic;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Utility to assemble a set of skills for an entity.
    /// </summary>
    [Serializable]
    public class SkillsetBuilder {
        private readonly HashSet<string> _skills = new();

        public void AddSkill(string skill) => _skills.Add(skill);

        public void RemoveSkill(string skill) => _skills.Remove(skill);

        public bool HasSkill(string skill) => _skills.Contains(skill);

        public IReadOnlyCollection<string> Skills => _skills;

        /// <summary>Produce an immutable array of all skills.</summary>
        public string[] Build() => System.Linq.Enumerable.ToArray(_skills);

        /// <summary>Add multiple skills at once.</summary>
        public void AddSkills(IEnumerable<string> skills) {
            foreach (var s in skills) _skills.Add(s);
        }
    }
}

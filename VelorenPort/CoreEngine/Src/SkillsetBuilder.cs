using System;
using System.Collections.Generic;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Utility to assemble a set of skills for an entity.
    /// </summary>
    [Serializable]
    public class SkillsetBuilder {
        private readonly Dictionary<string, int> _skills = new();

        public void AddSkill(string skill, int level = 1) => _skills[skill] = level;

        public void RemoveSkill(string skill) => _skills.Remove(skill);

        public bool HasSkill(string skill) => _skills.ContainsKey(skill);

        public int GetLevel(string skill) => _skills.TryGetValue(skill, out var l) ? l : 0;

        public IReadOnlyCollection<string> Skills => _skills.Keys;

        /// <summary>Produce an immutable array of all skills.</summary>
        public string[] Build() => System.Linq.Enumerable.ToArray(_skills.Keys);

        /// <summary>Add multiple skills at once.</summary>
        public void AddSkills(IEnumerable<string> skills, int level = 1) {
            foreach (var s in skills) _skills[s] = level;
        }
    }
}

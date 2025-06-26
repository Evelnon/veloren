using System;
using System.Collections.Generic;
using System.Linq;

namespace VelorenPort.CoreEngine {
    [Serializable]
    public class MaterialUse {
        public List<(float Amount, Good Material)> Values { get; } = new();

        public MaterialUse() {}
        public MaterialUse(IEnumerable<(float Amount, Good Material)> values) {
            Values.AddRange(values);
        }

        public static MaterialUse operator *(MaterialUse self, float scalar) {
            return new MaterialUse(self.Values.Select(v => (v.Amount * scalar, v.Material)));
        }

        internal static void VectorAddEq(List<(float Amount, Good Material)> target, IEnumerable<(float Amount, Good Material)> rhs) {
            foreach (var (amount, mat) in rhs) {
                var idx = target.FindIndex(t => EqualityComparer<Good>.Default.Equals(t.Material, mat));
                if (idx >= 0) {
                    var (a, m) = target[idx];
                    target[idx] = (a + amount, m);
                } else {
                    target.Add((amount, mat));
                }
            }
        }

        public static MaterialUse operator +(MaterialUse a, MaterialUse b) {
            var res = new MaterialUse(a.Values);
            VectorAddEq(res.Values, b.Values);
            return res;
        }

        public void AddAssign(MaterialUse other) {
            VectorAddEq(Values, other.Values);
        }
    }
}

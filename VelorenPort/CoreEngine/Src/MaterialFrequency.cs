using System;
using System.Collections.Generic;
using System.Linq;

namespace VelorenPort.CoreEngine {
    [Serializable]
    public class MaterialFrequency {
        public List<(float Amount, Good Material)> Values { get; } = new();

        public MaterialFrequency() {}
        public MaterialFrequency(IEnumerable<(float Amount, Good Material)> vals) {
            Values.AddRange(vals);
        }

        public static MaterialFrequency FromMaterialUse(MaterialUse use) {
            var list = use.Values.Select(v => (v.Amount, v.Material)).ToList();
            VectorInvert(list);
            return new MaterialFrequency(list);
        }

        public MaterialUse ToMaterialUse() {
            var list = Values.Select(v => (v.Amount, v.Material)).ToList();
            VectorInvert(list);
            return new MaterialUse(list);
        }

        private static void VectorInvert(List<(float Amount, Good Material)> list) {
            float oldSum = 0f;
            float newSum = 0f;
            for (int i = 0; i < list.Count; i++) {
                var (v, g) = list[i];
                oldSum += v;
                v = 1f / v;
                list[i] = (v, g);
                newSum += v;
            }
            float scale = 1f / (oldSum * newSum);
            for (int i = 0; i < list.Count; i++) {
                var (v, g) = list[i];
                list[i] = (v * scale, g);
            }
        }

        public static MaterialFrequency operator +(MaterialFrequency a, MaterialFrequency b) {
            var res = new MaterialFrequency(a.Values);
            MaterialUse.VectorAddEq(res.Values, b.Values);
            return res;
        }
    }
}

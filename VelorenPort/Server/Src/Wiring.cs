using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;

namespace VelorenPort.Server {
    /// <summary>
    /// Simplified wiring system definitions mirrored from <c>server/src/wiring.rs</c>.
    /// Only a subset of functionality is currently required by the C# port.
    /// </summary>
    public enum LogicKind { Min, Max, Sub, Sum, Mul }

    public class Logic {
        public LogicKind Kind { get; }
        public OutputFormula Left { get; }
        public OutputFormula Right { get; }
        public Logic(LogicKind kind, OutputFormula left, OutputFormula right) {
            Kind = kind; Left = left; Right = right;
        }
    }

    public abstract class OutputFormula {
        public static OutputFormula Constant(float value) => new ConstantFormula(value);
        public static OutputFormula Input(string name) => new InputFormula(name);

        private sealed class ConstantFormula : OutputFormula { public float Value; public ConstantFormula(float v){Value=v;} }
        private sealed class InputFormula : OutputFormula { public string Name; public InputFormula(string n){Name=n;} }
        private sealed class LogicFormula : OutputFormula { public Logic Logic; public LogicFormula(Logic l){Logic=l;} }

        public float Compute(Dictionary<string,float> inputs) {
            return this switch {
                ConstantFormula c => c.Value,
                InputFormula i => inputs.TryGetValue(i.Name, out var v) ? v : 0f,
                LogicFormula l => l.Logic.Kind switch {
                    LogicKind.Min => math.min(l.Logic.Left.Compute(inputs), l.Logic.Right.Compute(inputs)),
                    LogicKind.Max => math.max(l.Logic.Left.Compute(inputs), l.Logic.Right.Compute(inputs)),
                    LogicKind.Sub => l.Logic.Left.Compute(inputs) - l.Logic.Right.Compute(inputs),
                    LogicKind.Sum => l.Logic.Left.Compute(inputs) + l.Logic.Right.Compute(inputs),
                    LogicKind.Mul => l.Logic.Left.Compute(inputs) * l.Logic.Right.Compute(inputs),
                    _ => 0f
                },
                _ => 0f
            };
        }

        public static OutputFormula Logic(Logic logic) => new LogicFormula(logic);
    }

    public class WiringAction {
        public OutputFormula Formula { get; }
        public float Threshold { get; }
        public List<WiringActionEffect> Effects { get; }
        public WiringAction(OutputFormula formula, float threshold, List<WiringActionEffect> effects) {
            Formula = formula; Threshold = threshold; Effects = effects; }
    }

    public enum WiringActionEffectType { SpawnProjectile, SetBlock, SetLight }
    public class WiringActionEffect {
        public WiringActionEffectType Type { get; }
        public float3 Coords { get; }
        public WiringActionEffect(WiringActionEffectType type, float3 coords) {
            Type = type; Coords = coords; }
    }

    public class WiringElement : IComponentData {
        public Dictionary<string,float> Inputs { get; } = new();
        public Dictionary<string,OutputFormula> Outputs { get; } = new();
        public List<WiringAction> Actions { get; } = new();

        /// <summary>
        /// Evaluates this wiring element using its current inputs and returns
        /// the computed output values. Side effects of any actions are applied
        /// immediately.
        /// </summary>
        public Dictionary<string,float> Evaluate(
            Dictionary<Entity,WiringElement> context)
        {
            var values = new Dictionary<string,float>(Inputs);
            foreach (var (name, formula) in Outputs)
                values[name] = formula.Compute(values);

            foreach (var action in Actions)
            {
                float val = action.Formula.Compute(values);
                if (val >= action.Threshold)
                    ApplyEffects(action.Effects, context);
            }

            return values;
        }

        private static void ApplyEffects(IEnumerable<WiringActionEffect> effects,
                                          Dictionary<Entity,WiringElement> ctx) {
            foreach (var eff in effects) {
                // Only logs for now; real behaviour would mutate the world
                UnityEngine.Debug.Log($"Wiring effect {eff.Type} at {eff.Coords}");
            }
        }
    }

    public class Circuit : IComponentData { public List<Wire> Wires { get; } = new(); }

    public struct WireNode { public Entity Entity; public string Name; public WireNode(Entity e,string n){Entity=e;Name=n;} }
    public struct Wire { public WireNode Input; public WireNode Output; public Wire(WireNode i,WireNode o){Input=i;Output=o;} }
}

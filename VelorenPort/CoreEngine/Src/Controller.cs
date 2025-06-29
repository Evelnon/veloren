using System;
using System.Collections.Generic;
using VelorenPort.NativeMath;

namespace VelorenPort.CoreEngine
{
    /// <summary>
    /// Lightweight controller storing queued actions.
    /// This is a trimmed down port of common::comp::controller.
    /// </summary>
    [Serializable]
    public class Controller
    {
        public ControllerInputs Inputs;
        public Dictionary<InputKind, InputAttr> QueuedInputs { get; } = new();
        public List<ControlAction> Actions { get; } = new();

        public void Reset()
        {
            Inputs = default;
            QueuedInputs.Clear();
        }

        public void PushAction(ControlAction action) => Actions.Add(action);
        public void PushBasicInput(InputKind input) => Actions.Add(ControlAction.Start(input));
        public void PushCancelInput(InputKind input) => Actions.Add(ControlAction.Cancel(input));
        public void ClearActions() => Actions.Clear();
    }

    [Serializable]
    public readonly struct ControlAction
    {
        public enum Kind { StartInput, CancelInput }
        public Kind ActionKind { get; }
        public InputKind Input { get; }
        public Uid? TargetEntity { get; }
        public float3? SelectPos { get; }

        private ControlAction(Kind kind, InputKind input, Uid? target, float3? selectPos)
        {
            ActionKind = kind;
            Input = input;
            TargetEntity = target;
            SelectPos = selectPos;
        }

        public static ControlAction Start(InputKind input, Uid? target = null, float3? selectPos = null)
            => new(Kind.StartInput, input, target, selectPos);
        public static ControlAction Cancel(InputKind input)
            => new(Kind.CancelInput, input, null, null);
    }
}

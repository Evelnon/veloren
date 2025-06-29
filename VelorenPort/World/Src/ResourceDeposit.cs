using System;
using VelorenPort.NativeMath;

namespace VelorenPort.World
{
    /// <summary>
    /// Information about a resource block generated within a chunk.
    /// </summary>
    [Serializable]
    public struct ResourceDeposit
    {
        public int3 Position { get; set; }
        public BlockKind Kind { get; set; }
        public bool Depleted { get; private set; }
        public float Amount { get; private set; }

        public ResourceDeposit(int3 position, BlockKind kind, float amount = 10f)
        {
            Position = position;
            Kind = kind;
            Depleted = false;
            Amount = amount;
        }

        public void MarkDepleted()
        {
            Depleted = true;
            Amount = 0f;
        }

        public void Produce(float amt) => Amount += amt;

        public void Consume(float amt)
        {
            if (Depleted) return;
            Amount = Math.Max(0f, Amount - amt);
            if (Amount <= 0f)
                MarkDepleted();
        }
    }
}

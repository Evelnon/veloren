using System;

namespace VelorenPort.CoreEngine {
    [Serializable]
    public struct InvSlotId : IEquatable<InvSlotId>, IComparable<InvSlotId> {
        private readonly ushort _loadoutIdx;
        private readonly ushort _slotIdx;

        public InvSlotId(ushort loadoutIdx, ushort slotIdx) {
            _loadoutIdx = loadoutIdx;
            _slotIdx = slotIdx;
        }

        public uint Idx => ((uint)_loadoutIdx << 16) | _slotIdx;
        public int LoadoutIdx => _loadoutIdx;
        public int SlotIdx => _slotIdx;

        public bool Equals(InvSlotId other) => _loadoutIdx == other._loadoutIdx && _slotIdx == other._slotIdx;
        public int CompareTo(InvSlotId other) => Idx.CompareTo(other.Idx);
        public override bool Equals(object obj) => obj is InvSlotId other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(_loadoutIdx, _slotIdx);
        public override string ToString() => $"{_loadoutIdx}:{_slotIdx}";
    }
}

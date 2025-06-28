namespace VelorenPort.CoreEngine.comp
{
    public static class BuffKindExtensions
    {
        /// <summary>
        /// Determines whether the given kind represents a positive buff.
        /// In the enum definition all buffs precede the debuff variants so a
        /// simple range check is sufficient.
        /// </summary>
        public static bool IsBuff(this BuffKind kind) => kind < BuffKind.Burning;

        /// <summary>Returns true if the value represents a debuff.</summary>
        public static bool IsDebuff(this BuffKind kind) =>
            kind >= BuffKind.Burning && kind < BuffKind.Polymorphed;
    }
}

namespace VelorenPort.Server
{
    /// <summary>
    /// Constants used by region tracking and subscriptions.
    /// </summary>
    public static class RegionConstants
    {
        /// <summary>
        /// Bitshift between region and world coordinates. Region size is
        /// <c>1 &lt;&lt; RegionLog2</c> blocks.
        /// </summary>
        public const int RegionLog2 = 9;

        /// <summary>
        /// Size of a region in blocks.
        /// </summary>
        public const int RegionSize = 1 << RegionLog2;

        /// <summary>
        /// Distance an entity may move outside its region before switching.
        /// </summary>
        public const uint TetherLength = 16;
    }
}

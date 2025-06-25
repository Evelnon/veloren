namespace VelorenPort.Server
{
    /// <summary>
    /// Component indicating that an entity should be repositioned once
    /// its target chunk has finished loading.
    /// </summary>
    public struct RepositionOnChunkLoad
    {
        /// <summary>
        /// Whether the entity should be moved to ground level when
        /// repositioned.
        /// </summary>
        public bool NeedsGround { get; set; }
    }
}

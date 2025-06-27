namespace VelorenPort.Simulation {
    /// <summary>
    /// Marker interface for rtsim rules. Concrete rule implementations
    /// are expected to register their event handlers during <see cref="Start"/>.
    /// </summary>
    public interface IRule
    {
        /// <summary>
        /// Called when the rule is started by <see cref="RtState"/>. Rules
        /// should bind all event handlers here and perform any required
        /// initialisation.
        /// </summary>
        /// <param name="state">The simulation state starting this rule.</param>
        void Start(RtState state);
    }

    /// <summary>
    /// Errors that may occur while starting a rule.
    /// Currently only used for parity with the Rust implementation.
    /// </summary>
    public enum RuleError
    {
        /// <summary>No specific error.</summary>
        None,

        /// <summary>The requested rule does not exist.</summary>
        NoSuchRule,
    }
}

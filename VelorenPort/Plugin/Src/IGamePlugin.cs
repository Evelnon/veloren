using System;

namespace VelorenPort.Plugin
{
    /// <summary>
    /// Interface implemented by all game plugins. Plugins are discovered and
    /// loaded at runtime by <see cref="PluginManager"/>.
    /// </summary>
    public interface IGamePlugin
    {
        /// <summary>
        /// Name of the plugin displayed in logs or user interfaces.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Called after the plugin assembly is loaded. Use this to register
        /// callbacks or initialise state.
        /// </summary>
        void Initialize();
    }
}

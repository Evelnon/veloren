using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace VelorenPort.Plugin
{
    /// <summary>
    /// Loads plugin assemblies and instantiates types implementing
    /// <see cref="IGamePlugin"/>.
    /// </summary>
    public sealed class PluginManager
    {
        private readonly List<IGamePlugin> _plugins = new();

        /// <summary>List of loaded plugins.</summary>
        public IReadOnlyList<IGamePlugin> Plugins => _plugins;

        /// <summary>
        /// Load every plugin assembly found in <paramref name="directory"/>.
        /// </summary>
        public void LoadDirectory(string directory)
        {
            if (!Directory.Exists(directory))
                return;

            foreach (var dll in Directory.GetFiles(directory, "*.dll"))
                LoadPlugin(dll);
        }

        /// <summary>
        /// Load a single plugin from an assembly file and instantiate its
        /// <see cref="IGamePlugin"/> implementations.
        /// </summary>
        public void LoadPlugin(string assemblyPath)
        {
            var assembly = Assembly.LoadFrom(assemblyPath);
            foreach (var plugin in CreatePlugins(assembly))
            {
                _plugins.Add(plugin);
                try { plugin.Initialize(); } catch (Exception e)
                {
                    UnityEngine.Debug.Log($"[PluginManager] Failed to initialise {plugin.Name}: {e.Message}");
                }
            }
        }

        private static IEnumerable<IGamePlugin> CreatePlugins(Assembly assembly)
        {
            return assembly
                .GetTypes()
                .Where(t => typeof(IGamePlugin).IsAssignableFrom(t) && !t.IsAbstract)
                .Select(t => Activator.CreateInstance(t) as IGamePlugin)
                .Where(p => p != null)!;
        }
    }
}

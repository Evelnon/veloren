using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace VelorenPort.World;

/// <summary>
/// Utility for loading <see cref="SpotProperties"/> from JSON files.
/// Each file name must match a value of <see cref="Spot"/>.
/// </summary>
[Serializable]
public class SpotManifest
{
    public Dictionary<Spot, SpotProperties> Spots { get; } = new();

    /// <summary>Load all JSON manifests in <paramref name="directory"/>.</summary>
    public static SpotManifest LoadFromDir(string directory)
    {
        var manifest = new SpotManifest();
        if (!Directory.Exists(directory))
            return manifest;

        foreach (var file in Directory.GetFiles(directory, "*.json"))
        {
            try
            {
                string name = Path.GetFileNameWithoutExtension(file);
                if (!Enum.TryParse(name, out Spot spot))
                    continue;
                var props = JsonSerializer.Deserialize<SpotProperties>(File.ReadAllText(file));
                if (props != null)
                    manifest.Spots[spot] = props;
            }
            catch
            {
                // ignore malformed files
            }
        }
        return manifest;
    }
}

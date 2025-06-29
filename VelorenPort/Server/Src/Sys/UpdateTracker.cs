using System.Collections.Generic;
using Unity.Entities;

namespace VelorenPort.Server.Sys;

/// <summary>
/// Tracks insertions, removals and modifications of a component type on
/// entities. This is a lightweight approximation of the change detection
/// provided by the Rust sentinel system.
/// </summary>
public class UpdateTracker<T> where T : struct
{
    private HashSet<Entity> _previous = new();

    /// <summary>Entities that gained the component since the last update.</summary>
    public readonly HashSet<Entity> Inserted = new();
    /// <summary>Entities that lost the component since the last update.</summary>
    public readonly HashSet<Entity> Removed = new();
    /// <summary>Entities that still have the component and may have changed.</summary>
    public readonly HashSet<Entity> Modified = new();

    /// <summary>
    /// Compare the provided set of entities to the previous frame and update
    /// insertion, removal and modification lists.
    /// </summary>
    public void Track(HashSet<Entity> current)
    {
        Inserted.Clear();
        Removed.Clear();
        Modified.Clear();

        foreach (var e in current)
        {
            if (!_previous.Contains(e))
                Inserted.Add(e);
            else
                Modified.Add(e);
        }

        foreach (var e in _previous)
        {
            if (!current.Contains(e))
                Removed.Add(e);
        }

        _previous = current;
    }
}

using System;
using System.Collections.Generic;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Simple resource map used by systems to share data. This mirrors the
    /// generic `Resources` container from the Rust code but only implements
    /// the minimal functionality required by the ported modules.
    /// </summary>
    [Serializable]
    public class Resources {
        private readonly Dictionary<Type, object> _store = new();

        /// <summary>Insert or replace a resource instance.</summary>
        public void Insert<T>(T resource) where T : class {
            _store[typeof(T)] = resource;
        }

        /// <summary>Try to retrieve a resource by type.</summary>
        public bool TryGet<T>(out T? resource) where T : class {
            if (_store.TryGetValue(typeof(T), out var obj) && obj is T t) {
                resource = t;
                return true;
            }
            resource = null;
            return false;
        }

        /// <summary>Retrieve an existing resource or create a new one using the provided factory.</summary>
        public T GetOrCreate<T>(Func<T> factory) where T : class {
            if (!TryGet<T>(out var r) || r == null) {
                r = factory();
                Insert(r);
            }
            return r;
        }

        /// <summary>Remove a previously inserted resource.</summary>
        public void Remove<T>() where T : class {
            _store.Remove(typeof(T));
        }
    }
}

using System;
using System.Collections.Generic;

namespace VelorenPort.Simulation {
    /// <summary>
    /// Collection of sites indexed by identifier.
    /// </summary>
    [Serializable]
    public class Sites {
        private readonly Dictionary<SiteId, Site> _sites = new();
        private uint _counter;

        public SiteId Create(Site site) {
            var id = new SiteId(_counter++);
            _sites[id] = site;
            return id;
        }

        public bool Remove(SiteId id) => _sites.Remove(id);

        public Site? Get(SiteId id) => _sites.TryGetValue(id, out var site) ? site : null;

        public IEnumerable<KeyValuePair<SiteId, Site>> All => _sites;
    }
}

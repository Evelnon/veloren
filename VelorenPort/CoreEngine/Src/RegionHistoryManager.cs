using System;
using System.IO;
using System.Linq;

namespace VelorenPort.CoreEngine
{
    /// <summary>
    /// Helper for persisting region history snapshots with log rotation.
    /// </summary>
    public class RegionHistoryManager
    {
        private readonly string directory;
        private readonly int maxSnapshots;

        public RegionHistoryManager(string directory, int maxSnapshots = 5)
        {
            this.directory = directory;
            this.maxSnapshots = maxSnapshots;
            Directory.CreateDirectory(directory);
        }

        /// <summary>
        /// Save the region history to a timestamped file and remove older
        /// snapshots exceeding <see cref="maxSnapshots"/>.
        /// </summary>
        public string SaveSnapshot(Region region)
        {
            string fileName = Path.Combine(directory,
                $"region_{DateTime.UtcNow:yyyyMMddHHmmssfff}.log");
            region.SaveHistory(fileName);
            Rotate();
            return fileName;
        }

        /// <summary>
        /// Load the most recent snapshot if one exists.
        /// </summary>
        public void LoadLatest(Region region)
        {
            var latest = Directory.GetFiles(directory, "region_*.log")
                .OrderByDescending(f => f)
                .FirstOrDefault();
            if (latest != null)
                region.LoadHistory(latest);
        }

        private void Rotate()
        {
            var files = Directory.GetFiles(directory, "region_*.log")
                .OrderByDescending(f => f)
                .ToList();
            for (int i = maxSnapshots; i < files.Count; i++)
                File.Delete(files[i]);
        }
    }
}

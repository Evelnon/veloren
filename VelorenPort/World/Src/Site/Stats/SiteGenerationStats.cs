using System;
using System.Collections.Generic;
using System.IO;

namespace VelorenPort.World.Site.Stats
{
    /// <summary>
    /// Simplified statistics tracker for site generation. Mirrors a subset of
    /// `world/src/site/genstat.rs` from the Rust project. Statistics are kept
    /// in memory and can be printed to the console for debugging.
    /// </summary>
    public enum GenStatPlotKind
    {
        InitialPlaza,
        Plaza,
        Workshop,
        House,
        GuardTower,
        Castle,
        AirshipDock,
        Tavern,
        Yard,
        MultiPlot,
        Temple
    }

    public enum GenStatSiteKind
    {
        Terracotta,
        Myrmidon,
        City,
        CliffTown,
        SavannahTown,
        CoastalTown,
        DesertCity
    }

    /// <summary>Additional metrics for runtime events.</summary>
    public enum GenStatEventKind
    {
        TradeRoute,
        PopulationBirth,
        PopulationDeath,
        PlotCreated,
        DecorationPlaced,
        Warning
    }

    public class GenPlot
    {
        public uint Attempts { get; private set; }
        public uint Successful { get; private set; }

        public void Attempt() => Attempts++;
        public void Success() => Successful++;
    }

    public class GenSite
    {
        public GenStatSiteKind Kind { get; }
        public string Name { get; }
        private readonly Dictionary<GenStatPlotKind, GenPlot> _stats = new();
        private readonly Dictionary<GenStatEventKind, uint> _events = new();

        public IReadOnlyDictionary<GenStatEventKind, uint> Events => _events;
        public IReadOnlyDictionary<GenStatPlotKind, GenPlot> Stats => _stats;

        public GenSite(GenStatSiteKind kind, string name)
        {
            Kind = kind;
            Name = name;
        }

        public string Header => $"{Kind} {Name}";

        private GenPlot Get(GenStatPlotKind kind)
        {
            if (!_stats.TryGetValue(kind, out var plot))
            {
                plot = new GenPlot();
                _stats[kind] = plot;
            }
            return plot;
        }

        public void Attempt(GenStatPlotKind kind) => Get(kind).Attempt();
        public void Success(GenStatPlotKind kind) => Get(kind).Success();
        public void RecordEvent(GenStatEventKind kind)
        {
            _events.TryGetValue(kind, out var c);
            _events[kind] = c + 1;
        }

        public IEnumerable<string> FormatStats(bool verbose)
        {
            foreach (var kv in _stats)
            {
                var plot = kv.Value;
                if (verbose || plot.Successful != plot.Attempts)
                    yield return $"{kv.Key}: {plot.Successful}/{plot.Attempts}";
            }
            foreach (var ev in _events)
                yield return $"{ev.Key}: {ev.Value}";
        }

        private void AtLeast(uint count, GenStatPlotKind plotKind, GenPlot plot, List<string> errors)
        {
            if (plot.Successful < count)
                errors.Add($"  {Kind} {Name} {plotKind}: {plot.Successful}/{plot.Attempts} GenError: expected at least {count}");
        }

        private void AtMost(uint count, GenStatPlotKind plotKind, GenPlot plot, List<string> errors)
        {
            if (plot.Successful > count)
                errors.Add($"  {Kind} {Name} {plotKind}: {plot.Successful}/{plot.Attempts} GenError: expected at most {count}");
        }

        private void ShouldNotBeZero(GenStatPlotKind plotKind, GenPlot plot, List<string> warnings)
        {
            if (plot.Successful == 0)
                warnings.Add($"  {Kind} {Name} {plotKind}: {plot.Successful}/{plot.Attempts} GenWarn: should not be zero");
        }

        private void SuccessRate(float rate, GenStatPlotKind plotKind, GenPlot plot, List<string> warnings)
        {
            if ((float)plot.Successful / plot.Attempts < rate)
                warnings.Add($"  {Kind} {Name} {plotKind}: GenWarn: success rate less than {rate} ({plot.Successful}/{plot.Attempts})");
        }

        public void Validate(List<string> errors, List<string> warnings)
        {
            foreach (var kv in _stats)
            {
                var kind = kv.Key;
                var plot = kv.Value;
                switch (Kind)
                {
                    case GenStatSiteKind.Terracotta:
                        switch (kind)
                        {
                            case GenStatPlotKind.InitialPlaza:
                                AtLeast(1, kind, plot, errors);
                                break;
                            case GenStatPlotKind.Plaza:
                                ShouldNotBeZero(kind, plot, warnings);
                                break;
                            case GenStatPlotKind.House:
                                AtLeast(1, kind, plot, errors);
                                SuccessRate(0.1f, kind, plot, warnings);
                                break;
                            case GenStatPlotKind.Yard:
                                ShouldNotBeZero(kind, plot, warnings);
                                break;
                        }
                        break;
                    case GenStatSiteKind.Myrmidon:
                        switch (kind)
                        {
                            case GenStatPlotKind.InitialPlaza:
                                AtLeast(1, kind, plot, errors);
                                break;
                            case GenStatPlotKind.Plaza:
                                ShouldNotBeZero(kind, plot, warnings);
                                break;
                            case GenStatPlotKind.House:
                                AtLeast(1, kind, plot, errors);
                                SuccessRate(0.1f, kind, plot, warnings);
                                break;
                        }
                        break;
                    case GenStatSiteKind.City:
                        switch (kind)
                        {
                            case GenStatPlotKind.InitialPlaza:
                                AtLeast(1, kind, plot, errors);
                                break;
                            case GenStatPlotKind.Plaza:
                                ShouldNotBeZero(kind, plot, warnings);
                                break;
                            case GenStatPlotKind.Workshop:
                                AtLeast(1, kind, plot, errors);
                                break;
                            case GenStatPlotKind.House:
                                AtLeast(1, kind, plot, errors);
                                SuccessRate(0.2f, kind, plot, warnings);
                                break;
                        }
                        break;
                    case GenStatSiteKind.CliffTown:
                        switch (kind)
                        {
                            case GenStatPlotKind.InitialPlaza:
                                AtLeast(1, kind, plot, errors);
                                break;
                            case GenStatPlotKind.Plaza:
                                ShouldNotBeZero(kind, plot, warnings);
                                break;
                            case GenStatPlotKind.House:
                                AtLeast(5, kind, plot, errors);
                                SuccessRate(0.5f, kind, plot, warnings);
                                break;
                            case GenStatPlotKind.AirshipDock:
                                ShouldNotBeZero(kind, plot, warnings);
                                SuccessRate(0.1f, kind, plot, warnings);
                                break;
                        }
                        break;
                    case GenStatSiteKind.SavannahTown:
                        switch (kind)
                        {
                            case GenStatPlotKind.InitialPlaza:
                                AtLeast(1, kind, plot, errors);
                                break;
                            case GenStatPlotKind.Plaza:
                                ShouldNotBeZero(kind, plot, warnings);
                                break;
                            case GenStatPlotKind.Workshop:
                                AtLeast(1, kind, plot, errors);
                                break;
                            case GenStatPlotKind.House:
                                AtLeast(1, kind, plot, errors);
                                SuccessRate(0.5f, kind, plot, warnings);
                                break;
                            case GenStatPlotKind.AirshipDock:
                                ShouldNotBeZero(kind, plot, warnings);
                                break;
                        }
                        break;
                    case GenStatSiteKind.CoastalTown:
                        switch (kind)
                        {
                            case GenStatPlotKind.InitialPlaza:
                                AtLeast(1, kind, plot, errors);
                                break;
                            case GenStatPlotKind.Plaza:
                                ShouldNotBeZero(kind, plot, warnings);
                                break;
                            case GenStatPlotKind.Workshop:
                                AtLeast(1, kind, plot, errors);
                                break;
                            case GenStatPlotKind.House:
                                AtLeast(1, kind, plot, errors);
                                SuccessRate(0.5f, kind, plot, warnings);
                                break;
                            case GenStatPlotKind.AirshipDock:
                                ShouldNotBeZero(kind, plot, warnings);
                                AtMost(1, kind, plot, errors);
                                break;
                        }
                        break;
                    case GenStatSiteKind.DesertCity:
                        switch (kind)
                        {
                            case GenStatPlotKind.InitialPlaza:
                                AtLeast(1, kind, plot, errors);
                                break;
                            case GenStatPlotKind.Plaza:
                                ShouldNotBeZero(kind, plot, warnings);
                                break;
                            case GenStatPlotKind.MultiPlot:
                                AtLeast(1, kind, plot, errors);
                                break;
                            case GenStatPlotKind.Temple:
                                ShouldNotBeZero(kind, plot, warnings);
                                break;
                            case GenStatPlotKind.AirshipDock:
                                ShouldNotBeZero(kind, plot, warnings);
                                break;
                        }
                        break;
                }
            }
        }
    }

    public class SitesGenMeta
    {
        private readonly Dictionary<string, GenSite> _sites = new();
        public uint Seed { get; }
        public int SiteCount => _sites.Count;
        public IEnumerable<string> SiteNames => _sites.Keys;

        public SitesGenMeta(uint seed)
        {
            Seed = seed;
        }

        public void Add(string name, GenStatSiteKind kind)
        {
            if (!_sites.ContainsKey(name))
                _sites[name] = new GenSite(kind, name);
        }

        public void Attempt(string name, GenStatPlotKind kind)
        {
            if (_sites.TryGetValue(name, out var site))
                site.Attempt(kind);
        }

        public void Success(string name, GenStatPlotKind kind)
        {
            if (_sites.TryGetValue(name, out var site))
                site.Success(kind);
        }

        public void RecordEvent(string name, GenStatEventKind kind)
        {
            if (_sites.TryGetValue(name, out var site))
                site.RecordEvent(kind);
        }

        public uint GetEventCount(string name, GenStatEventKind kind)
        {
            if (_sites.TryGetValue(name, out var site))
                return site.Events.TryGetValue(kind, out var c) ? c : 0u;
            return 0u;
        }

        private static bool GetBoolEnvVar(string name)
        {
            var v = Environment.GetEnvironmentVariable(name);
            return !string.IsNullOrEmpty(v) && v.ToLowerInvariant() == "true";
        }

        public void Log()
        {
            bool verbose = GetBoolEnvVar("SITE_GENERATION_STATS_VERBOSE");
            string? path = Environment.GetEnvironmentVariable("SITE_GENERATION_STATS_LOG");

            var lines = new List<string> { $"---- SitesGenMeta seed {Seed}" };
            foreach (var site in _sites.Values)
            {
                lines.Add(site.Header);
                var errors = new List<string>();
                var warnings = new List<string>();
                site.Validate(errors, warnings);
                foreach (var line in site.FormatStats(verbose))
                    lines.Add($"  {line}");
                foreach (var err in errors)
                    lines.Add(err);
                if (verbose)
                    foreach (var warn in warnings)
                        lines.Add(warn);
            }

            var output = string.Join(Environment.NewLine, lines);
            Console.WriteLine(output);

            if (!string.IsNullOrEmpty(path))
            {
                File.AppendAllText(path, output + Environment.NewLine);
                Console.WriteLine($"Statistics written to {path}");
            }
        }
    }
}

using System;
using System.IO;

namespace VelorenPort.World.Site.Economy;

/// <summary>
/// Utility for long term economy simulations. Only writes a CSV of site stocks
/// similar to the Rust implementation's environment struct.
/// </summary>
public class Environment : IDisposable
{
    private readonly StreamWriter? _csv;

    public Environment(string? csvPath = null)
    {
        if (!string.IsNullOrEmpty(csvPath))
        {
            _csv = new StreamWriter(File.Open(csvPath, FileMode.Create, FileAccess.Write));
        }
    }

    public void WriteCsvHeader(string siteName)
    {
        _csv?.WriteLine($"Site,{siteName}");
    }

    public void Record(GoodMap<float> stocks)
    {
        if (_csv == null) return;
        for (int i = 0; i < GoodIndex.LENGTH; i++)
        {
            var gi = GoodIndex.FromInt(i);
            _csv.Write($"{stocks[gi]},");
        }
        _csv.WriteLine();
    }

    public void Dispose()
    {
        _csv?.Dispose();
    }
}


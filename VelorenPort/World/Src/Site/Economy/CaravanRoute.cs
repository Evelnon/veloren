using System;
using System.Collections.Generic;
using VelorenPort.CoreEngine;

namespace VelorenPort.World.Site.Economy;

/// <summary>
/// Defines a fixed route that caravans follow transporting goods.
/// </summary>
[Serializable]
public class CaravanRoute
{
    public List<Store<Site>.Id> Sites { get; } = new();
    public Dictionary<Good, float> Goods { get; } = new();

    public CaravanRoute() { }

    public CaravanRoute(IEnumerable<Store<Site>.Id> sites)
    {
        Sites.AddRange(sites);
    }

    public static CaravanRoute GenerateLinear(IEnumerable<Store<Site>.Id> sites)
        => new CaravanRoute(sites);
}

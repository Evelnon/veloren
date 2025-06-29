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
    public GoodMap<float> Goods { get; } = GoodMap<float>.FromDefault(0f);

    public CaravanRoute() { }

    public CaravanRoute(IEnumerable<Store<Site>.Id> sites)
    {
        Sites.AddRange(sites);
    }

    public static CaravanRoute GenerateLinear(IEnumerable<Store<Site>.Id> sites)
        => new CaravanRoute(sites);
}

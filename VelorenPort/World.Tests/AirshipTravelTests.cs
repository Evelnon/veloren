using VelorenPort.World;
using VelorenPort.World.Civ;
using VelorenPort.World.Site;
using VelorenPort.NativeMath;

namespace World.Tests;

public class AirshipTravelTests
{
    [Fact]
    public void RouteGeneration_CreatesConnectionBetweenSites()
    {
        var (world, index) = World.Empty();
        var siteA = new Site { Position = new int2(0, 0) };
        siteA.Plots.Add(new Plot { Kind = PlotKind.AirshipDock });
        var siteB = new Site { Position = new int2(100, 0) };
        siteB.Plots.Add(new Plot { Kind = PlotKind.AirshipDock });
        var idA = index.Sites.Insert(siteA);
        var idB = index.Sites.Insert(siteB);

        index.Airships.GenerateRoutes(world.Sim, index.Sites);

        Assert.Single(index.Airships.Routes);
        var route = index.Airships.Routes[0];
        Assert.Contains(idA, route.Sites);
        Assert.Contains(idB, route.Sites);
        Assert.True(route.Distance > 0);
        Assert.Equal(route.Distance / 200f, route.TravelTime, 3);
    }

    [Fact]
    public void Approaches_HaveValidDirection()
    {
        var (world, index) = World.Empty();
        var siteA = new Site { Position = new int2(0, 0) };
        siteA.Plots.Add(new Plot { Kind = PlotKind.AirshipDock });
        var siteB = new Site { Position = new int2(50, 50) };
        siteB.Plots.Add(new Plot { Kind = PlotKind.AirshipDock });
        index.Sites.Insert(siteA);
        index.Sites.Insert(siteB);

        index.Airships.GenerateRoutes(world.Sim, index.Sites);
        var route = index.Airships.Routes[0];
        var approach = route.Approaches[0];
        Assert.NotEqual(new float3(0,0,0), approach.AirshipDirection.ToFloat3());
        Assert.True(route.TravelTime > 0f);
        Assert.True(route.CycleTime > route.TravelTime);
    }
}

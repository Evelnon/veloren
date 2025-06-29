using VelorenPort.CoreEngine;
using Unity.Mathematics;

namespace World.Tests;

public class RegionTests
{
    [Fact]
    public void AddAndRemove_ProduceEvents()
    {
        var map = new RegionMap();
        var uid = new Uid(1);
        int2 pos = new int2(0, 0);
        map.Set(uid, pos);
        var events = map.GetEvents(pos);
        Assert.Single(events);
        Assert.IsType<RegionEvent.Entered>(events[0]);
        map.Tick();
        map.Remove(uid);
        events = map.GetEvents(pos);
        Assert.Single(events);
        Assert.IsType<RegionEvent.Left>(events[0]);
    }

    [Fact]
    public void History_PersistsAcrossTicks()
    {
        var map = new RegionMap();
        var uid = new Uid(2);
        int2 pos = new int2(1, 1);
        map.Set(uid, pos);
        map.Tick();
        map.Remove(uid);
        var history = map.GetHistory(pos).ToList();
        Assert.Equal(2, history.Count);
        Assert.IsType<RegionEvent.Entered>(history[0]);
        Assert.IsType<RegionEvent.Left>(history[1]);
    }

    [Fact]
    public void History_TruncatesAfterLimit()
    {
        var map = new RegionMap();
        int2 pos = new int2(2, 2);
        for (int i = 0; i < 70; i++)
        {
            var uid = new Uid((ulong)i);
            map.Set(uid, pos);
            map.Remove(uid);
        }
        var history = map.GetHistory(pos);
        Assert.Equal(64, history.Count);
    }
}

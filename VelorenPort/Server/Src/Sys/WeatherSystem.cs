using System;
using System.Collections.Generic;
using Unity.Mathematics;
using VelorenPort.CoreEngine;
using VelorenPort.Network;
using VelorenPort.Server.Weather;

namespace VelorenPort.Server.Sys;

/// <summary>
/// Periodically updates the world's weather and notifies all clients.
/// This is a lightweight placeholder for the complex weather system
/// present in the original server.
/// </summary>
public static class WeatherSystem
{
    private static readonly Random Rand = new();

    public static void Update(WorldIndex index, WeatherJob job, IEnumerable<Client> clients)
    {
        if (DateTime.UtcNow < job.NextUpdate)
            return;

        job.NextUpdate = DateTime.UtcNow + TimeSpan.FromSeconds(30);
        index.CurrentWeather = Generate(index.CurrentWeather);

        var msg = PreparedMsg.Create(
            0,
            new ServerGeneral.WeatherUpdate(index.CurrentWeather),
            new StreamParams(Promises.Ordered));
        foreach (var c in clients)
            c.SendPreparedAsync(msg).GetAwaiter().GetResult();
    }

    private static Weather Generate(Weather current)
    {
        float cloud = (float)Rand.NextDouble();
        float rain = cloud > 0.6f ? (float)Rand.NextDouble() : 0f;
        var wind = new float2((float)Rand.NextDouble() * 10f - 5f,
                              (float)Rand.NextDouble() * 10f - 5f);
        return new Weather(cloud, rain, wind);
    }
}

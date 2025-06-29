namespace VelorenPort.World.Layer;

/// <summary>
/// Adds points of interest to the chunk supplement based on noise.
/// </summary>
public static class SpotLayer
{
    public static void Apply(LayerContext ctx)
    {
        if (ctx.Rng.NextDouble() < ctx.ScatterChance)
        {
            var spot = SpotGenerator.Generate(ctx.ChunkPos, ctx.Noise);
            if (spot.HasValue)
                ctx.Supplement.AddEntity(spot.Value);
        }
    }
}

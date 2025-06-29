using VelorenPort.NativeMath;

namespace VelorenPort.World.Layer;

public static class WildlifeLayer
{
    public static void Apply(LayerContext ctx, Chunk chunk)
    {
        for (int x = 0; x < Chunk.Size.x; x++)
        for (int y = 0; y < Chunk.Size.y; y++)
        {
            int top = -1;
            for (int z = Chunk.Height - 1; z >= 0; z--)
            {
                if (chunk[x, y, z].IsFilled)
                {
                    top = z;
                    break;
                }
            }
            if (top <= 0 || top >= Chunk.Height - 1)
                continue;

            float3 wpos = new float3(
                chunk.Position.x * Chunk.Size.x + x,
                chunk.Position.y * Chunk.Size.y + y,
                top + 1);
            float n = ctx.Noise.Wildlife(wpos * 0.15f);
            if (n > 0.85f)
            {
                FaunaKind kind = (FaunaKind)((int)(n * 10) % 4 + 1);
                var spawn = new FaunaSpawn((int3)wpos, kind);
                chunk.AddWildlife(spawn);
                ctx.Supplement.Wildlife.Add(spawn);
                ctx.Supplement.WildlifeEntities.Add(new WildlifeEntity(spawn.Position, spawn.Kind));
            }
        }
    }
}

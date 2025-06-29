using VelorenPort.NativeMath;

namespace VelorenPort.World.Sim;

/// <summary>
/// More accurate topographic diffusion using a two-pass ADI solver adapted
/// from the Rust implementation in <c>world/src/sim/diffusion.rs</c>.
/// </summary>
public static class Diffusion
{
    private static void Tridag(float[] a, float[] b, float[] c, float[] r, float[] u, int n)
    {
        float[] gam = new float[n];
        float bet = b[0];
        u[0] = r[0] / bet;
        for (int j = 1; j < n; j++)
        {
            gam[j] = c[j - 1] / bet;
            bet = b[j] - a[j] * gam[j];
            u[j] = (r[j] - a[j] * u[j - 1]) / bet;
        }
        for (int j = n - 2; j >= 0; j--)
            u[j] -= gam[j + 1] * u[j + 1];
    }

    public static void Apply(WorldSim sim, float dt = 1f, float kd = 0.1f, float dx = 1f, float dy = 1f)
    {
        var size = sim.GetSize();
        int nx = size.x;
        int ny = size.y;
        int Len(int i, int j) => j * nx + i;

        var h = new float[nx * ny];
        var b = new float[nx * ny];
        for (int j = 0; j < ny; j++)
        for (int i = 0; i < nx; i++)
        {
            var chunk = sim.Get(new int2(i, j));
            if (chunk != null)
            {
                h[Len(i, j)] = chunk.Alt;
                b[Len(i, j)] = chunk.Basement;
            }
        }

        var zint = new float[nx * ny];
        var kdint = new float[nx * ny];
        var zintp = new float[nx * ny];
        for (int j = 0; j < ny; j++)
        for (int i = 0; i < nx; i++)
        {
            int idx = Len(i, j);
            zint[idx] = h[idx];
            kdint[idx] = kd;
        }
        Array.Copy(zint, zintp, zint.Length);

        // First pass along x
        var f = new float[nx];
        var diag = new float[nx];
        var sup = new float[nx];
        var inf = new float[nx];
        var res = new float[nx];
        for (int j = 1; j < ny - 1; j++)
        {
            for (int i = 1; i < nx - 1; i++)
            {
                int idx = Len(i, j);
                float factxp = (kdint[Len(i + 1, j)] + kdint[idx]) / 2f * (dt / 2f) / (dx * dx);
                float factxm = (kdint[Len(i - 1, j)] + kdint[idx]) / 2f * (dt / 2f) / (dx * dx);
                float factyp = (kdint[Len(i, j + 1)] + kdint[idx]) / 2f * (dt / 2f) / (dy * dy);
                float factym = (kdint[Len(i, j - 1)] + kdint[idx]) / 2f * (dt / 2f) / (dy * dy);
                diag[i] = 1f + factxp + factxm;
                sup[i] = -factxp;
                inf[i] = -factxm;
                f[i] = zintp[idx] + factyp * zintp[Len(i, j + 1)]
                    - (factyp + factym) * zintp[idx]
                    + factym * zintp[Len(i, j - 1)];
            }
            // boundary conditions (fixed)
            diag[0] = 1f;
            sup[0] = 0f;
            f[0] = zintp[Len(0, j)];
            diag[nx - 1] = 1f;
            inf[nx - 1] = 0f;
            f[nx - 1] = zintp[Len(nx - 1, j)];

            Tridag(inf, diag, sup, f, res, nx);
            for (int i = 0; i < nx; i++)
                zint[Len(i, j)] = res[i];
        }

        // Second pass along y
        f = new float[ny];
        diag = new float[ny];
        sup = new float[ny];
        inf = new float[ny];
        res = new float[ny];
        for (int i = 1; i < nx - 1; i++)
        {
            for (int j = 1; j < ny - 1; j++)
            {
                int idx = Len(i, j);
                float factxp = (kdint[Len(i + 1, j)] + kdint[idx]) / 2f * (dt / 2f) / (dx * dx);
                float factxm = (kdint[Len(i - 1, j)] + kdint[idx]) / 2f * (dt / 2f) / (dx * dx);
                float factyp = (kdint[Len(i, j + 1)] + kdint[idx]) / 2f * (dt / 2f) / (dy * dy);
                float factym = (kdint[Len(i, j - 1)] + kdint[idx]) / 2f * (dt / 2f) / (dy * dy);
                diag[j] = 1f + factyp + factym;
                sup[j] = -factyp;
                inf[j] = -factym;
                f[j] = zint[Len(i, j)] + factxp * zint[Len(i + 1, j)]
                    - (factxp + factxm) * zint[Len(i, j)]
                    + factxm * zint[Len(i - 1, j)];
            }
            diag[0] = 1f;
            sup[0] = 0f;
            f[0] = zint[Len(i, 0)];
            diag[ny - 1] = 1f;
            inf[ny - 1] = 0f;
            f[ny - 1] = zint[Len(i, ny - 1)];

            Tridag(inf, diag, sup, f, res, ny);
            for (int j = 0; j < ny; j++)
                zintp[Len(i, j)] = res[j];
        }

        for (int j = 0; j < ny; j++)
        for (int i = 0; i < nx; i++)
        {
            int idx = Len(i, j);
            h[idx] = zintp[idx];
        }

        for (int i = 0; i < b.Length; i++)
            b[i] = Math.Min(b[i], h[i]);

        for (int j = 0; j < ny; j++)
        for (int i = 0; i < nx; i++)
        {
            var chunk = sim.Get(new int2(i, j));
            if (chunk != null)
            {
                int idx = Len(i, j);
                chunk.Alt = h[idx];
                chunk.Basement = b[idx];
            }
        }
    }
}

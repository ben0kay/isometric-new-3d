using System;
using Godot;

// Pure deterministic value noise in absolute coordinates; independent of chunk loading and scene nodes.
public sealed class HeightSampler
{
    private readonly WorldGenerationSettings _settings;
    public HeightSampler(WorldGenerationSettings settings) { settings.Validate(); _settings = settings; }

    // =========================================================
    // Shared baseline elevation, including a gentle flat spawn clearing.
    public float Sample(float x, float z)
    {
        double scale = _settings.FeatureScaleMeters;
        double noise = Noise(x / scale, z / scale) * 0.8 +
            Noise(x / (scale * 0.5), z / (scale * 0.5)) * 0.2;
        float distance = MathF.Sqrt(x * x + z * z);
        float blend = Math.Clamp((distance - 10f) / 22f, 0f, 1f);
        blend = blend * blend * (3f - 2f * blend);
        return _settings.BaseHeightMeters + (float)noise * _settings.HillHeightMeters * blend;
    }

    // =========================================================
    // Central differences give identical normals on both sides of a seam.
    public Vector3 Normal(float x, float z) =>
        new Vector3(Sample(x - 0.5f, z) - Sample(x + 0.5f, z), 1f,
            Sample(x, z - 0.5f) - Sample(x, z + 0.5f)).Normalized();

    // =========================================================
    // Smooth seeded lattice noise; hash algorithm is locked by GeneratorVersion.
    private double Noise(double x, double z)
    {
        int ix = (int)Math.Floor(x), iz = (int)Math.Floor(z);
        double tx = x - ix, tz = z - iz;
        tx = tx * tx * (3 - 2 * tx); tz = tz * tz * (3 - 2 * tz);
        double a = Hash(ix, iz), b = Hash(ix + 1, iz);
        double c = Hash(ix, iz + 1), d = Hash(ix + 1, iz + 1);
        return (a + (b - a) * tx) * (1 - tz) + (c + (d - c) * tx) * tz;
    }

    // =========================================================
    // Explicit unchecked integer arithmetic is stable across runs and load order.
    private double Hash(int x, int z)
    {
        unchecked {
            uint h = (uint)_settings.Seed ^ ((uint)x * 374761393u) ^ ((uint)z * 668265263u);
            h = (h ^ (h >> 13)) * 1274126177u;
            h ^= h >> 16;
            return h / (double)uint.MaxValue * 2 - 1;
        }
    }
}

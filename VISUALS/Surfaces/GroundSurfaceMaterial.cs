using Godot;

// Creates one world-shared material from biome data; no per-chunk shader copies.
public static class GroundSurfaceMaterial
{
    // =========================================================
    // Apply bounded profile values and a deterministic seed offset.
    public static ShaderMaterial Create(BiomeSurfaceProfile profile, int seed)
    {
        var material = new ShaderMaterial {
            Shader = GD.Load<Shader>("res://VISUALS/Surfaces/GroundSurface.gdshader")
        };
        material.SetShaderParameter("soil_low", profile.SoilLow);
        material.SetShaderParameter("soil_high", profile.SoilHigh);
        material.SetShaderParameter("grass_low", profile.GrassLow);
        material.SetShaderParameter("grass_high", profile.GrassHigh);
        material.SetShaderParameter("dust_low", profile.DustLow);
        material.SetShaderParameter("dust_high", profile.DustHigh);
        material.SetShaderParameter("rock_low", profile.RockLow);
        material.SetShaderParameter("rock_high", profile.RockHigh);
        material.SetShaderParameter("patch_scale", Mathf.Max(1f, profile.PatchScaleMeters));
        material.SetShaderParameter("grass_coverage", Mathf.Clamp(profile.GrassCoverage, 0f, 1f));
        material.SetShaderParameter("dust_coverage", Mathf.Clamp(profile.DustCoverage, 0f, 1f));
        float start = Mathf.Clamp(profile.RockStartDegrees, 0f, 80f);
        float end = Mathf.Clamp(profile.RockFullDegrees, start + 1f, 89f);
        material.SetShaderParameter("rock_start", 1f - Mathf.Cos(Mathf.DegToRad(start)));
        material.SetShaderParameter("rock_end", 1f - Mathf.Cos(Mathf.DegToRad(end)));
        material.SetShaderParameter("detail_scale", Mathf.Max(0.1f, profile.DetailScaleMeters));
        material.SetShaderParameter("detail_strength", Mathf.Clamp(profile.DetailStrength, 0f, 0.5f));
        material.SetShaderParameter("surface_roughness", Mathf.Clamp(profile.Roughness, 0f, 1f));
        uint bits = unchecked((uint)seed);
        material.SetShaderParameter("seed_offset", new Vector2((bits & 65535u) / 64f, (bits >> 16) / 64f));
        return material;
    }
}

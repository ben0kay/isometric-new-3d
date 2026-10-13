using Godot;

// Creates one world-shared material from biome data; no per-chunk shader copies.
public static class GroundSurfaceMaterial
{
    #region Material Creation
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
        material.SetShaderParameter("grass_strength", Mathf.Clamp(profile.GrassBlendStrength, 0f, 1f));
        material.SetShaderParameter("dust_strength", Mathf.Clamp(profile.DustBlendStrength, 0f, 1f));
        material.SetShaderParameter("blend_softness", Mathf.Clamp(profile.BlendSoftness, 0.02f, 0.4f));
        material.SetShaderParameter("macro_variation", Mathf.Clamp(profile.MacroVariation, 0f, 0.4f));
        material.SetShaderParameter("flow_angle", Mathf.DegToRad(profile.FlowAngleDegrees));
        material.SetShaderParameter("flow_stretch", Mathf.Clamp(profile.FlowStretch, 1f, 6f));
        material.SetShaderParameter("warp_strength", Mathf.Clamp(profile.WarpStrength, 0f, 1f));
        float soilStart = Mathf.Clamp(profile.SoilStartDegrees, 0f, 60f);
        float soilEnd = Mathf.Clamp(profile.SoilFullDegrees, soilStart + 1f, 80f);
        material.SetShaderParameter("soil_start", 1f - Mathf.Cos(Mathf.DegToRad(soilStart)));
        material.SetShaderParameter("soil_end", 1f - Mathf.Cos(Mathf.DegToRad(soilEnd)));
        uint bits = unchecked((uint)seed);
        material.SetShaderParameter("seed_offset", new Vector2((bits & 65535u) / 64f, (bits >> 16) / 64f));
        return material;
    }
    #endregion
}


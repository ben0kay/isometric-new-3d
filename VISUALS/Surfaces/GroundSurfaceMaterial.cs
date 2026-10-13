using Godot;

// One material/noise resource per world; ports reference formulas without per-chunk shader copies.
public static class GroundSurfaceMaterial
{
    #region Material Creation
    // =========================================================
    // Bind raw reference palette coefficients and bounded biome settings.
    public static ShaderMaterial Create(BiomeSurfaceProfile profile, int seed)
    {
        var material = new ShaderMaterial {
            Shader = GD.Load<Shader>("res://VISUALS/Surfaces/GroundSurface.gdshader")
        };
        material.SetShaderParameter("ground_noise", GroundSurfaceNoise.GetTexture());
        material.SetShaderParameter("basalt_low", profile.BasaltLow);
        material.SetShaderParameter("basalt_high", profile.BasaltHigh);
        material.SetShaderParameter("dust_low", profile.DustLow);
        material.SetShaderParameter("dust_high", profile.DustHigh);
        material.SetShaderParameter("mineral_low", profile.MineralLow);
        material.SetShaderParameter("mineral_high", profile.MineralHigh);
        material.SetShaderParameter("rock_low", profile.RockLow);
        material.SetShaderParameter("rock_high", profile.RockHigh);
        material.SetShaderParameter("steep_low", profile.SteepRockLow);
        material.SetShaderParameter("steep_high", profile.SteepRockHigh);
        material.SetShaderParameter("grass_low", profile.GrassLow);
        material.SetShaderParameter("grass_high", profile.GrassHigh);
        material.SetShaderParameter("soil_low", profile.SoilLow);
        material.SetShaderParameter("soil_high", profile.SoilHigh);
        material.SetShaderParameter("mud_low", profile.MudLow);
        material.SetShaderParameter("mud_high", profile.MudHigh);
        material.SetShaderParameter("path_low", profile.PathLow);
        material.SetShaderParameter("path_high", profile.PathHigh);
        material.SetShaderParameter("meters_per_tile", Mathf.Clamp(profile.MetersPerReferenceTile, 0.25f, 8f));
        material.SetShaderParameter("deposit_scale", Mathf.Clamp(profile.DepositScale, 0.25f, 4f));
        material.SetShaderParameter("blend_softness", Mathf.Clamp(profile.BlendSoftness, 0.25f, 2f));
        material.SetShaderParameter("dust_amount", Mathf.Clamp(profile.DustCoverage, 0f, 1f));
        material.SetShaderParameter("dust_strength", Mathf.Clamp(profile.DustBlendStrength, 0f, 1f));
        material.SetShaderParameter("mineral_amount", Mathf.Clamp(profile.MineralAmount, 0f, 1f));
        material.SetShaderParameter("grass_amount", Mathf.Clamp(profile.GrassCoverage, 0f, 1f));
        material.SetShaderParameter("grass_strength", Mathf.Clamp(profile.GrassBlendStrength, 0f, 1f));
        material.SetShaderParameter("grass_patch_scale", Mathf.Clamp(profile.GrassPatchScale, 4f, 128f));
        material.SetShaderParameter("soil_base_weight", Mathf.Clamp(profile.SoilBaseWeight, 0f, 1f));
        material.SetShaderParameter("mud_strength", Mathf.Clamp(profile.MudStrength, 0f, 1f));
        material.SetShaderParameter("stony_strength", Mathf.Clamp(profile.StonyGroundStrength, 0f, 1f));
        material.SetShaderParameter("weathered_amount", Mathf.Clamp(profile.WeatheredRockAmount, 0f, 1f));
        material.SetShaderParameter("detail_strength", Mathf.Clamp(profile.DetailStrength, 0f, 1f));
        material.SetShaderParameter("grain_strength", Mathf.Clamp(profile.GrainStrength, 0f, 1f));
        material.SetShaderParameter("fracture_strength", Mathf.Clamp(profile.FractureStrength, 0f, 1f));
        material.SetShaderParameter("relief_strength", Mathf.Clamp(profile.ReliefStrengthMeters, 0f, 0.1f));
        material.SetShaderParameter("surface_brightness", Mathf.Clamp(profile.SurfaceBrightness, 0.5f, 2f));
        material.SetShaderParameter("surface_roughness", Mathf.Clamp(profile.Roughness, 0f, 1f));
        float start = Mathf.Clamp(profile.RockStartDegrees, 0f, 80f);
        float end = Mathf.Clamp(profile.RockFullDegrees, start + 1f, 89f);
        material.SetShaderParameter("rock_start", 1f - Mathf.Cos(Mathf.DegToRad(start)));
        material.SetShaderParameter("rock_end", 1f - Mathf.Cos(Mathf.DegToRad(end)));
        material.SetShaderParameter("steep_blocks", profile.SteepRockBlocks);
        uint bits = unchecked((uint)seed);
        material.SetShaderParameter("pattern_offset", profile.OffsetPatternByWorldSeed
            ? new Vector2((bits & 65535u) / 64f, (bits >> 16) / 64f) : Vector2.Zero);
        return material;
    }
    #endregion
}

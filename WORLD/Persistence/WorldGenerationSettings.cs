using System;

// Serializable creation settings. Future saves restore this snapshot, not current .tres defaults.
public sealed class WorldGenerationSettings
{
    public int GeneratorVersion { get; set; } = 1;
    public int Seed { get; set; }
    public int WidthChunks { get; set; }
    public int LengthChunks { get; set; }
    public int ChunkSizeMeters { get; set; }
    public int CellsPerChunk { get; set; }
    public string BiomeId { get; set; } = "test";
    public float BaseHeightMeters { get; set; }
    public float HillHeightMeters { get; set; }
    public float FeatureScaleMeters { get; set; }
    public int MinX => -(WidthChunks / 2);
    public int MinZ => -(LengthChunks / 2);

    // =========================================================
    // Capture and validate all inputs that affect baseline terrain.
    public static WorldGenerationSettings Capture(WorldConfig config)
    {
        var biome = config.PrototypeBiome;
        if (biome == null || !biome.Enabled || biome.Id != "test" || biome.Terrain == null)
            throw new InvalidOperationException("Terrain prototype requires the enabled Test biome and its terrain profile.");
        var settings = new WorldGenerationSettings {
            Seed = config.WorldSeed, WidthChunks = config.WorldWidthChunks,
            LengthChunks = config.WorldLengthChunks, ChunkSizeMeters = config.ChunkSizeMeters,
            CellsPerChunk = config.TerrainCellsPerChunk, BiomeId = biome.Id,
            BaseHeightMeters = biome.Terrain.BaseHeightMeters,
            HillHeightMeters = biome.Terrain.HillHeightMeters,
            FeatureScaleMeters = biome.Terrain.FeatureScaleMeters
        };
        settings.Validate();
        return settings;
    }

    // =========================================================
    // Reject unsupported versions or invalid saved creation inputs.
    public void Validate()
    {
        if (GeneratorVersion != 1 || BiomeId != "test" ||
            WidthChunks < 2 || WidthChunks > 128 || LengthChunks < 2 || LengthChunks > 128 ||
            ChunkSizeMeters < 16 || ChunkSizeMeters > 128 || CellsPerChunk < 8 || CellsPerChunk > 64 ||
            !float.IsFinite(BaseHeightMeters) || !float.IsFinite(HillHeightMeters) ||
            !float.IsFinite(FeatureScaleMeters) || HillHeightMeters < 0 || FeatureScaleMeters < 5)
            throw new InvalidOperationException("Invalid or unsupported terrain generation settings.");
    }
}

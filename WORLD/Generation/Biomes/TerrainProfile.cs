using Godot;

// Shared editable parameters for biome terrain shape; sampled by the shared deterministic HeightSampler.
[GlobalClass]
public partial class TerrainProfile : Resource
{
    [ExportGroup("Elevation")]
    [Export] public float BaseHeightMeters { get; set; } = 0f;
    [Export(PropertyHint.Range, "0,500,0.5")] public float HillHeightMeters { get; set; } = 8f;

    [ExportGroup("Noise")]
    [Export(PropertyHint.Range, "5,5000,5")] public float FeatureScaleMeters { get; set; } = 180f;
}

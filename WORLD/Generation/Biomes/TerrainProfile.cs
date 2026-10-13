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
    [ExportGroup("Prototype Mountain")]
    [Export] public bool MountainEnabled { get; set; } = false;
    [Export] public Vector2 MountainCenterMeters { get; set; } = new(-100, -100);
    [Export(PropertyHint.Range, "0,300,1")] public float MountainHeightMeters { get; set; } = 55f;
    [Export(PropertyHint.Range, "20,1000,5")] public float MountainRadiusMeters { get; set; } = 120f;
}


using Godot;

// Data-driven 3D adaptation of the original 2D basalt-first ground recipe.
[GlobalClass]
public partial class BiomeSurfaceProfile : Resource
{
    #region Basalt Foundation
    [ExportGroup("Basalt Foundation")]
    [Export] public Color BasaltLow { get; set; } = new(0.075f, 0.086f, 0.098f);
    [Export] public Color BasaltHigh { get; set; } = new(0.135f, 0.148f, 0.157f);
    [Export(PropertyHint.Range, "0.5,2,0.05")] public float SurfaceBrightness { get; set; } = 1f;
    #endregion

    #region Dust And Minerals
    [ExportGroup("Dust / Sediment")]
    [Export] public Color DustLow { get; set; } = new(0.112f, 0.105f, 0.094f);
    [Export] public Color DustHigh { get; set; } = new(0.177f, 0.158f, 0.130f);
    [Export(PropertyHint.Range, "0,1,0.01")] public float DustCoverage { get; set; } = 0.55f;
    [Export(PropertyHint.Range, "0,1,0.01")] public float DustBlendStrength { get; set; } = 1f;
    [ExportGroup("Mineral Seams")]
    [Export] public Color MineralLow { get; set; } = new(0.105f, 0.151f, 0.145f);
    [Export] public Color MineralHigh { get; set; } = new(0.215f, 0.279f, 0.245f);
    [Export(PropertyHint.Range, "0,1,0.01")] public float MineralAmount { get; set; } = 0.32f;
    #endregion

    #region Exposed Rock
    [ExportGroup("Exposed Rock")]
    [Export] public Color RockLow { get; set; } = new(0.115f, 0.125f, 0.135f);
    [Export] public Color RockHigh { get; set; } = new(0.235f, 0.245f, 0.255f);
    [Export] public Color SteepRockLow { get; set; } = new(0.135f, 0.145f, 0.155f);
    [Export] public Color SteepRockHigh { get; set; } = new(0.265f, 0.275f, 0.28f);
    [Export(PropertyHint.Range, "0,1,0.01")] public float WeatheredRockAmount { get; set; } = 0f;
    [Export(PropertyHint.Range, "0,80,1")] public float RockStartDegrees { get; set; } = 22f;
    [Export(PropertyHint.Range, "1,89,1")] public float RockFullDegrees { get; set; } = 38f;
    [Export] public bool SteepRockBlocks { get; set; } = true;
    #endregion

    #region Optional Future Biome Layers
    [ExportGroup("Optional Grass")]
    [Export] public Color GrassLow { get; set; } = new(0.12f, 0.20f, 0.10f);
    [Export] public Color GrassHigh { get; set; } = new(0.24f, 0.34f, 0.17f);
    [Export(PropertyHint.Range, "0,1,0.01")] public float GrassCoverage { get; set; } = 0f;
    [Export(PropertyHint.Range, "0,1,0.01")] public float GrassBlendStrength { get; set; } = 1f;
    [Export(PropertyHint.Range, "4,128,1")] public float GrassPatchScale { get; set; } = 32f;
    [ExportGroup("Optional Soil / Sand")]
    [Export] public Color SoilLow { get; set; } = new(0.20f, 0.16f, 0.12f);
    [Export] public Color SoilHigh { get; set; } = new(0.30f, 0.25f, 0.19f);
    [Export(PropertyHint.Range, "0,1,0.01")] public float SoilBaseWeight { get; set; } = 0f;
    [ExportGroup("Optional Mud")]
    [Export] public Color MudLow { get; set; } = new(0.035f, 0.036f, 0.032f);
    [Export] public Color MudHigh { get; set; } = new(0.078f, 0.071f, 0.056f);
    [Export(PropertyHint.Range, "0,1,0.01")] public float MudStrength { get; set; } = 0f;
    [ExportGroup("Optional Stony Ground")]
    [Export] public Color PathLow { get; set; } = new(0.145f, 0.142f, 0.135f);
    [Export] public Color PathHigh { get; set; } = new(0.215f, 0.210f, 0.195f);
    [Export(PropertyHint.Range, "0,1,0.01")] public float StonyGroundStrength { get; set; } = 0f;
    #endregion

    #region Reference Sampling And Detail
    [ExportGroup("Reference Sampling")]
    [Export(PropertyHint.Range, "0.25,8,0.25")] public float MetersPerReferenceTile { get; set; } = 1f;
    [Export] public bool OffsetPatternByWorldSeed { get; set; } = false;
    [Export(PropertyHint.Range, "0.25,4,0.05")] public float DepositScale { get; set; } = 1f;
    [Export(PropertyHint.Range, "0.25,2,0.05")] public float BlendSoftness { get; set; } = 1f;
    [ExportGroup("Fine Detail")]
    [Export(PropertyHint.Range, "0,1,0.01")] public float DetailStrength { get; set; } = 0.35f;
    [Export(PropertyHint.Range, "0,0.1,0.005")] public float ReliefStrengthMeters { get; set; } = 0.035f;
    [Export(PropertyHint.Range, "0,1,0.01")] public float GrainStrength { get; set; } = 1f;
    [Export(PropertyHint.Range, "0,1,0.01")] public float FractureStrength { get; set; } = 1f;
    [Export(PropertyHint.Range, "0,1,0.01")] public float Roughness { get; set; } = 0.95f;
    #endregion
}

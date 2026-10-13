using Godot;

// Biome-owned appearance settings, independent of terrain height and collision.
[GlobalClass]
public partial class BiomeSurfaceProfile : Resource
{
    #region Soil
    [ExportGroup("Soil")]
    [Export] public Color SoilLow { get; set; } = new(0.24f, 0.19f, 0.13f);
    [Export] public Color SoilHigh { get; set; } = new(0.43f, 0.34f, 0.23f);
    #endregion

    #region Grass
    [ExportGroup("Grass")]
    [Export] public Color GrassLow { get; set; } = new(0.18f, 0.27f, 0.12f);
    [Export] public Color GrassHigh { get; set; } = new(0.35f, 0.43f, 0.21f);
    [Export(PropertyHint.Range, "0,1,0.01")] public float GrassCoverage { get; set; } = 0.55f;
    #endregion

    #region Dust And Gravel
    [ExportGroup("Dust / Gravel")]
    [Export] public Color DustLow { get; set; } = new(0.43f, 0.37f, 0.26f);
    [Export] public Color DustHigh { get; set; } = new(0.61f, 0.54f, 0.39f);
    [Export(PropertyHint.Range, "0,1,0.01")] public float DustCoverage { get; set; } = 0.35f;
    #endregion

    #region Slope Rock
    [ExportGroup("Slope Rock")]
    [Export] public Color RockLow { get; set; } = new(0.26f, 0.28f, 0.29f);
    [Export] public Color RockHigh { get; set; } = new(0.48f, 0.49f, 0.46f);
    [Export(PropertyHint.Range, "0,80,1")] public float RockStartDegrees { get; set; } = 18f;
    [Export(PropertyHint.Range, "1,89,1")] public float RockFullDegrees { get; set; } = 36f;
    #endregion

    #region Surface Composition
    [ExportGroup("Surface Composition")]
    [Export(PropertyHint.Range, "0,1,0.01")] public float GrassBlendStrength { get; set; } = 0.38f;
    [Export(PropertyHint.Range, "0,1,0.01")] public float DustBlendStrength { get; set; } = 0.24f;
    [Export(PropertyHint.Range, "0.02,0.4,0.01")] public float BlendSoftness { get; set; } = 0.18f;
    [Export(PropertyHint.Range, "0,0.4,0.01")] public float MacroVariation { get; set; } = 0.10f;
    [ExportGroup("Slope Soil")]
    [Export(PropertyHint.Range, "0,60,1")] public float SoilStartDegrees { get; set; } = 12f;
    [Export(PropertyHint.Range, "1,80,1")] public float SoilFullDegrees { get; set; } = 28f;
    #endregion

    #region Terrain Flow
    [ExportGroup("Terrain Flow")]
    [Export(PropertyHint.Range, "-180,180,1")] public float FlowAngleDegrees { get; set; } = -25f;
    [Export(PropertyHint.Range, "1,6,0.1")] public float FlowStretch { get; set; } = 2.8f;
    [Export(PropertyHint.Range, "0,1,0.05")] public float WarpStrength { get; set; } = 0.35f;
    #endregion

    #region Pattern
    [ExportGroup("Pattern")]
    [Export(PropertyHint.Range, "1,100,0.5")] public float PatchScaleMeters { get; set; } = 14f;
    [Export(PropertyHint.Range, "0.1,5,0.1")] public float DetailScaleMeters { get; set; } = 0.35f;
    [Export(PropertyHint.Range, "0,0.5,0.01")] public float DetailStrength { get; set; } = 0.35f;
    [Export(PropertyHint.Range, "0,1,0.01")] public float Roughness { get; set; } = 0.95f;
    #endregion
}


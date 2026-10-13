using Godot;

// Shared sunlight, ambient fill and lightweight screen grading controls.
[GlobalClass]
public partial class VisualConfig : Resource
{
    #region Sun
    [ExportGroup("Sun")]
    [Export(PropertyHint.Range, "5,90,1")] public float SunElevationDegrees { get; set; } = 45f;
    [Export(PropertyHint.Range, "-180,180,1")] public float SunAzimuthDegrees { get; set; } = 90f;
    [Export(PropertyHint.Range, "0,4,0.1")] public float SunEnergy { get; set; } = 1.3f;
    [Export] public Color SunColor { get; set; } = new(1f, 0.95f, 0.88f);
    [Export] public bool SunShadows { get; set; } = true;

    #endregion

    #region Shadow Quality
    [ExportGroup("Shadow Quality")]
    [Export(PropertyHint.Range, "10,300,5")] public float ShadowMaxDistance { get; set; } = 45f;
    [Export(PropertyHint.Range, "0,1,0.005")] public float ShadowBias { get; set; } = 0.1f;
    [Export(PropertyHint.Range, "0,5,0.05")] public float ShadowNormalBias { get; set; } = 1f;
    [Export(PropertyHint.Range, "0,5,0.1")] public float ShadowBlur { get; set; } = 1f;

    #endregion

    #region Ambient Fill
    [ExportGroup("Ambient")]
    [Export] public Color AmbientColor { get; set; } = new(0.65f, 0.73f, 0.85f);
    [Export(PropertyHint.Range, "0,2,0.1")] public float AmbientEnergy { get; set; } = 0.6f;
    #endregion

    #region Colour Grading
    [ExportGroup("Colour Grading")]
    [Export] public bool GradeEnabled { get; set; } = true;
    [Export(PropertyHint.Range, "0.8,1.4,0.01")] public float GradeContrast { get; set; } = 1.12f;
    [Export(PropertyHint.Range, "0.5,1.2,0.01")] public float GradeSaturation { get; set; } = 0.94f;
    [Export(PropertyHint.Range, "0,0.05,0.005")] public float GradeShadowLift { get; set; } = 0.012f;
    [Export(PropertyHint.Range, "0,0.2,0.01")] public float GradeVignette { get; set; } = 0.04f;
    #endregion
}


using Godot;

[GlobalClass]
public partial class VisualConfig : Resource
{
    [ExportGroup("Sun")]
    [Export(PropertyHint.Range, "5,90,1")] public float SunElevationDegrees { get; set; } = 45f;
    [Export(PropertyHint.Range, "-180,180,1")] public float SunAzimuthDegrees { get; set; } = 135f;
    [Export(PropertyHint.Range, "0,4,0.1")] public float SunEnergy { get; set; } = 1.3f;
    [Export] public bool SunShadows { get; set; } = true;

    [ExportGroup("Shadow Quality")]
    [Export(PropertyHint.Range, "10,300,5")] public float ShadowMaxDistance { get; set; } = 60f;
    [Export(PropertyHint.Range, "0,1,0.005")] public float ShadowBias { get; set; } = 0.04f;
    [Export(PropertyHint.Range, "0,5,0.05")] public float ShadowNormalBias { get; set; } = 0.5f;
    [Export(PropertyHint.Range, "0,5,0.1")] public float ShadowBlur { get; set; } = 1f;

    [ExportGroup("Ambient")]
    [Export(PropertyHint.Range, "0,2,0.1")] public float AmbientEnergy { get; set; } = 0.6f;
}

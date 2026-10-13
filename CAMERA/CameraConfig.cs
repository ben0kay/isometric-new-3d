using Godot;

[GlobalClass]
public partial class CameraConfig : Resource
{
    [ExportGroup("Isometric View")]
    [Export(PropertyHint.Range, "15,80,1")] public float DownwardAngleDegrees { get; set; } = 40f;
    [Export(PropertyHint.Range, "-180,180,1")] public float AzimuthDegrees { get; set; } = 45f;
    [Export(PropertyHint.Range, "5,40,0.5")] public float OrthographicSize { get; set; } = 16f;
    [Export(PropertyHint.Range, "5,60,1")] public float CameraDistance { get; set; } = 22f;
    [Export(PropertyHint.Range, "0,30,0.5")] public float FollowSmoothness { get; set; } = 8f;
}

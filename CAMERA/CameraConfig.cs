using Godot;

// Camera view and clipping controls; far range also affects orthographic shadow precision.
[GlobalClass]
public partial class CameraConfig : Resource
{
    #region Isometric View
    [ExportGroup("Isometric View")]
    [Export(PropertyHint.Range, "15,80,1")] public float DownwardAngleDegrees { get; set; } = 40f;
    [Export(PropertyHint.Range, "-180,180,1")] public float AzimuthDegrees { get; set; } = 45f;
    [Export(PropertyHint.Range, "5,40,0.5")] public float OrthographicSize { get; set; } = 20f;
    [Export(PropertyHint.Range, "5,60,1")] public float CameraDistance { get; set; } = 22f;
    [Export(PropertyHint.Range, "0,30,0.5")] public float FollowSmoothness { get; set; } = 8f;
    #endregion

    #region Clipping
    [ExportGroup("Clipping")]
    [Export(PropertyHint.Range, "60,500,5")] public float FarClipMeters { get; set; } = 150f;
    #endregion
}


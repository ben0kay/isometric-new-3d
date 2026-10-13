using Godot;

// Shared tuning for the 2D-style HUD over the spatial 3D frontend.
[GlobalClass]
public partial class UiConfig : Resource
{
    [ExportGroup("Spatial Menu")]
    [Export(PropertyHint.Range, "0.2,4,0.05")] public float CameraTravelSeconds { get; set; } = 1.25f;
    [Export(PropertyHint.Range, "0.05,1,0.05")] public float FadeSeconds { get; set; } = 0.3f;
    [Export] public Vector3 MainCameraPosition { get; set; } = new(0, 3, 18);
    [Export] public Vector3 CampaignCameraPosition { get; set; } = new(0, 3, 6);
    [ExportGroup("Main Menu")]
    [Export] public float MenuWidth { get; set; } = 430f;
    [Export] public float MenuButtonHeight { get; set; } = 78f;
}

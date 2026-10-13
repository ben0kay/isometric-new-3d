using Godot;

// Inspector-friendly runtime diagnostics.
[GlobalClass]
public partial class DebugConfig : Resource
{
    [Export] public bool ShowHud { get; set; } = true;
    [Export(PropertyHint.Range, "0.05,1,0.05")] public float HudRefreshSeconds { get; set; } = 0.1f;
}

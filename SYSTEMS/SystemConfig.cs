using Godot;

// Master systems umbrella configuration; persistence details live in SYSTEMS/Saving.
[GlobalClass]
public partial class SystemConfig : Resource
{
    [ExportGroup("Saving")]
    [Export] public bool AllowManualSaving { get; set; } = true;
}

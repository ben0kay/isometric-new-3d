using Godot;

[GlobalClass]
public partial class PlayerConfig : Resource
{
    [ExportGroup("Movement")]
    [Export(PropertyHint.Range, "0.5,20,0.1")] public float WalkSpeed { get; set; } = 5f;
    [Export(PropertyHint.Range, "1,40,0.5")] public float Acceleration { get; set; } = 18f;
    [Export(PropertyHint.Range, "1,40,0.5")] public float Deceleration { get; set; } = 22f;
    [Export(PropertyHint.Range, "0,50,0.5")] public float Gravity { get; set; } = 24f;

    [ExportGroup("Cursor Facing")]
    [Export] public bool FaceCursor { get; set; } = true;
    [Export(PropertyHint.Range, "0,30,0.5")] public float VisualTurnSpeed { get; set; } = 12f;

    [ExportGroup("Action Keys")]
    [Export] public Key MoveLeftKey { get; set; } = Key.Left;
    [Export] public Key MoveRightKey { get; set; } = Key.Right;
    [Export] public Key MoveForwardKey { get; set; } = Key.Up;
    [Export] public Key MoveBackKey { get; set; } = Key.Down;
}

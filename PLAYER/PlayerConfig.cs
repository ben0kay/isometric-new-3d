using Godot;

[GlobalClass]
public partial class PlayerConfig : Resource
{
	[ExportGroup("Movement")]
	[Export(PropertyHint.Range, "0.5,20,0.1")] public float WalkSpeed { get; set; } = 5f;
	[Export(PropertyHint.Range, "1,4,0.1")] public float SprintMultiplier { get; set; } = 1.6f;
	[Export(PropertyHint.Range, "1,40,0.5")] public float Acceleration { get; set; } = 18f;
	[Export(PropertyHint.Range, "1,40,0.5")] public float Deceleration { get; set; } = 22f;
	[Export(PropertyHint.Range, "0,50,0.5")] public float Gravity { get; set; } = 24f;
	[Export(PropertyHint.Range, "1,20,0.25")] public float JumpVelocity { get; set; } = 7f;

	[ExportGroup("Cursor Facing")]
	[Export] public bool FaceCursor { get; set; } = true;
	[Export(PropertyHint.Range, "0,30,0.5")] public float VisualTurnSpeed { get; set; } = 12f;

[ExportGroup("Model and Animation")]
[Export] public bool AnimationEnabled { get; set; } = true;
[Export(PropertyHint.Range, "0.1,3,0.05")] public float ModelScale { get; set; } = 0.95f;
[Export] public float ModelOffsetY { get; set; } = -0.9f;
[Export(PropertyHint.Range, "0,1,0.01")] public float AnimationBlendSeconds { get; set; } = 0.12f;
[Export(PropertyHint.Range, "0.01,1,0.01")] public float AnimationMoveThreshold { get; set; } = 0.12f;
[Export(PropertyHint.Range, "0.1,10,0.1")] public float AnimationWalkReferenceSpeed { get; set; } = 1.5f;
[Export(PropertyHint.Range, "0.1,15,0.1")] public float AnimationRunReferenceSpeed { get; set; } = 3f;
[Export(PropertyHint.Range, "0,30,1")] public float AnimationDirectionMarginDegrees { get; set; } = 12f;
[Export(PropertyHint.Range, "1,30,1")] public float AnimationSpeedSmoothness { get; set; } = 10f;

	[ExportGroup("Action Keys")]
	[Export] public Key MoveLeftKey { get; set; } = Key.Left;
	[Export] public Key MoveRightKey { get; set; } = Key.Right;
	[Export] public Key MoveForwardKey { get; set; } = Key.Up;
	[Export] public Key MoveBackKey { get; set; } = Key.Down;
	[Export] public Key SprintKey { get; set; } = Key.Shift;
	[Export] public Key AnimationToggleKey { get; set; } = Key.F8;
	[Export] public MouseButton JumpMouseButton { get; set; } = MouseButton.Right;
}

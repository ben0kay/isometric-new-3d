using Godot;

// Thin player coordinator. Keep input, movement, visuals, inventory etc. independent.
public partial class Player : CharacterBody3D
{
    [Export] public PlayerConfig Config { get; set; }
    [Export] public Camera3D FollowCamera { get; set; }
    private Node3D _visual;

    public override void _Ready()
    {
        Config ??= GD.Load<PlayerConfig>("res://PLAYER/PlayerConfig.tres");
        _visual = GetNode<Node3D>("Visual");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (FollowCamera == null)
            return;
        PlayerMotor.Tick(this, FollowCamera, Config, delta);
        Vector3 planarVelocity = new(Velocity.X, 0, Velocity.Z);
        if (planarVelocity.LengthSquared() > 0.025f)
        {
            float target = Mathf.Atan2(-planarVelocity.X, -planarVelocity.Z);
            _visual.Rotation = new Vector3(0, Mathf.LerpAngle(_visual.Rotation.Y, target,
                Mathf.Clamp(Config.VisualTurnSpeed * (float)delta, 0, 1)), 0);
        }
    }
}

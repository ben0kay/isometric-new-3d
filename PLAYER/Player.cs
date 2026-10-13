using Godot;

// Thin player coordinator; visual aim stays independent of movement and jumping.
public partial class Player : CharacterBody3D
{
    [Export] public PlayerConfig Config { get; set; }
    [Export] public Camera3D FollowCamera { get; set; }
    private PlayerAnimator _visual;

    public override void _Ready()
    {
        Config ??= GD.Load<PlayerConfig>("res://PLAYER/PlayerConfig.tres");
        _visual = GetNode<PlayerAnimator>("Visual");
        _visual.Configure(Config);
        PlayerInput.Configure(Config);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (FollowCamera == null)
            return;

        PlayerMotor.Tick(this, FollowCamera, Config, delta);

        Vector3 facing = Config.FaceCursor ? CursorDirection() : new Vector3(Velocity.X, 0, Velocity.Z);
        if (facing.LengthSquared() >= 0.0001f)
        {
            float targetYaw = Mathf.Atan2(-facing.X, -facing.Z);
            float weight = Mathf.Clamp(Config.VisualTurnSpeed * (float)delta, 0f, 1f);
            _visual.Rotation = new Vector3(0, Mathf.LerpAngle(_visual.Rotation.Y, targetYaw, weight), 0);
        }
        _visual.Tick(this, delta);
    }

    private Vector3 CursorDirection()
    {
        Vector2 mouse = GetViewport().GetMousePosition();
        Vector3 origin = FollowCamera.ProjectRayOrigin(mouse);
        Vector3 ray = FollowCamera.ProjectRayNormal(mouse);
        Plane aimPlane = new(Vector3.Up, GlobalPosition.Y);
        Vector3? hit = aimPlane.IntersectsRay(origin, ray);
        if (!hit.HasValue)
            return Vector3.Zero;
        Vector3 direction = hit.Value - GlobalPosition;
        direction.Y = 0f;
        return direction;
    }
}

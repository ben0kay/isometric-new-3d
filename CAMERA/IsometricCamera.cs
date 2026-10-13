using Godot;

// Independent world-owned camera; no Camera3D is nested under the player.
public partial class IsometricCamera : Camera3D
{
    #region References
    [Export] public CameraConfig Config { get; set; }
    [Export] public Node3D Target { get; set; }

    #endregion

    #region Camera Updates
    // =========================================================
    // Set the projection and bounded clipping range before the first rendered frame.
    public override void _Ready()
    {
        Config ??= GD.Load<CameraConfig>("res://CAMERA/CameraConfig.tres");
        Projection = ProjectionType.Orthogonal;
        Size = Config.OrthographicSize;
        Far = Mathf.Max(Near + 1f, Config.FarClipMeters);
        Current = true;
        // Updated every render frame; avoid interpolating the camera a second time.
        PhysicsInterpolationMode = PhysicsInterpolationModeEnum.Off;
        if (Target != null) GlobalPosition = Target.GlobalPosition + Offset();
        LookAt(Target?.GlobalPosition ?? Vector3.Zero, Vector3.Up);
    }

    // =========================================================
    // Follow the rendered player position without adding a second interpolation pass.
    public override void _Process(double delta)
    {
        if (Target == null) return;
        Size = Config.OrthographicSize;
        // Follow the same position used to render the physics-driven player.
        Vector3 desired = Target.GetGlobalTransformInterpolated().Origin + Offset();
        float weight = 1f - Mathf.Exp(-Config.FollowSmoothness * (float)delta);
        GlobalPosition = GlobalPosition.Lerp(desired, weight);
        LookAt(GlobalPosition - Offset(), Vector3.Up);
    }

    #endregion

    #region View Geometry
    // =========================================================
    // Fixed view direction with a configurable physical camera distance.
    private Vector3 Offset()
    {
        float elevation = Mathf.DegToRad(Config.DownwardAngleDegrees);
        float azimuth = Mathf.DegToRad(Config.AzimuthDegrees);
        float horizontal = Config.CameraDistance * Mathf.Cos(elevation);
        return new Vector3(Mathf.Sin(azimuth) * horizontal,
            Mathf.Sin(elevation) * Config.CameraDistance,
            Mathf.Cos(azimuth) * horizontal);
    }
    #endregion
}


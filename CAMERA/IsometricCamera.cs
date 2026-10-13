using Godot;

// Independent world-owned camera; no Camera3D is nested under the player.
public partial class IsometricCamera : Camera3D
{
    [Export] public CameraConfig Config { get; set; }
    [Export] public Node3D Target { get; set; }

    public override void _Ready()
    {
        Config ??= GD.Load<CameraConfig>("res://CAMERA/CameraConfig.tres");
        Projection = ProjectionType.Orthogonal;
        Size = Config.OrthographicSize;
        Current = true;
        if (Target != null) GlobalPosition = Target.GlobalPosition + Offset();
        LookAt(Target?.GlobalPosition ?? Vector3.Zero, Vector3.Up);
    }

    public override void _Process(double delta)
    {
        if (Target == null) return;
        Size = Config.OrthographicSize;
        Vector3 desired = Target.GlobalPosition + Offset();
        float weight = 1f - Mathf.Exp(-Config.FollowSmoothness * (float)delta);
        GlobalPosition = GlobalPosition.Lerp(desired, weight);
        LookAt(GlobalPosition - Offset(), Vector3.Up);
    }

    private Vector3 Offset()
    {
        float elevation = Mathf.DegToRad(Config.DownwardAngleDegrees);
        float azimuth = Mathf.DegToRad(Config.AzimuthDegrees);
        float horizontal = Config.CameraDistance * Mathf.Cos(elevation);
        return new Vector3(Mathf.Sin(azimuth) * horizontal,
            Mathf.Sin(elevation) * Config.CameraDistance,
            Mathf.Cos(azimuth) * horizontal);
    }
}

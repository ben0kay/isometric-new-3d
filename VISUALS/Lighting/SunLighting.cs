using Godot;

public partial class SunLighting : DirectionalLight3D
{
    [Export] public VisualConfig Config { get; set; }
    [Export] public WorldEnvironment EnvironmentNode { get; set; }

    public override void _Ready()
    {
        Config ??= GD.Load<VisualConfig>("res://VISUALS/VisualConfig.tres");
        float elevation = Mathf.DegToRad(Config.SunElevationDegrees);
        float azimuth = Mathf.DegToRad(Config.SunAzimuthDegrees);
        Vector3 direction = new(Mathf.Cos(elevation) * Mathf.Sin(azimuth),
            -Mathf.Sin(elevation), Mathf.Cos(elevation) * Mathf.Cos(azimuth));
        LookAt(GlobalPosition + direction, Vector3.Up);
        LightEnergy = Config.SunEnergy;
        ShadowEnabled = Config.SunShadows;
        if (EnvironmentNode?.Environment != null)
            EnvironmentNode.Environment.AmbientLightEnergy = Config.AmbientEnergy;
    }
}

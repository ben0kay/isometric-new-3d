using Godot;

// Fixed directional sun and ambient fill; shared config remains the single tuning source.
public partial class SunLighting : DirectionalLight3D
{
    #region References
    [Export] public VisualConfig Config { get; set; }
    [Export] public WorldEnvironment EnvironmentNode { get; set; }
    #endregion

    #region Lifecycle
    // =========================================================
    // Apply initial settings and respond to changes in the running config resource.
    public override void _Ready()
    {
        Config ??= GD.Load<VisualConfig>("res://VISUALS/VisualConfig.tres");
        Config.Changed += ApplySettings;
        ApplySettings();
    }

    // =========================================================
    // Release the resource signal when this world unloads.
    public override void _ExitTree()
    {
        if (Config != null) Config.Changed -= ApplySettings;
    }
    #endregion

    #region Lighting Configuration
    // =========================================================
    // Position has no effect on a directional sun; rotation fixes incoming light direction.
    private void ApplySettings()
    {
        float elevation = Mathf.DegToRad(Config.SunElevationDegrees);
        float azimuth = Mathf.DegToRad(Config.SunAzimuthDegrees);
        Vector3 direction = new(Mathf.Cos(elevation) * Mathf.Sin(azimuth),
            -Mathf.Sin(elevation), Mathf.Cos(elevation) * Mathf.Cos(azimuth));
        LookAt(GlobalPosition + direction, Vector3.Up);
        LightEnergy = Mathf.Clamp(Config.SunEnergy, 0f, 4f);
        LightColor = Config.SunColor;
        ShadowEnabled = Config.SunShadows;
        DirectionalShadowMaxDistance = Config.ShadowMaxDistance;
        ShadowBias = Config.ShadowBias;
        ShadowNormalBias = Config.ShadowNormalBias;
        ShadowBlur = Config.ShadowBlur;
        if (EnvironmentNode?.Environment != null) {
            EnvironmentNode.Environment.AmbientLightSource = Godot.Environment.AmbientSource.Color;
            EnvironmentNode.Environment.AmbientLightColor = Config.AmbientColor;
            EnvironmentNode.Environment.AmbientLightEnergy = Mathf.Clamp(Config.AmbientEnergy, 0f, 2f);
        }
    }
    #endregion
}

using Godot;

// One screen-space colour grade below the debug HUD, with no camera or terrain dependencies.
public partial class WorldGrade : CanvasLayer
{
    #region Configuration And State
    [Export] public VisualConfig Config { get; set; }
    private ShaderMaterial _material;
    #endregion

    #region Lifecycle
    // =========================================================
    // Build one non-interactive fullscreen canvas draw after 3D and before HUD layer 20.
    public override void _Ready()
    {
        Config ??= GD.Load<VisualConfig>("res://VISUALS/VisualConfig.tres");
        Layer = 10;
        _material = new ShaderMaterial {
            Shader = GD.Load<Shader>("res://VISUALS/PostProcessing/WorldGrade.gdshader")
        };
        var overlay = new ColorRect {
            Color = Colors.White, Material = _material,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        AddChild(overlay);
        overlay.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        Config.Changed += ApplySettings;
        ApplySettings();
    }

    // =========================================================
    // Disconnect shared resource notifications before leaving the scene.
    public override void _ExitTree()
    {
        if (Config != null) Config.Changed -= ApplySettings;
    }
    #endregion

    #region Grading Controls
    // =========================================================
    // The off switch skips the screen draw entirely for direct A/B comparison.
    private void ApplySettings()
    {
        Visible = Config.GradeEnabled;
        _material.SetShaderParameter("contrast", Mathf.Clamp(Config.GradeContrast, 0.8f, 1.4f));
        _material.SetShaderParameter("saturation", Mathf.Clamp(Config.GradeSaturation, 0.5f, 1.2f));
        _material.SetShaderParameter("shadow_lift", Mathf.Clamp(Config.GradeShadowLift, 0f, 0.05f));
        _material.SetShaderParameter("vignette", Mathf.Clamp(Config.GradeVignette, 0f, 0.2f));
    }
    #endregion
}

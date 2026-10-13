using Godot;

// Non-interactive top-right telemetry, refreshed at a modest rate.
public partial class Hud : CanvasLayer
{
    [Export] public DebugConfig Config { get; set; }
    [Export] public Node3D Target { get; set; }
    [Export] public ChunkManager Terrain { get; set; }
    private Label _label;
    private double _elapsed;

    // =========================================================
    // Anchor a readable panel to the viewport, independent of the world camera.
    public override void _Ready()
    {
        Config ??= GD.Load<DebugConfig>("res://DEBUG/DebugConfig.tres");
        Layer = 20;
        var root = new Control { MouseFilter = Control.MouseFilterEnum.Ignore };
        AddChild(root);
        root.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        var panel = new PanelContainer { MouseFilter = Control.MouseFilterEnum.Ignore };
        root.AddChild(panel);
        panel.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.TopRight);
        panel.OffsetLeft = -310; panel.OffsetTop = 16;
        panel.OffsetRight = -16; panel.OffsetBottom = 174;
        var style = new StyleBoxFlat {
            BgColor = new Color(0.025f, 0.045f, 0.065f, 0.9f),
            BorderColor = new Color(0.2f, 0.8f, 0.9f, 0.6f),
            BorderWidthLeft = 1, BorderWidthRight = 1, BorderWidthTop = 1, BorderWidthBottom = 1,
            ContentMarginLeft = 16, ContentMarginRight = 16,
            ContentMarginTop = 12, ContentMarginBottom = 12
        };
        panel.AddThemeStyleboxOverride("panel", style);
        _label = new Label { MouseFilter = Control.MouseFilterEnum.Ignore };
        _label.AddThemeFontSizeOverride("font_size", 18);
        _label.AddThemeColorOverride("font_color", new Color(0.85f, 0.97f, 1));
        panel.AddChild(_label);
        Refresh();
    }

    // =========================================================
    // Coordinates use the player's actual simulation position, in meters.
    public override void _Process(double delta)
    {
        Visible = Config.ShowHud;
        if (!Visible) return;
        _elapsed += delta;
        if (_elapsed < Mathf.Max(0.05f, Config.HudRefreshSeconds)) return;
        _elapsed = 0;
        Refresh();
    }

    // =========================================================
    // Read biome identity from the active world snapshot rather than a HUD constant.
    private void Refresh()
    {
        if (Target == null || Terrain?.GenerationSettings == null) { _label.Text = "WORLD DEBUG\nWaiting for terrain…"; return; }
        Vector3 p = Target.GlobalPosition;
        var settings = Terrain.GenerationSettings;
        var chunk = ChunkCoordinates.FromPosition(p, settings.ChunkSizeMeters);
        _label.Text = $"WORLD DEBUG\nX  {p.X:F1} m   Y  {p.Y:F1} m\nZ  {p.Z:F1} m\nBIOME  {settings.BiomeId}\nCHUNK  {chunk.X}, {chunk.Z}   LOADED  {Terrain.LoadedChunkCount}";
    }
}

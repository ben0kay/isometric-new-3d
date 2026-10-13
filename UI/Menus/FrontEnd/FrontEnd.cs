using Godot;
using System;

// Spatial menu host: camera travels through a lightweight 3D corridor while native 2D Control menus stay crisp.
public partial class FrontEnd : Node3D
{
    [Export] public UiConfig Config { get; set; } = null!;
    private Camera3D _camera = null!;
    private MainMenu _main = null!;
    private CampaignMenu _campaign = null!;
    private Control _blocker = null!;
    private bool _transitioning;

    // =========================================================
    // Keep a single 3D scene loaded; UI switches only after the actual camera move.
    public override void _Ready()
    {
        GetTree().Paused = false;
        Input.MouseMode = Input.MouseModeEnum.Visible;
        if (ProfileStore.Selected == null) {
            Callable.From(() => MenuNavigation.Open(this, MenuNavigation.BootScene)).CallDeferred();
            return;
        }
        Config ??= GD.Load<UiConfig>("res://UI/UiConfig.tres");
        _camera = GetNode<Camera3D>("MenuCamera");
        _main = GetNode<MainMenu>("UI/MainMenu");
        _campaign = GetNode<CampaignMenu>("UI/CampaignMenu");
        _blocker = GetNode<Control>("UI/TransitionBlocker");
        _camera.Position = Config.MainCameraPosition;
        _main.Build(this, Config);
        _campaign.Build(this, Config);
        _campaign.Hide();
        BuildScenery();
    }

    // =========================================================
    // Move the perspective camera forward; fade UI to avoid unreadable text in transit.
    public void OpenCampaign() => Travel(true);
    public void ReturnMain() => Travel(false);

    private void Travel(bool toCampaign)
    {
        if (_transitioning) return;
        _transitioning = true;
        _blocker.Show();
        Control departing = toCampaign ? _main : _campaign;
        Control arriving = toCampaign ? _campaign : _main;
        arriving.Hide();
        var tween = CreateTween();
        tween.SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
        tween.TweenProperty(_camera, "position",
            toCampaign ? Config.CampaignCameraPosition : Config.MainCameraPosition,
            Config.CameraTravelSeconds);
        tween.Parallel().TweenProperty(departing, "modulate:a", 0f, Config.FadeSeconds);
        tween.TweenCallback(Callable.From(() => {
            departing.Hide();
            arriving.Modulate = new Color(1, 1, 1, 0);
            arriving.Show();
            if (toCampaign) _campaign.Refresh();
        }));
        tween.TweenProperty(arriving, "modulate:a", 1f, Config.FadeSeconds);
        tween.Finished += () => { _blocker.Hide(); _transitioning = false; };
    }

    // =========================================================
    // Placeholder command-deck geometry creates real parallax; replace with a ship model later.
    private void BuildScenery()
    {
        var steel = new StandardMaterial3D {
            AlbedoColor = new Color("#102831"), Metallic = 0.55f, Roughness = 0.62f
        };
        var dark = new StandardMaterial3D {
            AlbedoColor = new Color("#071219"), Metallic = 0.4f, Roughness = 0.75f
        };
        var cyan = new StandardMaterial3D {
            AlbedoColor = new Color("#3ebcca"),
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded
        };
        var blue = new StandardMaterial3D {
            AlbedoColor = new Color("#164a6a"),
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded
        };
        AddBox(new Vector3(0, -2, 1), new Vector3(19, 0.35f, 65), dark);
        AddBox(new Vector3(-8.2f, -0.7f, 0), new Vector3(0.32f, 5.5f, 65), steel);
        AddBox(new Vector3(8.2f, -0.7f, 0), new Vector3(0.32f, 5.5f, 65), steel);
        for (int i = -2; i <= 5; i++) {
            float z = i * 6;
            AddBox(new Vector3(-6.6f, 0.4f, z), new Vector3(0.4f, 6.4f, 0.65f), steel);
            AddBox(new Vector3(6.6f, 0.4f, z), new Vector3(0.4f, 6.4f, 0.65f), steel);
            AddBox(new Vector3(-6.3f, 2.2f, z + 0.2f), new Vector3(0.12f, 2.6f, 0.12f), cyan);
            AddBox(new Vector3(6.3f, 2.2f, z + 0.2f), new Vector3(0.12f, 2.6f, 0.12f), cyan);
            AddBox(new Vector3(0, -1.78f, z), new Vector3(13, 0.05f, 0.12f), blue);
        }
        AddBox(new Vector3(-4.9f, -1.5f, 13), new Vector3(1.5f, 1.1f, 1.5f), steel);
        AddBox(new Vector3(-4.9f, -0.9f, 13.2f), new Vector3(1.2f, 0.08f, 1.1f), cyan);
        AddBox(new Vector3(4.9f, -1.5f, 2), new Vector3(1.5f, 1.1f, 1.5f), steel);
        AddBox(new Vector3(4.9f, -0.9f, 2.2f), new Vector3(1.2f, 0.08f, 1.1f), cyan);

        // Distant planet gives the menu a planetary sci-fi horizon rather than an empty corridor.
        var planetMaterial = new StandardMaterial3D {
            AlbedoColor = new Color("#264860"), Roughness = 1f
        };
        var sphere = new MeshInstance3D {
            Name = "DistantPlanet",
            Mesh = new SphereMesh { Radius = 10f, Height = 20f, RadialSegments = 48, Rings = 24 },
            MaterialOverride = planetMaterial, Position = new Vector3(13, 11, -62)
        };
        AddChild(sphere);
        var light = new OmniLight3D {
            Position = new Vector3(0, 2, 4), LightColor = new Color("#57d6e5"),
            LightEnergy = 0.45f, OmniRange = 18f
        };
        AddChild(light);
        var stars = new Random(917);
        for (int i = 0; i < 65; i++) {
            float x = (float)(stars.NextDouble() * 100 - 50);
            float y = (float)(stars.NextDouble() * 35 + 10);
            float z = (float)(stars.NextDouble() * 65 - 75);
            AddBox(new Vector3(x, y, z), Vector3.One * 0.075f, cyan);
        }
    }

    private void AddBox(Vector3 position, Vector3 size, Material material)
    {
        AddChild(new MeshInstance3D {
            Mesh = new BoxMesh { Size = size },
            MaterialOverride = material,
            Position = position
        });
    }
}

using Godot;

// Replicates the older 2D game's title/profile card/right-button layout over a real 3D camera.
public partial class MainMenu : Control
{
    [Export] public string GameTitle { get; set; } = "FRACTURED\nHORIZONS";
    [Export] public string Tagline { get; set; } = "SURVIVE  /  BUILD  /  EXPLORE  /  ENDURE";

    // =========================================================
    // Build once; main-to-campaign is handled by the shared 3D frontend.
    public void Build(FrontEnd front, UiConfig config)
    {
        var tint = new ColorRect {
            Color = new Color(0.012f, 0.035f, 0.05f, 0.30f),
            MouseFilter = MouseFilterEnum.Ignore
        };
        AddChild(tint);
        tint.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        var margin = new MarginContainer();
        foreach (string side in new[] { "left", "right", "top", "bottom" })
            margin.AddThemeConstantOverride("margin_" + side, 40);
        AddChild(margin);
        margin.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        var layout = new HBoxContainer();
        layout.AddThemeConstantOverride("separation", 32);
        margin.AddChild(layout);
        var left = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        left.AddThemeConstantOverride("separation", 18);
        layout.AddChild(left);
        left.AddChild(UIButtonFactory.Label(GameTitle, 58));
        left.AddChild(UIButtonFactory.Label(Tagline, 17));
        left.AddChild(new Control { SizeFlagsVertical = SizeFlags.ExpandFill });
        var profilePanel = new PanelContainer();
        profilePanel.AddThemeStyleboxOverride("panel", UIButtonFactory.Box(
            new Color(0.025f, 0.075f, 0.10f, 0.9f), UIButtonFactory.Accent));
        left.AddChild(profilePanel);
        var profileContent = new VBoxContainer();
        profilePanel.AddChild(profileContent);
        profileContent.AddChild(UIButtonFactory.Label("ACTIVE PROFILE", 14));
        profileContent.AddChild(UIButtonFactory.Label(ProfileStore.Selected?.Name ?? "Explorer", 26));
        profileContent.AddChild(UIButtonFactory.Label("A stranger world. A new beginning.", 16));

        var scroll = new ScrollContainer {
            CustomMinimumSize = new Vector2(config.MenuWidth, 0),
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled
        };
        layout.AddChild(scroll);
        var menu = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        menu.AddThemeConstantOverride("separation", 13);
        scroll.AddChild(menu);
        menu.AddChild(UIButtonFactory.Label("A 3D ISOMETRIC\nSURVIVAL EXPERIENCE", 18));
        menu.AddChild(UIButtonFactory.CreateMenu("Campaign", front.OpenCampaign, config.MenuButtonHeight));
        menu.AddChild(UIButtonFactory.CreateMenu("Sandbox", () =>
            MenuNavigation.Open(this, MenuNavigation.WorldScene), config.MenuButtonHeight));
        menu.AddChild(UIButtonFactory.CreateMenu("Change Profile", () => {
            ProfileStore.ClearSelection();
            MenuNavigation.Open(this, MenuNavigation.BootScene);
        }, config.MenuButtonHeight));
        menu.AddChild(UIButtonFactory.CreateMenu("Options", () => Message(
            "Options", "Options will be added in a later pass."), config.MenuButtonHeight));
        menu.AddChild(UIButtonFactory.CreateMenu("About", () => Message(
            "About", "A science-fiction survival world.\nA stranger world. A new beginning."),
            config.MenuButtonHeight));
        menu.AddChild(UIButtonFactory.CreateMenu("Exit", () => GetTree().Quit(), config.MenuButtonHeight));
        menu.AddChild(UIButtonFactory.Label("SAME SKY. DIFFERENT WORLD.", 14));
    }

    // =========================================================
    // Nonfunctional early-development sections should tell the player clearly.
    private void Message(string title, string message)
    {
        var dialog = new AcceptDialog { Title = title, DialogText = message };
        AddChild(dialog);
        dialog.PopupCentered(new Vector2I(480, 190));
        dialog.Confirmed += dialog.QueueFree;
    }
}

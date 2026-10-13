using Godot;
using System;

// Closely adapted from the 2D BootScreen: profile selection before the 3D menu.
public partial class BootScreen : Control
{
    [ExportGroup("Layout")]
    [Export] public float PanelWidth { get; set; } = 560f;
    [Export] public string Heading { get; set; } = "SELECT PROFILE";
    private VBoxContainer _list = null!;
    private Label _status = null!, _nameError = null!;
    private LineEdit _name = null!;
    private AcceptDialog _create = null!;

    // =========================================================
    // Do not boot into an uncontrolled world without an explicit identity.
    public override void _Ready()
    {
        GetTree().Paused = false;
        Input.MouseMode = Input.MouseModeEnum.Visible;
        BuildUi();
        Refresh();
    }

    // =========================================================
    // Reproduce the original cyan/ink screen and editable profile dialog.
    private void BuildUi()
    {
        var background = new ColorRect { Color = UIButtonFactory.Ink, MouseFilter = MouseFilterEnum.Ignore };
        AddChild(background);
        background.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        var outer = new MarginContainer();
        foreach (string side in new[] { "left", "right", "top", "bottom" })
            outer.AddThemeConstantOverride("margin_" + side, 32);
        AddChild(outer);
        outer.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        var center = new CenterContainer();
        outer.AddChild(center);
        var panel = new PanelContainer { CustomMinimumSize = new Vector2(PanelWidth, 0) };
        panel.AddThemeStyleboxOverride("panel", UIButtonFactory.Box(new Color("#0d2029"), UIButtonFactory.Accent));
        center.AddChild(panel);
        var contents = new VBoxContainer();
        contents.AddThemeConstantOverride("separation", 16);
        panel.AddChild(contents);
        contents.AddChild(UIButtonFactory.Label(Heading, 28));
        contents.AddChild(UIButtonFactory.Label("Choose who is exploring this world."));
        var scroll = new ScrollContainer {
            CustomMinimumSize = new Vector2(0, 260),
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled
        };
        contents.AddChild(scroll);
        _list = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        _list.AddThemeConstantOverride("separation", 8);
        scroll.AddChild(_list);
        contents.AddChild(UIButtonFactory.Create("NEW PROFILE", OpenCreate));
        contents.AddChild(UIButtonFactory.Create("EXIT", () => GetTree().Quit()));
        _status = UIButtonFactory.Label("");
        _status.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        contents.AddChild(_status);

        _create = new AcceptDialog { Title = "New Profile", Exclusive = true, DialogHideOnOk = false };
        AddChild(_create);
        var form = new VBoxContainer();
        form.AddThemeConstantOverride("separation", 12);
        _create.AddChild(form);
        form.AddChild(UIButtonFactory.Label("Profile name"));
        _name = new LineEdit { MaxLength = 32, PlaceholderText = "Enter your name" };
        form.AddChild(_name);
        _nameError = UIButtonFactory.Label("");
        form.AddChild(_nameError);
        _create.GetOkButton().Text = "SAVE";
        UIButtonFactory.Apply(_create.GetOkButton());
        UIButtonFactory.Apply(_create.AddCancelButton("CANCEL"));
        _create.Confirmed += Create;
        _name.TextSubmitted += _ => Create();
    }

    // =========================================================
    // Profile presence and corruption/recovery never depend on game-world startup.
    private void Refresh()
    {
        foreach (Node node in _list.GetChildren()) { _list.RemoveChild(node); node.QueueFree(); }
        try {
            foreach (var profile in ProfileStore.Profiles) {
                string id = profile.Id;
                _list.AddChild(UIButtonFactory.Create(profile.Name, () => Select(id)));
            }
            if (ProfileStore.Profiles.Count == 0)
                _list.AddChild(UIButtonFactory.Label("No profiles yet. Create your first one."));
            _status.Text = ProfileStore.RecoveryMessage;
        } catch (Exception e) { _status.Text = e.Message; }
    }

    // =========================================================
    // Enter the spatial frontend without replacing or resetting campaign data.
    private void Select(string id)
    {
        try { ProfileStore.Select(id); MenuNavigation.Open(this, MenuNavigation.FrontEndScene); }
        catch (Exception e) { _status.Text = e.Message; }
    }

    private void OpenCreate()
    {
        _name.Text = "";
        _nameError.Text = "";
        _create.PopupCentered(new Vector2I(440, 210));
        _name.GrabFocus();
    }

    // =========================================================
    // Profile is saved before the frontend can become active.
    private void Create()
    {
        try { ProfileStore.Create(_name.Text); }
        catch (Exception e) { _nameError.Text = e.Message; return; }
        _create.Hide();
        try { MenuNavigation.Open(this, MenuNavigation.FrontEndScene); }
        catch (Exception e) { _status.Text = e.Message; Refresh(); }
    }
}

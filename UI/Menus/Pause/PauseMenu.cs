using Godot;
using System;

// 2D-style pause overlay shared by campaign and sandbox scenes.
public partial class PauseMenu : CanvasLayer
{
    private Control _overlay = null!;
    private Button _save = null!, _resume = null!;
    private AcceptDialog _message = null!;
    private ConfirmationDialog _exit = null!;
    private Action? _saveAction;
    private bool _open;
    private bool _exitGame;
    private Input.MouseModeEnum _oldMouse;

    // =========================================================
    // This CanvasLayer must remain responsive when the world scene tree is paused.
    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        BuildUi();
        _overlay.Hide();
    }

    // =========================================================
    // Gameplay input is stopped by the scene-tree pause, without touching player animation code.
    public override void _UnhandledInput(InputEvent input)
    {
        if (input is not InputEventKey key || !key.Pressed || key.Echo ||
            (key.Keycode != Key.Escape && key.PhysicalKeycode != Key.Escape)) return;
        GetViewport().SetInputAsHandled();
        if (_open) Resume();
        else Open();
    }

    // =========================================================
    // Campaign mode enables saving; sandbox testing deliberately leaves it disabled.
    public void BindSave(Action save)
    {
        _saveAction = save;
        if (_save != null) {
            _save.Disabled = save == null;
            _save.TooltipText = save == null ? "Saving is available in a campaign." : "Save this campaign.";
        }
    }

    // =========================================================
    // Backup restoration is visible to the player, not hidden in console output.
    public void ShowRecoveryNotice(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;
        if (!_open) Open();
        ShowMessage("Campaign recovered", message);
    }

    // =========================================================
    // Reuse the original translucent cyan-on-dark 2D pause menu.
    private void BuildUi()
    {
        _overlay = new Control { MouseFilter = Control.MouseFilterEnum.Stop };
        AddChild(_overlay);
        _overlay.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        var dim = new ColorRect {
            Color = new Color(0.015f, 0.035f, 0.05f, 0.82f),
            MouseFilter = Control.MouseFilterEnum.Stop
        };
        _overlay.AddChild(dim);
        dim.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        var center = new CenterContainer();
        _overlay.AddChild(center);
        center.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        var panel = new PanelContainer { CustomMinimumSize = new Vector2(440, 0) };
        panel.AddThemeStyleboxOverride("panel", UIButtonFactory.Box(UIButtonFactory.Ink, UIButtonFactory.Accent));
        center.AddChild(panel);
        var contents = new VBoxContainer();
        contents.AddThemeConstantOverride("separation", 12);
        panel.AddChild(contents);
        contents.AddChild(UIButtonFactory.Label("PAUSED", 30));
        _resume = UIButtonFactory.Create("Resume", Resume);
        contents.AddChild(_resume);
        _save = UIButtonFactory.Create("Save Game", Save);
        contents.AddChild(_save);
        contents.AddChild(UIButtonFactory.Create("Options", () =>
            ShowMessage("Options", "Options will be added in a later pass.")));
        contents.AddChild(UIButtonFactory.Create("Exit to Main Menu", () => ConfirmExit(false)));
        contents.AddChild(UIButtonFactory.Create("Exit Game", () => ConfirmExit(true)));
        BindSave(null!);
        _message = new AcceptDialog { Exclusive = true };
        AddChild(_message);
        UIButtonFactory.Apply(_message.GetOkButton());
        _exit = new ConfirmationDialog { Title = "Leave game?", Exclusive = true };
        AddChild(_exit);
        _exit.GetOkButton().Text = "LEAVE";
        UIButtonFactory.Apply(_exit.GetOkButton());
        UIButtonFactory.Apply(_exit.GetCancelButton());
        _exit.Confirmed += ExitConfirmed;
    }

    private void Open()
    {
        if (_open) return;
        _open = true;
        _oldMouse = Input.MouseMode;
        Input.MouseMode = Input.MouseModeEnum.Visible;
        _overlay.Show();
        GetTree().Paused = true;
        _resume.GrabFocus();
    }

    private void Resume()
    {
        if (!_open) return;
        _message.Hide();
        _exit.Hide();
        _overlay.Hide();
        _open = false;
        Input.MouseMode = _oldMouse;
        GetTree().Paused = false;
    }

    // =========================================================
    // A failed save leaves the game paused and displays the real error.
    private void Save()
    {
        if (_saveAction == null) return;
        try {
            _saveAction();
            ShowMessage("Campaign saved", "World settings and 3D player position saved.");
        } catch (Exception e) {
            ShowMessage("Save failed", e.Message);
        }
    }

    private void ConfirmExit(bool quit)
    {
        _exitGame = quit;
        _exit.DialogText = "Leave this game? Unsaved progress will be lost.";
        _exit.PopupCentered(new Vector2I(460, 180));
    }

    // =========================================================
    // Return to the active profile's spatial menu, or profile selection in sandbox mode.
    private void ExitConfirmed()
    {
        try {
            if (_exitGame) { GetTree().Paused = false; GetTree().Quit(); }
            else MenuNavigation.Open(this, ProfileStore.Selected == null ?
                MenuNavigation.BootScene : MenuNavigation.FrontEndScene);
        } catch (Exception e) { ShowMessage("Exit failed", e.Message); }
    }

    private void ShowMessage(string title, string message)
    {
        _message.Title = title;
        _message.DialogText = message;
        _message.PopupCentered(new Vector2I(480, 180));
    }

    // =========================================================
    // Scene changes must never leave the newly loaded menu paused.
    public override void _ExitTree()
    {
        if (_open && IsInsideTree()) GetTree().Paused = false;
    }
}

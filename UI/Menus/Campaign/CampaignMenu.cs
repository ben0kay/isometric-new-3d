using Godot;
using System;
using System.Linq;

// Campaign-specific page at the end of the spatial camera move; multiple campaigns per profile.
public partial class CampaignMenu : Control
{
    private FrontEnd _front = null!;
    private VBoxContainer _menu = null!, _saveList = null!;
    private ScrollContainer _loadPanel = null!;
    private Button _continue = null!;
    private ConfirmationDialog _newDialog = null!;
    private LineEdit _campaignName = null!;
    private AcceptDialog _message = null!;
    private UiConfig _config = null!;

    // =========================================================
    // Keep the Campaign page's layout and button styling faithful to the old main menu.
    public void Build(FrontEnd front, UiConfig config)
    {
        _front = front; _config = config;
        var dim = new ColorRect {
            Color = new Color(0.012f, 0.035f, 0.05f, 0.43f),
            MouseFilter = MouseFilterEnum.Ignore
        };
        AddChild(dim); dim.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        var margin = new MarginContainer();
        foreach (string side in new[] { "left", "right", "top", "bottom" })
            margin.AddThemeConstantOverride("margin_" + side, 40);
        AddChild(margin); margin.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        var layout = new HBoxContainer();
        layout.AddThemeConstantOverride("separation", 32);
        margin.AddChild(layout);
        var left = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        layout.AddChild(left);
        left.AddChild(UIButtonFactory.Label("CAMPAIGN\nARCHIVES", 58));
        left.AddChild(UIButtonFactory.Label("EXPLORE AN UNKNOWN WORLD", 17));
        left.AddChild(new Control { SizeFlagsVertical = SizeFlags.ExpandFill });
        var card = new PanelContainer();
        card.AddThemeStyleboxOverride("panel", UIButtonFactory.Box(
            new Color(0.025f, 0.075f, 0.10f, 0.9f), UIButtonFactory.Accent));
        left.AddChild(card);
        var profile = new VBoxContainer();
        card.AddChild(profile);
        profile.AddChild(UIButtonFactory.Label("ACTIVE EXPLORER", 14));
        profile.AddChild(UIButtonFactory.Label(ProfileStore.Selected?.Name ?? "Explorer", 26));
        var scroll = new ScrollContainer {
            CustomMinimumSize = new Vector2(config.MenuWidth, 0),
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled
        };
        layout.AddChild(scroll);
        var pages = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        scroll.AddChild(pages);
        _menu = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        _menu.AddThemeConstantOverride("separation", 13);
        pages.AddChild(_menu);
        _menu.AddChild(UIButtonFactory.Label("YOUR EXPEDITIONS", 20));
        _continue = UIButtonFactory.CreateMenu("Continue", ContinueLatest, config.MenuButtonHeight);
        _menu.AddChild(_continue);
        _menu.AddChild(UIButtonFactory.CreateMenu("New Campaign", ShowNew, config.MenuButtonHeight));
        _menu.AddChild(UIButtonFactory.CreateMenu("Load Campaign", ShowLoad, config.MenuButtonHeight));
        _menu.AddChild(UIButtonFactory.CreateMenu("Back", front.ReturnMain, config.MenuButtonHeight));

        _loadPanel = new ScrollContainer {
            CustomMinimumSize = new Vector2(config.MenuWidth, 300),
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled
        };
        pages.AddChild(_loadPanel);
        _saveList = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        _saveList.AddThemeConstantOverride("separation", 10);
        _loadPanel.AddChild(_saveList);
        _loadPanel.Hide();

        _newDialog = new ConfirmationDialog { Title = "New Campaign", Exclusive = true };
        AddChild(_newDialog);
        _newDialog.GetOkButton().Text = "CREATE";
        UIButtonFactory.Apply(_newDialog.GetOkButton());
        UIButtonFactory.Apply(_newDialog.GetCancelButton());
        var form = new VBoxContainer();
        form.AddThemeConstantOverride("separation", 12);
        _newDialog.AddChild(form);
        form.AddChild(UIButtonFactory.Label("Expedition name"));
        _campaignName = new LineEdit { MaxLength = 64, PlaceholderText = "New Expedition" };
        form.AddChild(_campaignName);
        _newDialog.Confirmed += CreateCampaign;
        _message = new AcceptDialog();
        AddChild(_message);
        UIButtonFactory.Apply(_message.GetOkButton());
    }

    // =========================================================
    // The Continue button always reflects the selected profile's existing saves.
    public void Refresh()
    {
        try {
            _continue.Disabled = CampaignStore.List(ProfileStore.Selected!.Id).Count == 0;
            _menu.Show();
            _loadPanel.Hide();
        } catch (Exception e) { ShowError(e); }
    }

    // =========================================================
    // Let the user name the world; never silently replace another campaign.
    private void ShowNew()
    {
        _campaignName.Text = "Expedition " + (CampaignStore.List(ProfileStore.Selected!.Id).Count + 1);
        _newDialog.PopupCentered(new Vector2I(480, 220));
        _campaignName.GrabFocus();
    }

    private void CreateCampaign()
    {
        try {
            string name = _campaignName.Text.Trim();
            if (string.IsNullOrWhiteSpace(name)) throw new InvalidOperationException("Enter a campaign name.");
            var rng = new RandomNumberGenerator(); rng.Randomize();
            var data = new CampaignData {
                ProfileId = ProfileStore.Selected!.Id,
                CampaignId = Guid.NewGuid().ToString("N"),
                Name = name, CreatedUtc = DateTime.UtcNow, SavedUtc = DateTime.UtcNow,
                Generation = WorldGenerationSettings.Capture(
                    GD.Load<WorldConfig>("res://WORLD/WorldConfig.tres")),
                PlayerX = 0f, PlayerY = 1f, PlayerZ = 0f
            };
            CampaignSession.Launch(this, data, true);
        } catch (Exception e) { ShowError(e); }
    }

    // =========================================================
    // Continue selects the last saved campaign; Load lists all valid campaign identities.
    private void ContinueLatest()
    {
        try {
            var entry = CampaignStore.List(ProfileStore.Selected!.Id).FirstOrDefault();
            if (entry == null) throw new InvalidOperationException("No saved campaigns are available.");
            CampaignSession.Launch(this, CampaignStore.Load(entry.ProfileId, entry.CampaignId), false);
        } catch (Exception e) { ShowError(e); }
    }

    private void ShowLoad()
    {
        foreach (Node node in _saveList.GetChildren()) { _saveList.RemoveChild(node); node.QueueFree(); }
        _saveList.AddChild(UIButtonFactory.Label("LOAD CAMPAIGN", 24));
        try {
            var campaigns = CampaignStore.List(ProfileStore.Selected!.Id);
            if (campaigns.Count == 0) _saveList.AddChild(UIButtonFactory.Label("No campaigns saved yet."));
            foreach (var data in campaigns) {
                string id = data.CampaignId;
                string label = $"{data.Name}  |  {data.SavedUtc.ToLocalTime():yyyy-MM-dd HH:mm}";
                _saveList.AddChild(UIButtonFactory.Create(label, () => {
                    try {
                        CampaignSession.Launch(this,
                            CampaignStore.Load(ProfileStore.Selected!.Id, id), false);
                    } catch (Exception e) { ShowError(e); }
                }, 60));
            }
        } catch (Exception e) { ShowError(e); }
        _saveList.AddChild(UIButtonFactory.Create("BACK", () => {
            _loadPanel.Hide(); _menu.Show();
        }));
        _menu.Hide();
        _loadPanel.Show();
    }

    // =========================================================
    // Never swallow a failed save or scene transition.
    private void ShowError(Exception error)
    {
        _message.Title = "Campaign operation failed";
        _message.DialogText = error.Message;
        _message.PopupCentered(new Vector2I(550, 210));
    }
}

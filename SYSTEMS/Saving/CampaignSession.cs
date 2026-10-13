using Godot;
using System;

// A world session owns this world's save data; separate files own profiles and storage.
public partial class CampaignSession : Node
{
    private CampaignData _data = null!;
    private Player _player = null!;

    // =========================================================
    // Set the saved generation recipe and 3D spawn BEFORE entering the scene tree.
    public static void Launch(Node source, CampaignData data, bool newCampaign)
    {
        data.Validate(ProfileStore.Selected?.Id);
        var packed = GD.Load<PackedScene>(MenuNavigation.WorldScene)
            ?? throw new InvalidOperationException("The world scene could not be loaded.");
        var world = packed.Instantiate<Node3D>();
        try {
            var chunks = world.GetNode<ChunkManager>("Terrain");
            var player = world.GetNode<Player>("Player");
            chunks.UseSavedSettings(data.Generation);
            player.Position = new Vector3(data.PlayerX, data.PlayerY, data.PlayerZ);
            world.AddChild(new CampaignSession { Name = "CampaignSession", _data = data });
            // Persist an initial save slot before entering gameplay, without affecting other campaigns.
            if (newCampaign) CampaignStore.Write(data);
        } catch {
            world.Free();
            throw;
        }
        var tree = source.GetTree();
        Callable.From(() => {
            tree.Paused = false;
            var old = tree.CurrentScene;
            tree.Root.AddChild(world);
            tree.CurrentScene = world;
            old?.QueueFree();
        }).CallDeferred();
    }

    // =========================================================
    // The existing ChunkManager already creates a collision-safe ring in _Ready.
    // This runs after the world scene's Terrain and Player nodes have initialized.
    public override void _Ready()
    {
        _player = GetParent().GetNode<Player>("Player");
        var pause = GetParent().GetNode<PauseMenu>("PauseMenu");
        var settings = GD.Load<SystemConfig>("res://SYSTEMS/SystemConfig.tres");
        if (settings == null || settings.AllowManualSaving) pause.BindSave(Save);
        if (!string.IsNullOrWhiteSpace(CampaignStore.RecoveryMessage))
            pause.ShowRecoveryNotice(CampaignStore.RecoveryMessage);
    }

    // =========================================================
    // Store only serializable state: permanent world recipe plus 3D player location.
    public void Save()
    {
        Vector3 position = _player.GlobalPosition;
        if (!float.IsFinite(position.X) || !float.IsFinite(position.Y) || !float.IsFinite(position.Z))
            throw new InvalidOperationException("The player position cannot be saved.");
        _data.PlayerX = position.X;
        _data.PlayerY = position.Y;
        _data.PlayerZ = position.Z;
        _data.SavedUtc = DateTime.UtcNow;
        CampaignStore.Write(_data);
        GD.Print($"[Campaign] Saved {_data.CampaignId} at {position}");
    }
}

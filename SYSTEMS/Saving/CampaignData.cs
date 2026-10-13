using System;

// Only state currently present in the 3D prototype is stored in version 1.
public sealed class CampaignData
{
    public int Version { get; set; } = 1;
    public string Coverage { get; set; } = "world-player-only";
    public string ProfileId { get; set; } = "";
    public string CampaignId { get; set; } = "";
    public string Name { get; set; } = "New Campaign";
    public DateTime CreatedUtc { get; set; }
    public DateTime SavedUtc { get; set; }
    public WorldGenerationSettings Generation { get; set; } = new();
    public float PlayerX { get; set; }
    public float PlayerY { get; set; } = 1f;
    public float PlayerZ { get; set; }

    // =========================================================
    // Future saves extend the version/schema instead of serializing Godot scenes.
    public void Validate(string? expectedProfile = null, string? expectedCampaign = null)
    {
        if (Version != 1 || Coverage != "world-player-only" ||
            !Guid.TryParseExact(ProfileId, "N", out _) ||
            !Guid.TryParseExact(CampaignId, "N", out _) ||
            (expectedProfile != null && ProfileId != expectedProfile) ||
            (expectedCampaign != null && CampaignId != expectedCampaign) ||
            string.IsNullOrWhiteSpace(Name) || Name.Length > 64 || Generation == null ||
            !float.IsFinite(PlayerX) || !float.IsFinite(PlayerY) || !float.IsFinite(PlayerZ))
            throw new InvalidOperationException("Invalid or unsupported campaign save.");
        Generation.Validate();
    }
}

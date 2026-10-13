using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

// Per-profile, multi-campaign persistence with independent files and backups.
public static class CampaignStore
{
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };
    public static string RecoveryMessage { get; private set; } = "";

    // =========================================================
    // Only stable GUIDs are permitted in local save paths.
    public static string PathFor(string profileId, string campaignId)
    {
        if (!Guid.TryParseExact(profileId, "N", out _) || !Guid.TryParseExact(campaignId, "N", out _))
            throw new InvalidDataException("Invalid profile/campaign ID.");
        return ProjectSettings.GlobalizePath($"user://Profiles/{profileId}/Campaigns/{campaignId}.json");
    }

    // =========================================================
    // Never deserialize untrusted Godot object graphs; accept the versioned pure-data schema.
    private static CampaignData Read(string path, string profileId, string campaignId)
    {
        var data = JsonSerializer.Deserialize<CampaignData>(SaveFile.ReadText(path))
            ?? throw new InvalidDataException("Empty campaign save.");
        data.Validate(profileId, campaignId);
        return data;
    }

    // =========================================================
    // Revert to a previous valid save if the primary cannot be read.
    public static CampaignData Load(string profileId, string campaignId)
    {
        RecoveryMessage = "";
        string path = PathFor(profileId, campaignId);
        try { return Read(path, profileId, campaignId); }
        catch (Exception error) {
            try {
                var backup = Read(path + ".bak", profileId, campaignId);
                RecoveryMessage = "Recovered an earlier campaign save from backup.";
                return backup;
            } catch {
                throw new IOException("Campaign and backup could not be loaded; files remain intact. " + error.Message, error);
            }
        }
    }

    // =========================================================
    // Enumerate valid campaign identities without opening arbitrary paths.
    public static IReadOnlyList<CampaignData> List(string profileId)
    {
        string dummy = PathFor(profileId, "00000000000000000000000000000000");
        string directory = Path.GetDirectoryName(dummy)!;
        if (!Directory.Exists(directory)) return Array.Empty<CampaignData>();
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (string file in Directory.EnumerateFiles(directory, "*.json*")) {
            string name = Path.GetFileName(file);
            string id = name.EndsWith(".json.bak", StringComparison.Ordinal)
                ? name[..^9] : name.EndsWith(".json", StringComparison.Ordinal) ? name[..^5] : "";
            if (Guid.TryParseExact(id, "N", out _)) ids.Add(id);
        }
        var results = new List<CampaignData>();
        foreach (string id in ids) {
            try { results.Add(Load(profileId, id)); } catch (Exception e) { GD.PushWarning($"Unreadable campaign {id}: {e.Message}"); }
        }
        return results.OrderByDescending(x => x.SavedUtc).ToArray();
    }

    // =========================================================
    // A new record never overwrites an existing campaign for the same profile.
    public static void Write(CampaignData data)
    {
        data.Validate();
        string path = PathFor(data.ProfileId, data.CampaignId);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(data, Json);
        SaveFile.Write(path, bytes, file => {
            try { Read(file, data.ProfileId, data.CampaignId); return true; }
            catch { return false; }
        });
        RecoveryMessage = "";
    }
}

using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

// Mirrors the 2D game's local profile identity model, independent of campaigns.
public sealed class PlayerProfile
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public DateTime CreatedUtc { get; set; }
}

public static class ProfileStore
{
    private sealed class Index {
        public int Version { get; set; } = 1;
        public List<PlayerProfile> Profiles { get; set; } = new();
    }
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };
    private static List<PlayerProfile>? _profiles;
    public static PlayerProfile? Selected { get; private set; }
    public static string RecoveryMessage { get; private set; } = "";
    public static IReadOnlyList<PlayerProfile> Profiles { get { Load(); return _profiles!.AsReadOnly(); } }
    private static string Path => ProjectSettings.GlobalizePath("user://Profiles/profiles.json");

    // =========================================================
    // Load a versioned profile index; recover a valid backup but never overwrite damaged files.
    public static void Load()
    {
        if (_profiles != null) return;
        if (!File.Exists(Path) && !File.Exists(Path + ".bak")) { _profiles = new(); return; }
        try { _profiles = Read(Path); }
        catch (Exception error) {
            try { _profiles = Read(Path + ".bak"); RecoveryMessage = "Recovered profiles from backup."; }
            catch { throw new IOException("Profiles and backup could not be opened. Files remain intact. " + error.Message, error); }
        }
    }

    // =========================================================
    // Check format, unique stable identities, and names before accepting them.
    private static List<PlayerProfile> Read(string path)
    {
        var index = JsonSerializer.Deserialize<Index>(SaveFile.ReadText(path));
        if (index?.Version != 1 || index.Profiles == null || index.Profiles.Count > 1024)
            throw new InvalidDataException("Invalid profile index.");
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var profile in index.Profiles)
            if (profile == null || !Guid.TryParseExact(profile.Id, "N", out _) ||
                !ids.Add(profile.Id) || !ValidName(profile.Name))
                throw new InvalidDataException("Invalid profile entry.");
        return index.Profiles;
    }

    // =========================================================
    // Keep display names independent of on-disk paths.
    public static void Select(string id)
    {
        Load();
        Selected = _profiles!.FirstOrDefault(p => p.Id == id)
            ?? throw new ArgumentException("The selected profile no longer exists.");
    }

    // =========================================================
    // The next front-end visit requires an explicit profile selection.
    public static void ClearSelection() => Selected = null;

    // =========================================================
    // Persist before announcing the newly created profile to UI.
    public static PlayerProfile Create(string name)
    {
        Load();
        name = (name ?? "").Trim();
        if (!ValidName(name)) throw new ArgumentException("Use 1–32 characters without control characters.");
        if (_profiles!.Any(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException("That profile name already exists.");
        var created = new PlayerProfile { Id = Guid.NewGuid().ToString("N"), Name = name, CreatedUtc = DateTime.UtcNow };
        var updated = new List<PlayerProfile>(_profiles) { created };
        var bytes = JsonSerializer.SerializeToUtf8Bytes(new Index { Profiles = updated }, Json);
        SaveFile.Write(Path, bytes, p => TryRead(p));
        _profiles = updated;
        Selected = created;
        RecoveryMessage = "";
        return created;
    }

    private static bool TryRead(string path) { try { Read(path); return true; } catch { return false; } }
    private static bool ValidName(string? name) =>
        !string.IsNullOrWhiteSpace(name) && name.Length <= 32 && !name.Any(char.IsControl);
}

using Godot;
using System;
using System.IO;

// File transaction helper shared by the profile index and campaign records.
public static class SaveFile
{
    // =========================================================
    // Never replace a valid backup with a corrupt old primary. Preserve broken primaries.
    public static void Write(string path, byte[] bytes, Func<string, bool> isValid)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        string temp = path + ".tmp";
        using (var stream = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.None)) {
            stream.Write(bytes);
            stream.Flush(true);
        }
        if (!isValid(temp)) throw new InvalidDataException("Save did not pass validation.");
        if (!File.Exists(path)) {
            File.Move(temp, path);
            return;
        }
        if (isValid(path)) {
            File.Replace(temp, path, path + ".bak");
        } else {
            string quarantine = path + ".corrupt-" + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
            File.Move(path, quarantine);
            File.Move(temp, path);
        }
    }

    // =========================================================
    // Limit the amount of malformed file data read into memory.
    public static string ReadText(string path, long maxBytes = 32L * 1024 * 1024)
    {
        if (new FileInfo(path).Length > maxBytes)
            throw new InvalidDataException("Save exceeds the permitted size.");
        return File.ReadAllText(path);
    }
}

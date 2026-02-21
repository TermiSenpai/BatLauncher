using System.IO;
using System.Text.Json;
using BatLauncher.Models;

namespace BatLauncher.Services;

public static class DataService
{
    private static readonly string AppFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "BatLauncher");

    private static readonly string DataFile = Path.Combine(AppFolder, "data.json");
    private static readonly string IconsFolder = Path.Combine(AppFolder, "icons");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static string GetIconsFolder()
    {
        Directory.CreateDirectory(IconsFolder);
        return IconsFolder;
    }

    public static AppData Load()
    {
        try
        {
            if (!File.Exists(DataFile))
                return new AppData();

            var json = File.ReadAllText(DataFile);
            return JsonSerializer.Deserialize<AppData>(json, JsonOptions) ?? new AppData();
        }
        catch
        {
            return new AppData();
        }
    }

    public static void Save(AppData data)
    {
        Directory.CreateDirectory(AppFolder);
        var json = JsonSerializer.Serialize(data, JsonOptions);
        File.WriteAllText(DataFile, json);
    }

    public static string? CopyIcon(string sourcePath, string entryId)
    {
        try
        {
            Directory.CreateDirectory(IconsFolder);
            var ext = Path.GetExtension(sourcePath);
            var destPath = Path.Combine(IconsFolder, $"{entryId}{ext}");
            File.Copy(sourcePath, destPath, true);
            return destPath;
        }
        catch
        {
            return null;
        }
    }

    public static void DeleteIcon(string? iconPath)
    {
        try
        {
            if (!string.IsNullOrEmpty(iconPath) && File.Exists(iconPath))
                File.Delete(iconPath);
        }
        catch { }
    }
}

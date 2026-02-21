using System.Text.Json.Serialization;

namespace BatLauncher.Models;

public class AppData
{
    [JsonPropertyName("version")]
    public int Version { get; set; } = 1;

    [JsonPropertyName("entries")]
    public List<BatEntry> Entries { get; set; } = [];
}

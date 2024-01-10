using System.Text.Json;

namespace BGME.BattleThemes.Utils;

internal static class ReloadedConfigParser
{
    public static ReloadedConfig Parse(string file)
    {
        try
        {
            return JsonSerializer.Deserialize<ReloadedConfig>(File.ReadAllText(file)) ?? throw new Exception();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to parse mod config.");
            return new() { ModId = "BGME.BattleThemes" };
        }
    }
}

internal class ReloadedConfig
{
    public string ModId { get; set; } = string.Empty;
}

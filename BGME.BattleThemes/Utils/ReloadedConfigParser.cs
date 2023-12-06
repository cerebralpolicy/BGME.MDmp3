using System.Text.Json;

namespace BGME.BattleThemes.Utils;

internal static class ReloadedConfigParser
{
    public static ReloadedConfig Parse(string file)
        => JsonSerializer.Deserialize<ReloadedConfig>(File.ReadAllText(file))!;
}

internal class ReloadedConfig
{
    public string ModId { get; set; } = string.Empty;
}

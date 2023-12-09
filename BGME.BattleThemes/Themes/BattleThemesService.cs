using BGME.BattleThemes.Interfaces;
using BGME.Framework.Interfaces;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using System.Text;
using System.Text.RegularExpressions;

namespace BGME.BattleThemes.Themes;

internal class BattleThemesService : IBattleThemesApi
{
    private readonly IModLoader modLoader;
    private readonly IBgmeApi bgme;
    private readonly MusicRegistry musicRegistry;

    private readonly HashSet<ThemePath> themePaths = new();
    private readonly List<string> themes = new();
    private readonly StringBuilder musicScriptBuilder = new();
    private Func<string>? themeScriptCallback;

    private bool hasLoaded = false;

    public BattleThemesService(
        IModLoader modLoader,
        IBgmeApi bgme,
        MusicRegistry musicRegistry)
    {
        this.modLoader = modLoader;
        this.bgme = bgme;
        this.musicRegistry = musicRegistry;

        this.modLoader.ModLoading += this.OnModLoading;
        this.modLoader.OnModLoaderInitialized += this.ApplyThemeScript;
    }

    public void AddPath(string modId, string path)
    {
        this.themePaths.Add(new(modId, path));
        if (this.hasLoaded)
        {
            this.ApplyThemeScript();
        }

        Log.Debug($"Added theme path.\nPath: {path}");
    }

    public void RemovePath(string path)
    {
        if (this.themePaths.FirstOrDefault(x => x.Path == path) is ThemePath themePath)
        {
            this.themePaths.Remove(themePath);
            this.ApplyThemeScript();
            Log.Debug($"Removed theme path.\nPath: {path}");
        }
        else
        {
            Log.Debug($"Could not find theme path to remove.\nPath: {path}");
        }
    }

    private void OnModLoading(IModV1 mod, IModConfigV1 config)
    {
        if (!config.ModDependencies.Contains("BGME.BattleThemes"))
        {
            return;
        }

        var modDir = this.modLoader.GetDirectoryForModId(config.ModId);
        var battleThemesDir = Path.Join(modDir, "battle-themes");
        if (Directory.Exists(battleThemesDir))
        {
            this.AddPath(config.ModId, battleThemesDir);
        }
    }

    private void ApplyThemeScript()
    {
        if (this.themeScriptCallback != null)
        {
            this.bgme.RemoveMusicScript(this.themeScriptCallback);
            this.musicScriptBuilder.Clear();
            this.themes.Clear();
        }

        foreach (var theme in this.themePaths)
        {
            if (theme.IsFile)
            {
                this.ProcessFile(theme.ModId, theme.Path);
            }
            else
            {
                this.ProcessFolder(theme.ModId, theme.Path);
            }
        }

        if (this.themes.Count == 0)
        {
            return;
        }

        musicScriptBuilder.AppendLine($"const allThemes = [{string.Join(',', this.themes)}]");
        musicScriptBuilder.AppendLine($"const randomizedThemes = random_music(allThemes)");
        musicScriptBuilder.AppendLine("encounter[\"Normal Battles\"]:");
        musicScriptBuilder.AppendLine("  music = randomizedThemes");
        musicScriptBuilder.AppendLine("end");

        var musicScript = this.musicScriptBuilder.ToString();
        this.themeScriptCallback = () => musicScript;
        this.bgme.AddMusicScript(this.themeScriptCallback);
        Log.Debug($"Battle Theme Script:\n{musicScript}");

        this.hasLoaded = true;
    }

    private void ProcessFile(string modId, string filePath)
    {
        try
        {
            var modSongs = this.musicRegistry.GetModSongs(modId);
            var musicScriptText = File.ReadAllText(filePath);
            foreach (var song in modSongs)
            {
                var pattern = $@"\b({song.Name})\b";
                musicScriptText = Regex.Replace(musicScriptText, pattern, song.BgmId.ToString());
            }

            var uniqueThemeId = $"theme_{this.themes.Count}";
            musicScriptText = musicScriptText.Replace("BATTLE_THEME", uniqueThemeId);
            this.themes.Add(uniqueThemeId);
            this.musicScriptBuilder.AppendLine(musicScriptText);

            Log.Information($"Added battle theme from {modId}: {Path.GetFileName(filePath)}");
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Failed to add battle theme from {modId}.\nFile: {filePath}");
        }
    }

    private void ProcessFolder(string modId, string folderPath)
    {
        foreach (var themeFile in Directory.EnumerateFiles(folderPath, "*.theme.pme", SearchOption.TopDirectoryOnly))
        {
            this.ProcessFile(modId, themeFile);
        }
    }

    private record ThemePath(string ModId, string Path)
    {
        public bool IsFile { get; } = File.Exists(Path);
    }
}

using BGME.BattleThemes.Interfaces;
using BGME.Framework.Interfaces;
using DynamicData;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BGME.BattleThemes.Themes;

internal class BattleThemesService : IBattleThemesApi
{
    private readonly IModLoader modLoader;
    private readonly IBgmeApi bgme;
    private readonly MusicRegistry musicRegistry;

    private readonly SourceCache<ThemePath, string> themePaths = new(x => x.Path);

    private readonly List<string> themes = new();
    private readonly StringBuilder musicScriptBuilder = new();
    private Func<string>? themeScriptCallback;

    public BattleThemesService(
        IModLoader modLoader,
        IBgmeApi bgme,
        MusicRegistry musicRegistry)
    {
        this.modLoader = modLoader;
        this.bgme = bgme;
        this.musicRegistry = musicRegistry;

        this.themePaths.Connect()
            .Throttle(TimeSpan.FromMilliseconds(250))
            .Subscribe(x =>
            {
                this.ApplyThemeScript();
            });

        this.modLoader.ModLoading += this.OnModLoading;
    }

    public void AddPath(string modId, string path)
    {
        this.themePaths.AddOrUpdate(new ThemePath(modId, path));
        Log.Debug($"Added theme path.\nPath: {path}");
    }

    public void RemovePath(string path)
    {
        if (this.themePaths.Items.FirstOrDefault(x => x.Path == path) is ThemePath themePath)
        {
            this.themePaths.Remove(themePath);
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

        foreach (var theme in this.themePaths.Items)
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

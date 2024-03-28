using BGME.MDmp3.Interfaces;
using BGME.Framework.Interfaces;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using Timer = System.Timers.Timer;
using BGME.MDmp3.Utils;

namespace BGME.MDmp3.Playlists;
internal class PlaylistService : IMDmp3API
{
    private readonly IModLoader modLoader;
    private readonly IBgmeApi bgme;
    private readonly SongRegistry songRegistry;

    private readonly ObservableCollection<PlaylistPath> playlistPaths = new();
    private readonly List<string> playlists = new();
    private readonly StringBuilder musicScriptBuilder = new();
    private Func<string> playlistScriptCallback = () => string.Empty;
    private readonly Timer playlistsRefreshTimer;

    public PlaylistService(
        IModLoader modLoader,
        IBgmeApi bgme,
        SongRegistry songRegistry)
    {
        this.modLoader = modLoader;
        this.bgme = bgme;
        this.songRegistry = songRegistry;

        playlistsRefreshTimer = new(TimeSpan.FromMilliseconds(250))
        {
            AutoReset = false,
        };

        // Add temp callback to retain mod priority
        // in music scripts.
        bgme.AddMusicScript(playlistScriptCallback);

        playlistsRefreshTimer.Elapsed += (sender, args) => ApplyThemeScript();
        playlistPaths.CollectionChanged += (sender, args) =>
        {
            playlistsRefreshTimer.Stop();
            playlistsRefreshTimer.Start();
        };

        modLoader.ModLoading += OnModLoading;
    }

    public void AddPath(string modId, string path, string type)
    {
        var playlistPath = new PlaylistPath(modId, path, type);
        if (!playlistPaths.Contains(playlistPath))
        {
            playlistPaths.Add(new PlaylistPath(modId, path, type));
            Log.Debug($"Added playlist path.\nPath: {path}");
        }
    }

    public void RemovePath(string path)
    {
        if (playlistPaths.FirstOrDefault(x => x.Path == path) is PlaylistPath playlistPath)
        {
            playlistPaths.Remove(playlistPath);
            Log.Debug($"Removed playlist path.\nPath: {path}");
        }
        else
        {
            Log.Verbose($"Could not find playlist path to remove.\nPath: {path}");
        }
    }

    private void OnModLoading(IModV1 mod, IModConfigV1 config)
    {
        if (!config.ModDependencies.Contains("BGME.MDmp3"))
        {
            return;
        }


        var modDir = modLoader.GetDirectoryForModId(config.ModId);

        var _definitionDir = PlaylistCategory.GetCatDir(modDir, "_library_");

        if (Directory.Exists(_definitionDir))
        {
            AddPath(config.ModId, _definitionDir, "library");
        }
    }

    private void ApplyThemeScript()
    {
        musicScriptBuilder.Clear();
        playlists.Clear();

        foreach (var playlist in playlistPaths)
        {
            if (playlist.IsFile)
            {
                ProcessFile(playlist.ModId, playlist.Path, playlist.Type);
            }
            else
            {
                ProcessFolder(playlist.ModId, playlist.Path, playlist.Type);
            }
        }

        if (playlists.Count == 0)
        {
            return;
        }

        musicScriptBuilder.AppendLine($"const allThemes = [{string.Join(',', playlists)}]");
        musicScriptBuilder.AppendLine($"const randomizedThemes = random_music(allThemes)");
        musicScriptBuilder.AppendLine("encounter[\"Normal Battles\"]:");
        musicScriptBuilder.AppendLine("  music = randomizedThemes");
        musicScriptBuilder.AppendLine("end");

        var musicScript = musicScriptBuilder.ToString();
        string newCallback() => musicScript;

        // Replace current callback with new one.
        bgme.AddMusicScript(newCallback, playlistScriptCallback);
        playlistScriptCallback = newCallback;

        Log.Debug($"Battle Themes Script:\n{musicScript}");
    }

    private void ProcessFile(string modId, string filePath, string type)
    {
        try
        {
            var modSongs = songRegistry.GetModSongs(modId);
            var musicScriptText = File.ReadAllText(filePath);
            foreach (var song in modSongs)
            {
                var pattern = $@"\b({song.Name})\b";
                musicScriptText = Regex.Replace(musicScriptText, pattern, song.BgmId.ToString());
            }
            if (type == "combat")
            {
                var uniqueThemeId = $"battleplaylist_{playlists.Count}";
                musicScriptText = musicScriptText.Replace("BATTLE_THEME", uniqueThemeId);
                playlists.Add(uniqueThemeId); // Allows for blank themes
            }
            musicScriptBuilder.AppendLine(musicScriptText);

            Log.Information($"Added {type.ToUpper()} playlist from {modId}: {Path.GetFileName(filePath)}");
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Failed to add {type.ToUpper()} playlist from {modId}.\nFile: {filePath}");
        }
    }

    private void ProcessFolder(string modId, string folderPath, string type)
    {
        foreach (var playlistFile in Directory.EnumerateFiles(folderPath, "*.playlist.pme", SearchOption.TopDirectoryOnly))
        {
            ProcessFile(modId, playlistFile, type);
        }
    }

    private record PlaylistPath(string ModId, string Path, string Type)
    {
        public bool IsFile { get; } = File.Exists(Path);
    }
}
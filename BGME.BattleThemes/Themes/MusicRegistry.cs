using BGME.BattleThemes.Utils;
using PersonaModdingMetadata.Shared.Games;
using Phos.MusicManager.Library.Audio.Encoders;
using Phos.MusicManager.Library.Audio.Encoders.VgAudio;

namespace BGME.BattleThemes.Themes;

internal class MusicRegistry
{
    private readonly Game game;
    private readonly Configuration.Config config;
    private readonly bool devMode;
    private readonly List<ModSong> songs = new();
    private readonly Dictionary<Game, IEncoder> encoders = new();
    private readonly string[] supportedExts;

    public MusicRegistry(
        Game game,
        Configuration.Config config,
        string baseDir)
    {
        this.game = game;
        this.config = config;
        this.devMode = File.Exists(Path.Join(baseDir, "battle-themes", "dev.json"));
        if (this.devMode)
        {
            Log.Information("Developer Mode Enabled. Songs files will always be built.");
        }

        var cachedDir = new DirectoryInfo(Path.Join(game.GameFolder(baseDir), "cached"));
        cachedDir.Create();
        this.encoders[Game.P4G_PC] = new CachedEncoder(new VgAudioEncoder(new() { OutContainerFormat = "hca" }), cachedDir.FullName);
        this.encoders[Game.P3P_PC] = new CachedEncoder(new VgAudioEncoder(new() { OutContainerFormat = "adx" }), cachedDir.FullName);
        this.encoders[Game.P5R_PC] = new CachedEncoder(new VgAudioEncoder(new() { OutContainerFormat = "adx", KeyCode = 9923540143823782 }), cachedDir.FullName);
        this.supportedExts = this.encoders.First().Value.InputTypes;

        this.RegisterMusic(Path.GetDirectoryName(baseDir)!);
    }

    /// <summary>
    /// Gets the list of songs added by the specified mod.
    /// </summary>
    /// <param name="modId">Mod ID to get songs for.</param>
    /// <returns>Array of songs.</returns>
    public ModSong[] GetModSongs(string modId) => this.songs.Where(x => x.ModId == modId).ToArray();

    private void RegisterMusic(string modsDir)
    {
        foreach (var modDir in Directory.EnumerateDirectories(modsDir))
        {
            var modConfigFile = Path.Join(modDir, "ModConfig.json");
            if (!File.Exists(modConfigFile))
            {
                continue;
            }

            var modConfig = ReloadedConfigParser.Parse(modConfigFile);
            this.RegisterModMusic(modConfig.ModId, modDir);
        }
    }

    private void RegisterModMusic(string modId, string modDir)
    {
        var battleThemesDir = Path.Join(modDir, "battle-themes");
        if (!Directory.Exists(battleThemesDir))
        {
            return;
        }

        var musicDir = Path.Join(battleThemesDir, "music");
        if (!Directory.Exists(musicDir))
        {
            return;
        }

        var songs = Directory.GetFiles(musicDir, "*", SearchOption.AllDirectories)
            .Where(file => this.supportedExts
            .Contains(Path.GetExtension(file), StringComparer.OrdinalIgnoreCase))
            .Select(file =>
            {
                var bgmId = this.GetNextBgmId();
                var buildFile = Path.Join(modDir, this.GetReplacementPath(bgmId));
                var song = new ModSong(modId, Path.GetFileNameWithoutExtension(file), bgmId, file, buildFile);
                this.songs.Add(song);
                return song;
            })
            .ToArray();

        Task.WhenAll(songs.Select(this.RegisterSong)).Wait();
    }

    private async Task RegisterSong(ModSong song)
    {
        await this.BuildSong(song);
        Log.Information($"Registered Song: {song.Name} || Mod: {song.ModId} || BGM ID: {song.BgmId}");
    }

    private async Task BuildSong(ModSong song)
    {
        Log.Debug($"Building song: {song.FilePath}");

        var outputFile = new FileInfo(song.BuildFilePath);
        if (outputFile.Exists && !this.devMode)
        {
            Log.Debug($"Song already built.");
        }
        else
        {
            outputFile.Directory!.Create();
            var encoder = this.encoders[this.game];
            await encoder.Encode(song.FilePath, outputFile.FullName);
        }

        Log.Debug($"Built song: {song.BuildFilePath}");
    }

    private string GetReplacementPath(int bgmId) => this.game switch
    {
        Game.P3P_PC => Path.Join("P5REssentials/CPK/Battle Themes/data/sound/bgm", $"{bgmId}.adx"),
        Game.P4G_PC => Path.Join("FEmulator/AWB/snd00_bgm.awb", $"{bgmId}.hca"),
        Game.P5R_PC => Path.Join("FEmulator/AWB/BGM_42.AWB", $"{bgmId - 10000}.adx"),
        _ => throw new Exception("Unknown game."),
    };

    private int GetNextBgmId() => this.GetBaseBgmId() + this.songs.Count;

    private int GetBaseBgmId() => this.game switch
    {
        Game.P3P_PC => this.config.BaseBgmId_P3P,
        Game.P4G_PC => this.config.BaseBgmId_P4G,
        Game.P5R_PC => this.config.BaseBgmId_P5R,
        _ => throw new Exception("Unknown game."),
    };
}

internal record ModSong(string ModId, string Name, int BgmId, string FilePath, string BuildFilePath);
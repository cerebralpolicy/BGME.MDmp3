using BGME.BattleThemes.Configuration;
using BGME.BattleThemes.Interfaces;
using BGME.BattleThemes.Template;
using BGME.BattleThemes.Themes;
using BGME.Framework.Interfaces;
using PersonaModdingMetadata.Shared.Games;
using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using System.Diagnostics;

namespace BGME.BattleThemes;

public class Mod : ModBase, IExports
{
    private readonly IModLoader modLoader;
    private readonly IReloadedHooks? hooks;
    private readonly ILogger logger;
    private readonly IMod owner;
    private Config configuration;
    private readonly IModConfig modConfig;

    private readonly Game game;
    private readonly BattleThemesService? battleThemesService;

    public Mod(ModContext context)
    {
        this.modLoader = context.ModLoader;
        this.hooks = context.Hooks;
        this.logger = context.Logger;
        this.owner = context.Owner;
        this.configuration = context.Configuration;
        this.modConfig = context.ModConfig;

#if DEBUG
        Debugger.Launch();
#endif

        Log.Logger = this.logger;
        Log.LogLevel = this.configuration.LogLevel;

        this.modLoader.GetController<IBgmeApi>().TryGetTarget(out var bgme);

        try
        {
            this.game = this.GetGame();
            var baseDir = modLoader.GetDirectoryForModId(this.modConfig.ModId);
            var musicRegistry = new MusicRegistry(this.game, this.configuration, baseDir);
            this.battleThemesService = new(this.modLoader, bgme!, musicRegistry);
            this.modLoader.AddOrReplaceController<IBattleThemesApi>(this.owner, this.battleThemesService);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to start battle themes service.");
        }
    }

    private Game GetGame()
    {
        var appId = this.modLoader.GetAppConfig().AppId.ToLower();
        return appId switch
        {
            "p5r.exe" => Game.P5R_PC,
            "p4g.exe" => Game.P4G_PC,
            "p3p.exe" => Game.P3P_PC,
            _ => Game.P5R_PC,
        };
    }

    #region Standard Overrides
    public override void ConfigurationUpdated(Config configuration)
    {
        // Apply settings from configuration.
        // ... your code here.
        this.configuration = configuration;
        logger.WriteLine($"[{modConfig.ModId}] Config Updated: Applying");
    }
    #endregion

    #region For Exports, Serialization etc.

    public Type[] GetTypes() => new[] { typeof(IBgmeApi) };

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Mod() { }
#pragma warning restore CS8618
    #endregion
}

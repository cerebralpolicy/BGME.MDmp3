using BGME.Framework.Interfaces;
using BGME.MDmp3.Interfaces;
using BGME.MDmp3.Template;
using BGME.MDmp3.Playlists;
using PersonaModdingMetadata.Shared.Games;
using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using System.Diagnostics;

namespace BGME.MDmp3
{
    /// <summary>
    /// Your mod logic goes here.
    /// </summary>
    public class Mod : ModBase, IExports // <= Do not Remove.
    {
        /// <summary>
        /// Provides access to the mod loader API.
        /// </summary>
        private readonly IModLoader _modLoader;

        /// <summary>
        /// Provides access to the Reloaded.Hooks API.
        /// </summary>
        /// <remarks>This is null if you remove dependency on Reloaded.SharedLib.Hooks in your mod.</remarks>
        private readonly IReloadedHooks? _hooks;

        /// <summary>
        /// Provides access to the Reloaded logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Entry point into the mod, instance that created this class.
        /// </summary>
        private readonly IMod _owner;

        /// <summary>
        /// Provides access to this mod's configuration.
        /// </summary>
        private Config _configuration;

        /// <summary>
        /// The configuration of the currently executing mod.
        /// </summary>
        private readonly IModConfig _modConfig;

        private readonly Game _game;
        private readonly PlaylistService _playlistService;

        public Mod(ModContext context)
        {
            _modLoader = context.ModLoader;
            _hooks = context.Hooks;
            _logger = context.Logger;
            _owner = context.Owner;
            _configuration = context.Configuration;
            _modConfig = context.ModConfig;



            // For more information about this template, please see
            // https://reloaded-project.github.io/Reloaded-II/ModTemplate/

            // If you want to implement e.g. unload support in your mod,
            // and some other neat features, override the methods in ModBase.

            // TODO: Implement some mod logic
#if DEBUG
            Debugger.Launch();
#endif

            Log.Logger = _logger;
            Log.LogLevel = _configuration.LogLevel;

            _modLoader.GetController<IBgmeApi>().TryGetTarget(out var bgme);

            try
            {
                _game = GetGame();
                var baseDir = _modLoader.GetDirectoryForModId(_modConfig.ModId);
                var musicRegistry = new SongRegistry(_game, _configuration, baseDir, _modLoader.GetAppConfig().EnabledMods);
                _playlistService = new(_modLoader, bgme!, musicRegistry);
                _modLoader.AddOrReplaceController<IMDmp3API>(_owner, _playlistService);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to start battle themes service.");
            }
        }

        private Game GetGame()
        {
            var appId = _modLoader.GetAppConfig().AppId;
            if (appId.Contains("p3r"))
                return Game.P3R_PC;
/*
            else if (appId.Contains("p5r"))
                return Game.P5R_PC;
            else if (appId.Contains("p4g"))
                return Game.P4G_PC;
            else if (appId.Contains("p3p"))
                return Game.P3P_PC;
*/
            throw new Exception($"Unknown game: {appId}");
        }

        #region Standard Overrides
        public override void ConfigurationUpdated(Config configuration)
        {
            // Apply settings from configuration.
            // ... your code here.
            _configuration = configuration;
            _logger.WriteLine($"[{_modConfig.ModId}] Config Updated: Applying");
        }
        #endregion

        #region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Type[] GetTypes() => new[] { typeof(IMDmp3API) };
        public Mod() { }
#pragma warning restore CS8618
        #endregion
    }
}
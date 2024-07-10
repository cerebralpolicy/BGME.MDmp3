using BGME.MDmp3.Interfaces;
using Reloaded.Mod.Interfaces;
using System.Drawing;

namespace BGME.MDmp3.Config
{
    public class PlaylistConfig
    {
        private readonly string modId;
        private readonly string modDir;
        private readonly ILogger log;
        private readonly IMDmp3API playlistsApi;
        private readonly Dictionary<PlaylistSetting, Func<bool>> settings = new();

        private IUpdatableConfigurable config;
        /// <summary>
        /// Create a playlist config.
        /// </summary>
        /// <param name="modLoader">Mod loader.</param>
        /// <param name="modConfig">Mod config.</param>
        /// <param name="config">Mod configuration containing the playlist settings.</param>
        /// <param name="log">Logger.</param>
        public PlaylistConfig(
            IModLoader modLoader,
            IModConfig modConfig,
            IUpdatableConfigurable config,
            ILogger log)
        {
            modId = modConfig.ModId;
            modDir = modLoader.GetDirectoryForModId(modConfig.ModId);
            this.config = config;
            this.log = log;

            modLoader.GetController<IMDmp3API>().TryGetTarget(out playlistsApi!);
            this.config.ConfigurationUpdated += OnConfigurationUpdated;
        }
        public void Initialize()
        {
            ApplySettings();
        }

        public void AddSetting(string propertyName, string playlistFileName, string playlistType)
        {
            var configType = config.GetType();
            var configProp = configType.GetProperty(propertyName, typeof(bool));
            if (configProp == null)
            {
                log.WriteLine($"[PlaylistConfig] Config missing bool property: {propertyName}", Color.Red);
                return;
            }

            var playlistFile = Path.Join(modDir, "MDmp3", "playlist", playlistType, playlistFileName);
            if (!File.Exists(playlistFile))
            {
                log.WriteLine($"[ThemeConfig] Theme file not found.\nFile: {playlistFile}", Color.Red);
                return;
            }

            var option = new PlaylistSetting(propertyName, playlistFile, playlistType);
            settings[option] = () =>
            {
                var enabled = (bool)(configProp.GetValue(config) ?? false);
                return enabled;
            };
        }

        private void ApplySettings()
        {
            foreach (var setting in settings)
            {
                setting.Key.Deconstruct(out var propertyName, out var playlistFile, out var playlistType);
                var enabled = setting.Value();

                if (enabled)
                {
                    playlistsApi.AddPath(modId, playlistFile, playlistType);
                }
                else
                {
                    playlistsApi.RemovePath(playlistFile);
                }

                log.WriteLine($"[ThemeConfig] \"{propertyName}\": {(enabled ? "Enabled" : "Disabled")}");
            }
        }

        private void OnConfigurationUpdated(IUpdatableConfigurable config)
        {
            this.config = config;
            ApplySettings();
        }

        private record PlaylistSetting(string PropertyName, string PlaylistFile, string playlistType);
    }
}

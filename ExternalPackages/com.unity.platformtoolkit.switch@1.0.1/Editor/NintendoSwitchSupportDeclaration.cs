using System.Collections.Generic;
using Unity.PlatformToolkit.Editor;
using UnityEditor;

namespace Unity.PlatformToolkit.nintendoswitch.Editor
{
    internal class NintendoSwitchSupportDeclaration : IPlatformToolkitSupportDeclaration
    {
        private static readonly BuildTarget[] k_SupportedBuildTargets = new[]
        {
            BuildTarget.Switch,
            BuildTarget.Switch2
        };

        public string DisplayName => "Nintendo Switch™";
        public string Key => "Unity.Switch";

        public IReadOnlyCollection<BuildTarget> SupportedPlatforms => k_SupportedBuildTargets;

        private readonly NintendoSwitchPlatformToolkitSettingsProvider m_SettingsProvider = new();
        public IPlatformToolkitSettingsProvider SettingsProvider => m_SettingsProvider;

        public IPlatformToolkitBuilder CreateBuilder(IAchievementConfigurationContext _, ISettingsConfigurationContext settingsContext)
        {
            var settingsConfiguration = m_SettingsProvider.CreateSettingsConfiguration(settingsContext);
            var settings = (NintendoSwitchPlatformToolkitSettings)settingsConfiguration.Settings;
            return new NintendoSwitchBuilder(settings, SupportedPlatforms);
        }
    }
}

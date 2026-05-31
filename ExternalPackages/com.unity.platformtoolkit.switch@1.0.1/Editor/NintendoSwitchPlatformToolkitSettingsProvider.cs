using System;
using Unity.PlatformToolkit.Editor;

namespace Unity.PlatformToolkit.nintendoswitch.Editor
{
    internal class NintendoSwitchPlatformToolkitSettingsProvider : IPlatformToolkitSettingsProvider
    {
        public Type SettingsType => typeof(INintendoSwitchPlatformToolkitSettings);

        public ISettingsConfiguration CreateSettingsConfiguration(ISettingsConfigurationContext context)
        {
            return new NintendoSwitchSettingsConfiguration(context);
        }
    }
}

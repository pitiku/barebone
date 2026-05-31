using Unity.PlatformToolkit.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.PlatformToolkit.nintendoswitch.Editor
{
    internal class NintendoSwitchSettingsConfiguration : ISettingsConfiguration
    {
        private readonly ISettingsConfigurationContext m_Context;
        private readonly NintendoSwitchPlatformToolkitSettings m_Settings;
        public object Settings => m_Settings;

        public NintendoSwitchSettingsConfiguration(ISettingsConfigurationContext context)
        {
            m_Context = context;
            if (!context.TryGetSerializedSettings(out var settings))
            {
                m_Settings = new();
            }
            else
            {
                m_Settings = JsonUtility.FromJson<NintendoSwitchPlatformToolkitSettings>(settings);
            }

            m_Settings.SwitchAttributeSettings.SettingsChanged += SaveSettings;
        }

        private void SaveSettings()
        {
            m_Context.SetSerializedSettings(JsonUtility.ToJson(m_Settings));
        }

        public VisualElement CreateSettingsUI()
        {
            return new AttributeSettingsField(m_Settings.SwitchAttributeSettings);
        }
    }
}

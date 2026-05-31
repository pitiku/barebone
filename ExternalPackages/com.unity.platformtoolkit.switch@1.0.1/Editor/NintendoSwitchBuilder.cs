using System;
using System.Collections.Generic;
using System.Linq;
using Unity.PlatformToolkit.Editor;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Unity.PlatformToolkit.nintendoswitch.Editor
{
    internal class NintendoSwitchBuilder : IPlatformToolkitBuilder
    {
        private readonly IReadOnlyCollection<BuildTarget> m_SupportedTargets;
        private readonly NintendoSwitchPlatformToolkitSettings m_Settings;

        public NintendoSwitchBuilder(NintendoSwitchPlatformToolkitSettings settings, IReadOnlyCollection<BuildTarget> supportedTargets)
        {
            m_Settings = settings;
            m_SupportedTargets = supportedTargets;
        }

        public void PostBuild(BuildReport buildReport) { }

        BaseRuntimeConfiguration IPlatformToolkitBuilder.PrepareBuild(BuildReport buildReport)
        {
            if (!m_SupportedTargets.Contains(buildReport.summary.platform))
                throw new InvalidOperationException($"Attempting to build for the switch implementation when we are on an unsupported build target '{buildReport.summary.platform}'");

#if UNITY_SWITCH || UNITY_SWITCH2
            var runtimeConfig = ScriptableObject.CreateInstance<SwitchRuntimeConfiguration>();
            runtimeConfig.Attributes = m_Settings.SwitchAttributeSettings.BuildAttributes();
            return runtimeConfig;
#else
            Debug.LogWarning($"{nameof(IPlatformToolkitBuilder.PrepareBuild)} called when UNITY_SWITCH or UNITY_SWITCH2 is not defined. This will result in PlatformToolkit for Nintendo Switch failing to initialize as a runtime configuration will not exist.");
            return null;
#endif
        }
    }
}

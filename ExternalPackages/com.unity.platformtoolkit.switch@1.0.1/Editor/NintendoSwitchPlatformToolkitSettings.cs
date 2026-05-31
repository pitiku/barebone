using System;
using System.Collections.Generic;
using Unity.PlatformToolkit.Editor;
using UnityEngine;

namespace Unity.PlatformToolkit.nintendoswitch.Editor
{
    [Serializable]
    internal class NintendoSwitchPlatformToolkitSettings : INintendoSwitchPlatformToolkitSettings
    {
        [SerializeField]
        private SwitchAttributeSettings m_AttributeSettings = new();

        public IAttributeSettings AttributeSettings => m_AttributeSettings;
        public SwitchAttributeSettings SwitchAttributeSettings => m_AttributeSettings;
    }

    [Serializable]
    internal class SwitchAttributeSettings : AttributeSettings
    {
        protected override void InitializeAttributes(
            out IReadOnlyList<(string AttributeId, Type AttributeType, string AttributeName)> attributeDefinitions)
        {
            attributeDefinitions = new List<(string attributeId, Type attributeType, string attributeName)>()
            {
                ("Nickname", typeof(string), "Nickname"),
                ("ProfileImage", typeof(Texture2D), "Profile Image"),
                ("UserID", typeof(nn.account.Uid), "User ID"),
                ("UserHandle", typeof(nn.account.UserHandle), "User Handle"),
            };
        }
    }
}

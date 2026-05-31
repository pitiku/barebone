using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;

namespace Unity.PlatformToolkit.nintendoswitch
{
    class SwitchPlatformToolkit : IPlatformToolkit
    {
        public ICapabilities Capabilities { get; private set; }
        private readonly SwitchAccountSystemManager m_SwitchAccountSystemManager;

        public SwitchPlatformToolkit(AttributeStore attributes)
        {
            m_SwitchAccountSystemManager = new(attributes);
        }

        public IAccountSystem Accounts => m_SwitchAccountSystemManager.AccountSystem;

#pragma warning disable CS1998
        public async Task Initialize()
#pragma warning restore CS1998
        {
            bool primarySupport = m_SwitchAccountSystemManager.Initialize();

            var capabilityBuilder = new CapabilityBuilder
            {
                AccountSupport = true,
                PrimaryAccount = primarySupport,
                PrimaryAccountEstablishLimited = false,
                AccountName = true,
                AccountPicture = true,
                AccountAchievementSystem = false,
                AccountSavingSystem = true,
                AccountManualSignOut = true,
                AccountInputPairingSystem = false,
                AdditionalAccountSystem = true,
                LocalSavingSystem = false
            };

            Capabilities = capabilityBuilder.ToCapabilities();
        }
    }
}
